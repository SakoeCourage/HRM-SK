using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HRM_SK.Migrations
{
    /// <inheritdoc />
    public partial class Removed_Speciality_add_nationality_to_staff_2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Staff_Speciality_specialityId",
                table: "Staff");

            migrationBuilder.RenameColumn(
                name: "specialityId",
                table: "Staff",
                newName: "SpecialityId");

            migrationBuilder.RenameIndex(
                name: "IX_Staff_specialityId",
                table: "Staff",
                newName: "IX_Staff_SpecialityId");

            migrationBuilder.AlterColumn<Guid>(
                name: "SpecialityId",
                table: "Staff",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AddForeignKey(
                name: "FK_Staff_Speciality_SpecialityId",
                table: "Staff",
                column: "SpecialityId",
                principalTable: "Speciality",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Staff_Speciality_SpecialityId",
                table: "Staff");

            migrationBuilder.RenameColumn(
                name: "SpecialityId",
                table: "Staff",
                newName: "specialityId");

            migrationBuilder.RenameIndex(
                name: "IX_Staff_SpecialityId",
                table: "Staff",
                newName: "IX_Staff_specialityId");

            migrationBuilder.AlterColumn<Guid>(
                name: "specialityId",
                table: "Staff",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Staff_Speciality_specialityId",
                table: "Staff",
                column: "specialityId",
                principalTable: "Speciality",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
