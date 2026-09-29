using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WhatDoYouMeme.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddSelectedCaptionToRound : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "SelectedCaptionId",
                table: "Rounds",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Rounds_SelectedCaptionId",
                table: "Rounds",
                column: "SelectedCaptionId");

            migrationBuilder.AddForeignKey(
                name: "FK_Rounds_Captions_SelectedCaptionId",
                table: "Rounds",
                column: "SelectedCaptionId",
                principalTable: "Captions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Rounds_Captions_SelectedCaptionId",
                table: "Rounds");

            migrationBuilder.DropIndex(
                name: "IX_Rounds_SelectedCaptionId",
                table: "Rounds");

            migrationBuilder.DropColumn(
                name: "SelectedCaptionId",
                table: "Rounds");
        }
    }
}
