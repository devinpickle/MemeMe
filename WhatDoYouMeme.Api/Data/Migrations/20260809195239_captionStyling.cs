using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WhatDoYouMeme.Api.Migrations
{
    /// <inheritdoc />
    public partial class captionStyling : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Style",
                table: "Captions",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Style",
                table: "Captions");
        }
    }
}
