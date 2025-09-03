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

            migrationBuilder.AlterColumn<int>(
                name: "CampeonatoId",
                table: "Times",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddForeignKey(
                name: "FK_Times_Campeonatos_CampeonatoId",
                table: "Times",
                column: "CampeonatoId",
                principalTable: "Campeonatos",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Times_Campeonatos_CampeonatoId",
                table: "Times");

            migrationBuilder.AlterColumn<int>(
                name: "CampeonatoId",
                table: "Times",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Times_Campeonatos_CampeonatoId",
                table: "Times",
                column: "CampeonatoId",
                principalTable: "Campeonatos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
