using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HRM_SK.Migrations
{
    /// <inheritdoc />
    public partial class update_speciality_6 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Staff_ECOWASCardNumber",
                table: "Staff");

            migrationBuilder.DropIndex(
                name: "IX_Staff_phone",
                table: "Staff");

            migrationBuilder.DropIndex(
                name: "IX_Staff_SNNITNumber",
                table: "Staff");

            migrationBuilder.AlterColumn<string>(
                name: "phone",
                table: "Staff",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "SNNITNumber",
                table: "Staff",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ECOWASCardNumber",
                table: "Staff",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "phone",
                table: "Staff",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "SNNITNumber",
                table: "Staff",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ECOWASCardNumber",
                table: "Staff",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Staff_ECOWASCardNumber",
                table: "Staff",
                column: "ECOWASCardNumber",
                unique: true,
                filter: "[ECOWASCardNumber] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Staff_phone",
                table: "Staff",
                column: "phone",
                unique: true,
                filter: "[phone] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Staff_SNNITNumber",
                table: "Staff",
                column: "SNNITNumber",
                unique: true,
                filter: "[SNNITNumber] IS NOT NULL");
        }
    }
}
