using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ArenaVirtualAPI.Migrations
{
    /// <inheritdoc />
    public partial class NovaMigrate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Times_CampeonatoId",
                table: "Times",
                column: "CampeonatoId");

            migrationBuilder.AddForeignKey(
                name: "FK_Times_Campeonatos_CampeonatoId",
                table: "Times",
                column: "CampeonatoId",
                principalTable: "Campeonatos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Times_Campeonatos_CampeonatoId",
                table: "Times");

            migrationBuilder.DropIndex(
                name: "IX_Times_CampeonatoId",
                table: "Times");
        }
    }
}
