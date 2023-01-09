using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Scribble.Identity.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RemoveApplicationUserProfile : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_Profiles_ApplicationUserProfileId",
                table: "AspNetUsers");

            migrationBuilder.DropTable(
                name: "Profiles");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_ApplicationUserProfileId",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "ApplicationUserProfileId",
                table: "AspNetUsers");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ApplicationUserProfileId",
                table: "AspNetUsers",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Profiles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdateAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Profiles", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_ApplicationUserProfileId",
                table: "AspNetUsers",
                column: "ApplicationUserProfileId",
                unique: true,
                filter: "[ApplicationUserProfileId] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_Profiles_ApplicationUserProfileId",
                table: "AspNetUsers",
                column: "ApplicationUserProfileId",
                principalTable: "Profiles",
                principalColumn: "Id");
        }
    }
}
