using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using test_api.Data;
using test_api.Helpers;
using test_api.Middleware;
using test_api.Middlewares;
using test_api.Services;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(8080);
});
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Test API",
        Version = "v1",
        Description = "A simple API for testing purposes"
    });


    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter 'Bearer' [space] and then your token in the text input"
    });

    options.OperationFilter<DynamicAuthOperationFilter>();

});
builder.Services.AddControllers();
builder.Services.AddDbContext<StoreContext>(options =>
{
    _ = options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
    _ = options.LogTo(_ => { });
});
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<JwtHelper>();
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<ICartService, CartService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<ICommentService, CommentService>();
builder.Services.AddScoped<IAdminService, AdminService>();
builder.Services.AddScoped<ISellerService, SellerService>();
builder.Services.AddScoped<AppInitializer>();
//
// // Add JWT Authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"] ?? "your-default-secret-key-here")),
        ValidateIssuer = false,
        ValidateAudience = false,
        RequireExpirationTime = true,
        ClockSkew = TimeSpan.Zero // Optional: Set clock skew to zero for immediate expiration
    };
});

WebApplication app = builder.Build();

_ = app.UseSwagger();
_ = app.UseSwaggerUI();

app.UseHttpsRedirection();

// Use JWT Middleware

app.UseMiddleware<ErrorHandlerMiddleware>();
app.UseRouting();
app.UseCors(policy =>
    policy.AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader()
);

app.UseMiddleware<TestMiddleware>();
app.UseAuthentication();
app.UseAuthorization();
app.UseMiddleware<DynamicAuthorizationMiddleware>();

app.MapControllers();


using (IServiceScope scope = app.Services.CreateScope())
{
    AppInitializer initializer = scope.ServiceProvider.GetRequiredService<AppInitializer>();
    initializer.Initialize();
}

app.Run();
