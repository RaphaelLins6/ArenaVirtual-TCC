using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ArenaVirtualAPI.Migrations
{
    /// <inheritdoc />
    public partial class Migracao2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CampeonatoId",
                table: "UsuarioCampeonatoFavoritos",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "UsuarioId",
                table: "UsuarioCampeonatoFavoritos",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<Guid>(
                name: "CapitaoClientAppId",
                table: "Times",
                type: "uniqueidentifier",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CampeonatoId",
                table: "UsuarioCampeonatoFavoritos");

            migrationBuilder.DropColumn(
                name: "UsuarioId",
                table: "UsuarioCampeonatoFavoritos");

            migrationBuilder.DropColumn(
                name: "CapitaoClientAppId",
                table: "Times");
        }
    }
}
