using System.Reflection;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ActionConstraints;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.EntityFrameworkCore;
using test_api.Data;
using test_api.Models.Dtos;
using test_api.Models.Entities;
using test_api.Models.Responses;

namespace test_api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RoutesController : ControllerBase
    {
        private readonly IActionDescriptorCollectionProvider _actionDescriptorCollectionProvider;
        private readonly StoreContext _context;

        public RoutesController(IActionDescriptorCollectionProvider actionDescriptorCollectionProvider, StoreContext context)
        {
            _actionDescriptorCollectionProvider = actionDescriptorCollectionProvider;
            _context = context;
        }

        [HttpGet]
        public IActionResult GetAllRoutes()
        {
            var routes = _actionDescriptorCollectionProvider.ActionDescriptors.Items
                .Where(static x => x.AttributeRouteInfo != null)
                .Select(static x => new
                {
                    Method = x.ActionConstraints?.OfType<HttpMethodActionConstraint>()
                        .FirstOrDefault()?.HttpMethods.FirstOrDefault() ?? "GET",
                    Path = "/" + x.AttributeRouteInfo.Template,
                    Controller = x.RouteValues["Controller"],
                    Action = x.RouteValues["Action"],
                    Name = x.DisplayName
                })
                .OrderBy(x => x.Path)
                .ToList();

            return Ok(new
            {
                Success = true,
                Message = "All API routes retrieved successfully",
                Data = routes,
                Total = routes.Count
            });
        }

        [HttpGet("from-database")]
        public async Task<ActionResult<GetAllActionsPermissionsResponse>> GetRoutesFromDatabase()
        {
            var permissions = await Task.FromResult(_context.ActionPermissions
                .OrderBy(static p => p.Path)
                .ThenBy(static p => p.Method)
                .ToList());

            var response = new GetAllActionsPermissionsResponse
            {
                Success = true,
                Message = "All action permissions retrieved successfully",
                Data = permissions,
                Total = permissions.Count
            };

            return Ok(response);
        }

        [HttpGet("all-rules")]
        public IActionResult GetAllRules()
        {
            var rules = _context.Roles
                .Select(static r => new
                {
                    r.Id,
                    r.Name,
                    r.Description,
                    Permissions = r.ActionPermissions.Select(static p => new
                    {
                        p.Id,
                        p.Name,
                        p.Description,
                        p.Path,
                        p.Method
                    }).ToList()
                })
                .ToList();

            return Ok(new
            {
                Success = true,
                Message = "All rules retrieved successfully",
                Data = rules,
                Total = rules.Count
            });

        }

        [HttpPost("add-permission-to-role")]
        public async Task<IActionResult> AddPermissionToRole([FromBody] AddPermissionToRoleRequest request)
        {
            Console.WriteLine($"Received request to add permission to role: {request?.RoleId}, {request?.ActionPermissionId}");
            if (request == null || request.RoleId <= 0 || request.ActionPermissionId <= 0)
            {
                return BadRequest(new
                {
                    Success = false,
                    Message = "Invalid request data"
                });
            }

            // Kiểm tra role có tồn tại không
            var roleExists = await _context.Roles.AnyAsync(r => r.Id == request.RoleId);
            if (!roleExists)
            {
                return NotFound(new
                {
                    Success = false,
                    Message = "Role not found"
                });
            }

            // Kiểm tra permission có tồn tại không
            var permissionExists = await _context.ActionPermissions.AnyAsync(p => p.Id == request.ActionPermissionId);
            if (!permissionExists)
            {
                return NotFound(new
                {
                    Success = false,
                    Message = "Permission not found"
                });
            }

            // Kiểm tra relationship đã tồn tại chưa
            var relationshipExists = await _context.Roles
                .Where(r => r.Id == request.RoleId)
                .SelectMany(r => r.ActionPermissions)
                .AnyAsync(p => p.Id == request.ActionPermissionId);

            if (relationshipExists)
            {
                return Ok(new
                {
                    Success = true,
                    Message = "Permission already exists in this role"
                });
            }

            // Load role với ActionPermissions collection
            var role = await _context.Roles
                .Include(r => r.ActionPermissions)
                .FirstAsync(r => r.Id == request.RoleId);
            var permission = await _context.ActionPermissions.FirstAsync(p => p.Id == request.ActionPermissionId);

            role.ActionPermissions.Add(permission);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                Success = true,
                Message = "Permission added to role successfully",
                Data = new
                {
                    RoleId = role.Id,
                    RoleName = role.Name,
                    PermissionId = permission.Id,
                    PermissionName = permission.Name
                }
            });

        }

        [HttpPost("remove-permission-from-role")]
        public async Task<IActionResult> RemovePermissionFromRole([FromBody] AddPermissionToRoleRequest request)
        {
            Console.WriteLine($"Received request to remove permission from role: {request?.RoleId}, {request?.ActionPermissionId}");
            if (request == null || request.RoleId <= 0 || request.ActionPermissionId <= 0)
            {
                return BadRequest(new
                {
                    Success = false,
                    Message = "Invalid request data"
                });
            }

            // Kiểm tra role có tồn tại không
            bool roleExists = await _context.Roles.AnyAsync(r => r.Id == request.RoleId);
            if (!roleExists)
            {
                return NotFound(new
                {
                    Success = false,
                    Message = "Role not found"
                });
            }

            // Kiểm tra permission có tồn tại không
            bool permissionExists = await _context.ActionPermissions.AnyAsync(p => p.Id == request.ActionPermissionId);
            if (!permissionExists)
            {
                return NotFound(new
                {
                    Success = false,
                    Message = "Permission not found"
                });
            }

            // Load role với ActionPermissions collection
            Rule role = await _context.Roles
                .Include(r => r.ActionPermissions)
                .FirstAsync(r => r.Id == request.RoleId);

            // Kiểm tra relationship có tồn tại không
            ActionPermission? permissionToRemove = role.ActionPermissions.FirstOrDefault(p => p.Id == request.ActionPermissionId);
            if (permissionToRemove == null)
            {
                return Ok(new
                {
                    Success = true,
                    Message = "Permission does not exist in this role"
                });
            }

            _ = role.ActionPermissions.Remove(permissionToRemove);
            _ = await _context.SaveChangesAsync();

            return Ok(new
            {
                Success = true,
                Message = "Permission removed from role successfully",
                Data = new
                {
                    RoleId = role.Id,
                    RoleName = role.Name,
                    PermissionId = permissionToRemove.Id,
                    PermissionName = permissionToRemove.Name
                }
            });
        }
    }
}
