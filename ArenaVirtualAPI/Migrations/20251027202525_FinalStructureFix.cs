using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ArenaVirtualAPI.Migrations
{
    /// <inheritdoc />
    public partial class FinalStructureFix : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Convites_Times_TimeId",
                table: "Convites");

            migrationBuilder.DropForeignKey(
                name: "FK_Times_Campeonatos_CampeonatoId",
                table: "Times");

            migrationBuilder.DropForeignKey(
                name: "FK_Times_Usuarios_CapitaoId",
                table: "Times");

            migrationBuilder.DropForeignKey(
                name: "FK_Usuarios_Times_TimeId",
                table: "Usuarios");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Times",
                table: "Times");

            migrationBuilder.RenameTable(
                name: "Times",
                newName: "Time");

            migrationBuilder.RenameIndex(
                name: "IX_Times_CapitaoId",
                table: "Time",
                newName: "IX_Time_CapitaoId");

            migrationBuilder.RenameIndex(
                name: "IX_Times_CampeonatoId",
                table: "Time",
                newName: "IX_Time_CampeonatoId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Time",
                table: "Time",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "Jogos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ClientAppId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TimeAId = table.Column<int>(type: "int", nullable: false),
                    TimeBId = table.Column<int>(type: "int", nullable: false),
                    CampeonatoId = table.Column<int>(type: "int", nullable: false),
                    ArbitroId = table.Column<int>(type: "int", nullable: true),
                    ArbitroClientAppId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Local = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    DataHora = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PlacarA = table.Column<int>(type: "int", nullable: false),
                    PlacarB = table.Column<int>(type: "int", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsSynced = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Jogos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Jogos_Campeonatos_CampeonatoId",
                        column: x => x.CampeonatoId,
                        principalTable: "Campeonatos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Jogos_Time_TimeAId",
                        column: x => x.TimeAId,
                        principalTable: "Time",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Jogos_Time_TimeBId",
                        column: x => x.TimeBId,
                        principalTable: "Time",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Jogos_Usuarios_ArbitroId",
                        column: x => x.ArbitroId,
                        principalTable: "Usuarios",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_UsuarioCampeonatoFavoritos_UsuarioId_CampeonatoId",
                table: "UsuarioCampeonatoFavoritos",
                columns: new[] { "UsuarioId", "CampeonatoId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Jogos_ArbitroId",
                table: "Jogos",
                column: "ArbitroId");

            migrationBuilder.CreateIndex(
                name: "IX_Jogos_CampeonatoId",
                table: "Jogos",
                column: "CampeonatoId");

            migrationBuilder.CreateIndex(
                name: "IX_Jogos_TimeAId",
                table: "Jogos",
                column: "TimeAId");

            migrationBuilder.CreateIndex(
                name: "IX_Jogos_TimeBId",
                table: "Jogos",
                column: "TimeBId");

            migrationBuilder.AddForeignKey(
                name: "FK_Convites_Time_TimeId",
                table: "Convites",
                column: "TimeId",
                principalTable: "Time",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Time_Campeonatos_CampeonatoId",
                table: "Time",
                column: "CampeonatoId",
                principalTable: "Campeonatos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Time_Usuarios_CapitaoId",
                table: "Time",
                column: "CapitaoId",
                principalTable: "Usuarios",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Usuarios_Time_TimeId",
                table: "Usuarios",
                column: "TimeId",
                principalTable: "Time",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Convites_Time_TimeId",
                table: "Convites");

            migrationBuilder.DropForeignKey(
                name: "FK_Time_Campeonatos_CampeonatoId",
                table: "Time");

            migrationBuilder.DropForeignKey(
                name: "FK_Time_Usuarios_CapitaoId",
                table: "Time");

            migrationBuilder.DropForeignKey(
                name: "FK_Usuarios_Time_TimeId",
                table: "Usuarios");

            migrationBuilder.DropTable(
                name: "Jogos");

            migrationBuilder.DropIndex(
                name: "IX_UsuarioCampeonatoFavoritos_UsuarioId_CampeonatoId",
                table: "UsuarioCampeonatoFavoritos");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Time",
                table: "Time");

            migrationBuilder.RenameTable(
                name: "Time",
                newName: "Times");

            migrationBuilder.RenameIndex(
                name: "IX_Time_CapitaoId",
                table: "Times",
                newName: "IX_Times_CapitaoId");

            migrationBuilder.RenameIndex(
                name: "IX_Time_CampeonatoId",
                table: "Times",
                newName: "IX_Times_CampeonatoId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Times",
                table: "Times",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Convites_Times_TimeId",
                table: "Convites",
                column: "TimeId",
                principalTable: "Times",
                principalColumn: "Id");

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
    }
}
