using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace polwart_backend.Migrations
{
    /// <inheritdoc />
    public partial class StorebackgroundformatinMap : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "BackgroundFormat",
                table: "Maps",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BackgroundFormat",
                table: "Maps");
        }
    }
}
