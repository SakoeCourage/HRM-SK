using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HRM_SK.Migrations
{
    /// <inheritdoc />
    public partial class Updated_Staff_Posting_Relationship : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_StaffPosting_departmentId",
                table: "StaffPosting");

            migrationBuilder.DropIndex(
                name: "IX_StaffPosting_directorateId",
                table: "StaffPosting");

            migrationBuilder.DropIndex(
                name: "IX_StaffPosting_unitId",
                table: "StaffPosting");

            migrationBuilder.CreateIndex(
                name: "IX_StaffPosting_departmentId",
                table: "StaffPosting",
                column: "departmentId");

            migrationBuilder.CreateIndex(
                name: "IX_StaffPosting_directorateId",
                table: "StaffPosting",
                column: "directorateId");

            migrationBuilder.CreateIndex(
                name: "IX_StaffPosting_unitId",
                table: "StaffPosting",
                column: "unitId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_StaffPosting_departmentId",
                table: "StaffPosting");

            migrationBuilder.DropIndex(
                name: "IX_StaffPosting_directorateId",
                table: "StaffPosting");

            migrationBuilder.DropIndex(
                name: "IX_StaffPosting_unitId",
                table: "StaffPosting");

            migrationBuilder.CreateIndex(
                name: "IX_StaffPosting_departmentId",
                table: "StaffPosting",
                column: "departmentId",
                unique: true,
                filter: "[departmentId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_StaffPosting_directorateId",
                table: "StaffPosting",
                column: "directorateId",
                unique: true,
                filter: "[directorateId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_StaffPosting_unitId",
                table: "StaffPosting",
                column: "unitId",
                unique: true,
                filter: "[unitId] IS NOT NULL");
        }
    }
}
