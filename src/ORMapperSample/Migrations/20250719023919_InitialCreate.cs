using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace ORMapperSample.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Products",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    Price = table.Column<decimal>(type: "TEXT", precision: 10, scale: 2, nullable: false),
                    Quantity = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Products", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "Products",
                columns: new[] { "Id", "CreatedAt", "Description", "Name", "Price", "Quantity", "UpdatedAt" },
                values: new object[,]
                {
                    { 1, new DateTime(2025, 7, 19, 11, 39, 19, 537, DateTimeKind.Local).AddTicks(823), "これはサンプル商品1の説明です", "サンプル商品1", 1000m, 10, new DateTime(2025, 7, 19, 11, 39, 19, 537, DateTimeKind.Local).AddTicks(839) },
                    { 2, new DateTime(2025, 7, 19, 11, 39, 19, 537, DateTimeKind.Local).AddTicks(842), "これはサンプル商品2の説明です", "サンプル商品2", 2000m, 5, new DateTime(2025, 7, 19, 11, 39, 19, 537, DateTimeKind.Local).AddTicks(843) }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Products");
        }
    }
}
