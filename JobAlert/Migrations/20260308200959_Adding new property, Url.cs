using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JobAlert.Migrations
{
    /// <inheritdoc />
    public partial class AddingnewpropertyUrl : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Url",
                table: "Jobs",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Url",
                table: "Jobs");
        }
    }
}
