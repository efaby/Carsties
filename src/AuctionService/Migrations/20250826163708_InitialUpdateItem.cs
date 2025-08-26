using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AuctionService.Migrations
{
    /// <inheritdoc />
    public partial class InitialUpdateItem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Milage",
                table: "Items",
                newName: "Mileage");

            migrationBuilder.AddColumn<string>(
                name: "ImageUrl",
                table: "Items",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImageUrl",
                table: "Items");

            migrationBuilder.RenameColumn(
                name: "Mileage",
                table: "Items",
                newName: "Milage");
        }
    }
}
