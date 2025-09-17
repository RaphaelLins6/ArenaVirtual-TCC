using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ArenaVirtualAPI.Migrations
{
    /// <inheritdoc />
    public partial class Migracao3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Times_Campeonatos_CampeonatoId",
                table: "Times");

            migrationBuilder.DropForeignKey(
                name: "FK_Times_Usuarios_CapitaoId",
                table: "Times");

            migrationBuilder.DropForeignKey(
                name: "FK_Usuarios_Times_TimeId",
                table: "Usuarios");

            migrationBuilder.DropIndex(
                name: "IX_Times_CapitaoId",
                table: "Times");

            migrationBuilder.CreateIndex(
                name: "IX_Times_CapitaoId",
                table: "Times",
                column: "CapitaoId",
                unique: true,
                filter: "[CapitaoId] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_Times_Campeonatos_CampeonatoId",
                table: "Times",
                column: "CampeonatoId",
                principalTable: "Campeonatos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Times_Usuarios_CapitaoId",
                table: "Times",
                column: "CapitaoId",
                principalTable: "Usuarios",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Usuarios_Times_TimeId",
                table: "Usuarios",
                column: "TimeId",
                principalTable: "Times",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Times_Campeonatos_CampeonatoId",
                table: "Times");

            migrationBuilder.DropForeignKey(
                name: "FK_Times_Usuarios_CapitaoId",
                table: "Times");

            migrationBuilder.DropForeignKey(
                name: "FK_Usuarios_Times_TimeId",
                table: "Usuarios");

            migrationBuilder.DropIndex(
                name: "IX_Times_CapitaoId",
                table: "Times");

            migrationBuilder.CreateIndex(
                name: "IX_Times_CapitaoId",
                table: "Times",
                column: "CapitaoId");

            migrationBuilder.AddForeignKey(
                name: "FK_Times_Campeonatos_CampeonatoId",
                table: "Times",
                column: "CampeonatoId",
                principalTable: "Campeonatos",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Times_Usuarios_CapitaoId",
                table: "Times",
                column: "CapitaoId",
                principalTable: "Usuarios",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Usuarios_Times_TimeId",
                table: "Usuarios",
                column: "TimeId",
                principalTable: "Times",
                principalColumn: "Id");
        }
    }
}
