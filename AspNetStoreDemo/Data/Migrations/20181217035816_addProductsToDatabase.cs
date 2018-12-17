using Microsoft.EntityFrameworkCore.Migrations;

namespace AspNetStoreDemo.Data.Migrations
{
    public partial class addProductsToDatabase : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ShadeColor",
                table: "Products",
                newName: "Description");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Description",
                table: "Products",
                newName: "ShadeColor");
        }
    }
}
