using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WhatDoYouMeme.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddWinningCaptionToRound : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "WinningCaptionId",
                table: "Rounds",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Rounds_WinningCaptionId",
                table: "Rounds",
                column: "WinningCaptionId");

            migrationBuilder.AddForeignKey(
                name: "FK_Rounds_Captions_WinningCaptionId",
                table: "Rounds",
                column: "WinningCaptionId",
                principalTable: "Captions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Rounds_Captions_WinningCaptionId",
                table: "Rounds");

            migrationBuilder.DropIndex(
                name: "IX_Rounds_WinningCaptionId",
                table: "Rounds");

            migrationBuilder.DropColumn(
                name: "WinningCaptionId",
                table: "Rounds");
        }
    }
}
