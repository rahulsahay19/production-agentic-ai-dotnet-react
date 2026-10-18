using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NexaStore.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddFeaturedProductsAndPrimaryImages : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsFeatured",
                table: "products",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsPrimary",
                table: "product_images",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_products_IsFeatured_IsActive",
                table: "products",
                columns: new[] { "IsFeatured", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_product_images_ProductId_IsPrimary",
                table: "product_images",
                columns: new[] { "ProductId", "IsPrimary" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_products_IsFeatured_IsActive",
                table: "products");

            migrationBuilder.DropIndex(
                name: "IX_product_images_ProductId_IsPrimary",
                table: "product_images");

            migrationBuilder.DropColumn(
                name: "IsFeatured",
                table: "products");

            migrationBuilder.DropColumn(
                name: "IsPrimary",
                table: "product_images");
        }
    }
}
