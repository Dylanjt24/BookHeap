using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BookHeapWeb.Migrations
{
    /// <inheritdoc />
    public partial class changedMyPropertyToPostalCodeforAppUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "MyProperty",
                table: "AspNetUsers",
                newName: "PostalCode");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "PostalCode",
                table: "AspNetUsers",
                newName: "MyProperty");
        }
    }
}
