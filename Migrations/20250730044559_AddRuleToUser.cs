using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace test_api.Migrations
{
    /// <inheritdoc />
    public partial class AddRuleToUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Users_Roles_RuleId",
                table: "Users");

            migrationBuilder.AlterColumn<int>(
                name: "RuleId",
                table: "Users",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Users_Roles_RuleId",
                table: "Users",
                column: "RuleId",
                principalTable: "Roles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Users_Roles_RuleId",
                table: "Users");

            migrationBuilder.AlterColumn<int>(
                name: "RuleId",
                table: "Users",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddForeignKey(
                name: "FK_Users_Roles_RuleId",
                table: "Users",
                column: "RuleId",
                principalTable: "Roles",
                principalColumn: "Id");
        }
    }
}
