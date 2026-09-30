using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebApplication1.Migrations
{
    /// <inheritdoc />
    public partial class AddingSuplierProduct : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SupplierProducts",
                columns: table => new
                {
                    SupplierProductID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),

                    SupplierID = table.Column<int>(type: "int", nullable: false),

                    ProductID = table.Column<int>(type: "int", nullable: false),

                    SupplierSKU = table.Column<string>(
                        type: "nvarchar(50)",
                        maxLength: 50,
                        nullable: false),

                    ContractPrice = table.Column<decimal>(
                        type: "decimal(18,2)",
                        nullable: false),

                    ContractQuantity = table.Column<int>(
                        type: "int",
                        nullable: false),

                    CreatedAt = table.Column<DateTime>(
                        type: "datetime2",
                        nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey(
                        "PK_SupplierProducts",
                        x => x.SupplierProductID);

                    table.ForeignKey(
                        name: "FK_SupplierProducts_Products_ProductID",
                        column: x => x.ProductID,
                        principalTable: "Products",
                        principalColumn: "ProductID",
                        onDelete: ReferentialAction.Cascade);

                    table.ForeignKey(
                        name: "FK_SupplierProducts_Suppliers_SupplierID",
                        column: x => x.SupplierID,
                        principalTable: "Suppliers",
                        principalColumn: "SupplierId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SupplierProducts_ProductID",
                table: "SupplierProducts",
                column: "ProductID");

            migrationBuilder.CreateIndex(
                name: "IX_SupplierProducts_SupplierID",
                table: "SupplierProducts",
                column: "SupplierID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SupplierProducts");
        }
    }
}