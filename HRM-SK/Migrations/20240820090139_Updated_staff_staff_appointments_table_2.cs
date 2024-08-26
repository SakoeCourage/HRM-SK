using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HRM_SK.Migrations
{
    /// <inheritdoc />
    public partial class Updated_staff_staff_appointments_table_2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_StaffAppointment_staffId",
                table: "StaffAppointment");

            migrationBuilder.CreateIndex(
                name: "IX_StaffAppointment_staffId",
                table: "StaffAppointment",
                column: "staffId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_StaffAppointment_staffId",
                table: "StaffAppointment");

            migrationBuilder.CreateIndex(
                name: "IX_StaffAppointment_staffId",
                table: "StaffAppointment",
                column: "staffId");
        }
    }
}
