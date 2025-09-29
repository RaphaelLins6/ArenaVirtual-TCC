using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ArenaVirtualAPI.Migrations
{
    /// <inheritdoc />
    public partial class MakeConviteFKsNullable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Campeonatos_Usuarios_OrganizadorId",
                table: "Campeonatos");

            migrationBuilder.DropForeignKey(
                name: "FK_Convites_Times_TimeId",
                table: "Convites");

            migrationBuilder.DropForeignKey(
                name: "FK_Convites_Usuarios_IdSolicitanteServidor",
                table: "Convites");

            migrationBuilder.AlterColumn<int>(
                name: "TimeId",
                table: "Convites",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<int>(
                name: "IdSolicitanteServidor",
                table: "Convites",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<int>(
                name: "OrganizadorId",
                table: "Campeonatos",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddForeignKey(
                name: "FK_Campeonatos_Usuarios_OrganizadorId",
                table: "Campeonatos",
                column: "OrganizadorId",
                principalTable: "Usuarios",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Convites_Times_TimeId",
                table: "Convites",
                column: "TimeId",
                principalTable: "Times",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Convites_Usuarios_IdSolicitanteServidor",
                table: "Convites",
                column: "IdSolicitanteServidor",
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
                name: "FK_Convites_Times_TimeId",
                table: "Convites");

            migrationBuilder.DropForeignKey(
                name: "FK_Convites_Usuarios_IdSolicitanteServidor",
                table: "Convites");

            migrationBuilder.AlterColumn<int>(
                name: "TimeId",
                table: "Convites",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "IdSolicitanteServidor",
                table: "Convites",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "OrganizadorId",
                table: "Campeonatos",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Campeonatos_Usuarios_OrganizadorId",
                table: "Campeonatos",
                column: "OrganizadorId",
                principalTable: "Usuarios",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Convites_Times_TimeId",
                table: "Convites",
                column: "TimeId",
                principalTable: "Times",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Convites_Usuarios_IdSolicitanteServidor",
                table: "Convites",
                column: "IdSolicitanteServidor",
                principalTable: "Usuarios",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
