using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ArenaVirtualAPI.Migrations
{
    /// <inheritdoc />
    public partial class AdicionarMembrosEmTime : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Usuarios_TimeId",
                table: "Usuarios",
                column: "TimeId");

            migrationBuilder.AddForeignKey(
                name: "FK_Usuarios_Times_TimeId",
                table: "Usuarios",
                column: "TimeId",
                principalTable: "Times",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Usuarios_Times_TimeId",
                table: "Usuarios");

            migrationBuilder.DropIndex(
                name: "IX_Usuarios_TimeId",
                table: "Usuarios");
        }
    }
}
