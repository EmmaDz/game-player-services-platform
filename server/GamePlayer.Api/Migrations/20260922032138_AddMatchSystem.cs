using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GamePlayer.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddMatchSystem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "matches",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    external_match_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    game_mode = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    played_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_matches", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "match_participants",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    match_id = table.Column<Guid>(type: "uuid", nullable: false),
                    player_profile_id = table.Column<Guid>(type: "uuid", nullable: false),
                    score = table.Column<int>(type: "integer", nullable: false),
                    kills = table.Column<int>(type: "integer", nullable: false),
                    deaths = table.Column<int>(type: "integer", nullable: false),
                    result = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_match_participants", x => x.id);
                    table.ForeignKey(
                        name: "FK_match_participants_matches_match_id",
                        column: x => x.match_id,
                        principalTable: "matches",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_match_participants_player_profiles_player_profile_id",
                        column: x => x.player_profile_id,
                        principalTable: "player_profiles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_match_participants_match_id_player_profile_id",
                table: "match_participants",
                columns: new[] { "match_id", "player_profile_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_match_participants_player_profile_id",
                table: "match_participants",
                column: "player_profile_id");

            migrationBuilder.CreateIndex(
                name: "IX_matches_external_match_id",
                table: "matches",
                column: "external_match_id",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "match_participants");

            migrationBuilder.DropTable(
                name: "matches");
        }
    }
}
