using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ArenaVirtualAPI.Migrations
{
    /// <inheritdoc />
    public partial class Migracao1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Times_CapitaoId",
                table: "Times",
                column: "CapitaoId");

            migrationBuilder.CreateIndex(
                name: "IX_Campeonatos_OrganizadorId",
                table: "Campeonatos",
                column: "OrganizadorId");

            migrationBuilder.AddForeignKey(
                name: "FK_Campeonatos_Usuarios_OrganizadorId",
                table: "Campeonatos",
                column: "OrganizadorId",
                principalTable: "Usuarios",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Times_Usuarios_CapitaoId",
                table: "Times",
                column: "CapitaoId",
                principalTable: "Usuarios",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Campeonatos_Usuarios_OrganizadorId",
                table: "Campeonatos");

            migrationBuilder.DropForeignKey(
                name: "FK_Times_Usuarios_CapitaoId",
                table: "Times");

            migrationBuilder.DropIndex(
                name: "IX_Times_CapitaoId",
                table: "Times");

            migrationBuilder.DropIndex(
                name: "IX_Campeonatos_OrganizadorId",
                table: "Campeonatos");
        }
    }
}
