using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HRM_SK.Migrations
{
    /// <inheritdoc />
    public partial class Removed_Speciality_add_nationality_to_staff : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Staff_Speciality_specialityId",
                table: "Staff");

            migrationBuilder.DropForeignKey(
                name: "FK_User_Department_departmentId",
                table: "User");

            migrationBuilder.DropForeignKey(
                name: "FK_User_Unit_unitId",
                table: "User");

            migrationBuilder.DropColumn(
                name: "specialityId",
                table: "StaffBioUpdateHistory");

            migrationBuilder.RenameColumn(
                name: "unitId",
                table: "User",
                newName: "UnitId");

            migrationBuilder.RenameColumn(
                name: "departmentId",
                table: "User",
                newName: "DepartmentId");

            migrationBuilder.RenameIndex(
                name: "IX_User_unitId",
                table: "User",
                newName: "IX_User_UnitId");

            migrationBuilder.RenameIndex(
                name: "IX_User_departmentId",
                table: "User",
                newName: "IX_User_DepartmentId");

            migrationBuilder.AlterColumn<Guid>(
                name: "UnitId",
                table: "User",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AlterColumn<Guid>(
                name: "DepartmentId",
                table: "User",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AddColumn<string>(
                name: "nationality",
                table: "StaffBioUpdateHistory",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<Guid>(
                name: "specialityId",
                table: "Staff",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "nationality",
                table: "Staff",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddForeignKey(
                name: "FK_Staff_Speciality_specialityId",
                table: "Staff",
                column: "specialityId",
                principalTable: "Speciality",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_User_Department_DepartmentId",
                table: "User",
                column: "DepartmentId",
                principalTable: "Department",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_User_Unit_UnitId",
                table: "User",
                column: "UnitId",
                principalTable: "Unit",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Staff_Speciality_specialityId",
                table: "Staff");

            migrationBuilder.DropForeignKey(
                name: "FK_User_Department_DepartmentId",
                table: "User");

            migrationBuilder.DropForeignKey(
                name: "FK_User_Unit_UnitId",
                table: "User");

            migrationBuilder.DropColumn(
                name: "nationality",
                table: "StaffBioUpdateHistory");

            migrationBuilder.DropColumn(
                name: "nationality",
                table: "Staff");

            migrationBuilder.RenameColumn(
                name: "UnitId",
                table: "User",
                newName: "unitId");

            migrationBuilder.RenameColumn(
                name: "DepartmentId",
                table: "User",
                newName: "departmentId");

            migrationBuilder.RenameIndex(
                name: "IX_User_UnitId",
                table: "User",
                newName: "IX_User_unitId");

            migrationBuilder.RenameIndex(
                name: "IX_User_DepartmentId",
                table: "User",
                newName: "IX_User_departmentId");

            migrationBuilder.AlterColumn<Guid>(
                name: "unitId",
                table: "User",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "departmentId",
                table: "User",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "specialityId",
                table: "StaffBioUpdateHistory",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "specialityId",
                table: "Staff",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AddForeignKey(
                name: "FK_Staff_Speciality_specialityId",
                table: "Staff",
                column: "specialityId",
                principalTable: "Speciality",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_User_Department_departmentId",
                table: "User",
                column: "departmentId",
                principalTable: "Department",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_User_Unit_unitId",
                table: "User",
                column: "unitId",
                principalTable: "Unit",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
