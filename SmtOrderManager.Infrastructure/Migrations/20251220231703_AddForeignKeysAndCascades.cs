using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmtOrderManager.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddForeignKeysAndCascades : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "BoardId1",
                table: "OrderBoards",
                type: "TEXT",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_OrderBoards_BoardId1",
                table: "OrderBoards",
                column: "BoardId1");

            migrationBuilder.AddForeignKey(
                name: "FK_OrderBoards_Boards_BoardId1",
                table: "OrderBoards",
                column: "BoardId1",
                principalTable: "Boards",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OrderBoards_Boards_BoardId1",
                table: "OrderBoards");

            migrationBuilder.DropIndex(
                name: "IX_OrderBoards_BoardId1",
                table: "OrderBoards");

            migrationBuilder.DropColumn(
                name: "BoardId1",
                table: "OrderBoards");
        }
    }
}
