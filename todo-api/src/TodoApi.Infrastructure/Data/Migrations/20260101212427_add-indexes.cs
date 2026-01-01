using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TodoApi.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class addindexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_TodoItems_CreatedAt",
                table: "TodoItems",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_TodoItems_DueDate",
                table: "TodoItems",
                column: "DueDate");

            migrationBuilder.CreateIndex(
                name: "IX_TodoItems_IsCompleted",
                table: "TodoItems",
                column: "IsCompleted");

            migrationBuilder.CreateIndex(
                name: "IX_TodoItems_IsDeleted",
                table: "TodoItems",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_TodoItems_Priority",
                table: "TodoItems",
                column: "Priority");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_TodoItems_CreatedAt",
                table: "TodoItems");

            migrationBuilder.DropIndex(
                name: "IX_TodoItems_DueDate",
                table: "TodoItems");

            migrationBuilder.DropIndex(
                name: "IX_TodoItems_IsCompleted",
                table: "TodoItems");

            migrationBuilder.DropIndex(
                name: "IX_TodoItems_IsDeleted",
                table: "TodoItems");

            migrationBuilder.DropIndex(
                name: "IX_TodoItems_Priority",
                table: "TodoItems");
        }
    }
}
