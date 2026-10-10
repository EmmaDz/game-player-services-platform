using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GamePlayer.Api.Migrations
{
    /// <inheritdoc />
    public partial class InitialPlayerProfile : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "player_profiles",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    display_name = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    region = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    bio = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_player_profiles", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_player_profiles_display_name",
                table: "player_profiles",
                column: "display_name",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "player_profiles");
        }
    }
}
