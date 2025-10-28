using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ArenaVirtualAPI.Migrations
{
    /// <inheritdoc />
    public partial class NovaMigracaoComRelacionamentosCorrigidos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Time_CapitaoId",
                table: "Time");

            migrationBuilder.DropColumn(
                name: "PontuacaoTotal",
                table: "Time");

            migrationBuilder.DropColumn(
                name: "Descricao",
                table: "Campeonatos");

            migrationBuilder.DropColumn(
                name: "Modalidade",
                table: "Campeonatos");

            migrationBuilder.DropColumn(
                name: "Regras",
                table: "Campeonatos");

            migrationBuilder.AlterColumn<string>(
                name: "Regiao",
                table: "Time",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Descricao",
                table: "Time",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "CapitaoId",
                table: "Time",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "CapitaoClientAppId",
                table: "Time",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CampeonatoClientAppId",
                table: "Jogos",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<int>(
                name: "RodadaDeJogosId",
                table: "Jogos",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "TimeAClientAppId",
                table: "Jogos",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "TimeBClientAppId",
                table: "Jogos",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AlterColumn<string>(
                name: "TelefoneOrganizador",
                table: "Campeonatos",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "NumeroEquipes",
                table: "Campeonatos",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "NomeOrganizador",
                table: "Campeonatos",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "LogoUrl",
                table: "Campeonatos",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "FormatoCampeonato",
                table: "Campeonatos",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "EmailOrganizador",
                table: "Campeonatos",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "DataTermino",
                table: "Campeonatos",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.CreateTable(
                name: "AvaliacoesArbitros",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ClientAppId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ArbitroId = table.Column<int>(type: "int", nullable: false),
                    JogoId = table.Column<int>(type: "int", nullable: false),
                    Comentarios = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Nota = table.Column<int>(type: "int", nullable: false),
                    IsSynced = table.Column<bool>(type: "bit", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AvaliacoesArbitros", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AvaliacoesArbitros_Jogos_JogoId",
                        column: x => x.JogoId,
                        principalTable: "Jogos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AvaliacoesArbitros_Usuarios_ArbitroId",
                        column: x => x.ArbitroId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CampanhasPatrocinios",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ClientAppId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ImagemPatrocinador = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Nome = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PatrocinadorId = table.Column<int>(type: "int", nullable: false),
                    CampeonatoId = table.Column<int>(type: "int", nullable: false),
                    ValorProposta = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Inicio = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Fim = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Descricao = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsSynced = table.Column<bool>(type: "bit", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CampanhasPatrocinios", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CampanhasPatrocinios_Campeonatos_CampeonatoId",
                        column: x => x.CampeonatoId,
                        principalTable: "Campeonatos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CampanhasPatrocinios_Usuarios_PatrocinadorId",
                        column: x => x.PatrocinadorId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "EstatisticasPartidas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ClientAppId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UsuarioId = table.Column<int>(type: "int", nullable: false),
                    JogoId = table.Column<int>(type: "int", nullable: false),
                    TimeId = table.Column<int>(type: "int", nullable: false),
                    Pontos = table.Column<int>(type: "int", nullable: false),
                    Rebotes = table.Column<int>(type: "int", nullable: false),
                    Assistencias = table.Column<int>(type: "int", nullable: false),
                    Roubos = table.Column<int>(type: "int", nullable: false),
                    Bloqueios = table.Column<int>(type: "int", nullable: false),
                    Faltas = table.Column<int>(type: "int", nullable: false),
                    Turnovers = table.Column<int>(type: "int", nullable: false),
                    Arremessos2PontosConvertidos = table.Column<int>(type: "int", nullable: false),
                    Arremessos2PontosTentados = table.Column<int>(type: "int", nullable: false),
                    Arremessos3PontosConvertidos = table.Column<int>(type: "int", nullable: false),
                    Arremessos3PontosTentados = table.Column<int>(type: "int", nullable: false),
                    LancesLivresConvertidos = table.Column<int>(type: "int", nullable: false),
                    LancesLivresTentados = table.Column<int>(type: "int", nullable: false),
                    IsSynced = table.Column<bool>(type: "bit", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EstatisticasPartidas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EstatisticasPartidas_Jogos_JogoId",
                        column: x => x.JogoId,
                        principalTable: "Jogos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EstatisticasPartidas_Time_TimeId",
                        column: x => x.TimeId,
                        principalTable: "Time",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EstatisticasPartidas_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Inscricoes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ClientAppId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TimeClientAppId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TimeId = table.Column<int>(type: "int", nullable: true),
                    CampeonatoClientAppId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CampeonatoId = table.Column<int>(type: "int", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsSynced = table.Column<bool>(type: "bit", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Inscricoes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Inscricoes_Campeonatos_CampeonatoId",
                        column: x => x.CampeonatoId,
                        principalTable: "Campeonatos",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Inscricoes_Time_TimeId",
                        column: x => x.TimeId,
                        principalTable: "Time",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "PropostasPatrocinio",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ClientAppId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PatrocinadorId = table.Column<int>(type: "int", nullable: false),
                    CampeonatoId = table.Column<int>(type: "int", nullable: false),
                    NomePatrocinador = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ImagemPatrocinador = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LinkPatrocinador = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ValorMonetario = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    DataInicio = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DataFim = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Mensagem = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Aprovada = table.Column<bool>(type: "bit", nullable: false),
                    IsSynced = table.Column<bool>(type: "bit", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PropostasPatrocinio", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PropostasPatrocinio_Campeonatos_CampeonatoId",
                        column: x => x.CampeonatoId,
                        principalTable: "Campeonatos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PropostasPatrocinio_Usuarios_PatrocinadorId",
                        column: x => x.PatrocinadorId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "RodadasDeJogos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ClientAppId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NomeRodada = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsSynced = table.Column<bool>(type: "bit", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RodadasDeJogos", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Time_CapitaoId",
                table: "Time",
                column: "CapitaoId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Jogos_RodadaDeJogosId",
                table: "Jogos",
                column: "RodadaDeJogosId");

            migrationBuilder.CreateIndex(
                name: "IX_AvaliacoesArbitros_ArbitroId",
                table: "AvaliacoesArbitros",
                column: "ArbitroId");

            migrationBuilder.CreateIndex(
                name: "IX_AvaliacoesArbitros_JogoId",
                table: "AvaliacoesArbitros",
                column: "JogoId");

            migrationBuilder.CreateIndex(
                name: "IX_CampanhasPatrocinios_CampeonatoId",
                table: "CampanhasPatrocinios",
                column: "CampeonatoId");

            migrationBuilder.CreateIndex(
                name: "IX_CampanhasPatrocinios_PatrocinadorId",
                table: "CampanhasPatrocinios",
                column: "PatrocinadorId");

            migrationBuilder.CreateIndex(
                name: "IX_EstatisticasPartidas_JogoId",
                table: "EstatisticasPartidas",
                column: "JogoId");

            migrationBuilder.CreateIndex(
                name: "IX_EstatisticasPartidas_TimeId",
                table: "EstatisticasPartidas",
                column: "TimeId");

            migrationBuilder.CreateIndex(
                name: "IX_EstatisticasPartidas_UsuarioId",
                table: "EstatisticasPartidas",
                column: "UsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_Inscricoes_CampeonatoId",
                table: "Inscricoes",
                column: "CampeonatoId");

            migrationBuilder.CreateIndex(
                name: "IX_Inscricoes_TimeId",
                table: "Inscricoes",
                column: "TimeId");

            migrationBuilder.CreateIndex(
                name: "IX_PropostasPatrocinio_CampeonatoId",
                table: "PropostasPatrocinio",
                column: "CampeonatoId");

            migrationBuilder.CreateIndex(
                name: "IX_PropostasPatrocinio_PatrocinadorId",
                table: "PropostasPatrocinio",
                column: "PatrocinadorId");

            migrationBuilder.AddForeignKey(
                name: "FK_Jogos_RodadasDeJogos_RodadaDeJogosId",
                table: "Jogos",
                column: "RodadaDeJogosId",
                principalTable: "RodadasDeJogos",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Jogos_RodadasDeJogos_RodadaDeJogosId",
                table: "Jogos");

            migrationBuilder.DropTable(
                name: "AvaliacoesArbitros");

            migrationBuilder.DropTable(
                name: "CampanhasPatrocinios");

            migrationBuilder.DropTable(
                name: "EstatisticasPartidas");

            migrationBuilder.DropTable(
                name: "Inscricoes");

            migrationBuilder.DropTable(
                name: "PropostasPatrocinio");

            migrationBuilder.DropTable(
                name: "RodadasDeJogos");

            migrationBuilder.DropIndex(
                name: "IX_Time_CapitaoId",
                table: "Time");

            migrationBuilder.DropIndex(
                name: "IX_Jogos_RodadaDeJogosId",
                table: "Jogos");

            migrationBuilder.DropColumn(
                name: "CampeonatoClientAppId",
                table: "Jogos");

            migrationBuilder.DropColumn(
                name: "RodadaDeJogosId",
                table: "Jogos");

            migrationBuilder.DropColumn(
                name: "TimeAClientAppId",
                table: "Jogos");

            migrationBuilder.DropColumn(
                name: "TimeBClientAppId",
                table: "Jogos");

            migrationBuilder.AlterColumn<string>(
                name: "Regiao",
                table: "Time",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<string>(
                name: "Descricao",
                table: "Time",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500);

            migrationBuilder.AlterColumn<int>(
                name: "CapitaoId",
                table: "Time",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<Guid>(
                name: "CapitaoClientAppId",
                table: "Time",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AddColumn<int>(
                name: "PontuacaoTotal",
                table: "Time",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<string>(
                name: "TelefoneOrganizador",
                table: "Campeonatos",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<int>(
                name: "NumeroEquipes",
                table: "Campeonatos",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<string>(
                name: "NomeOrganizador",
                table: "Campeonatos",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "LogoUrl",
                table: "Campeonatos",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "FormatoCampeonato",
                table: "Campeonatos",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "EmailOrganizador",
                table: "Campeonatos",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<DateTime>(
                name: "DataTermino",
                table: "Campeonatos",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AddColumn<string>(
                name: "Descricao",
                table: "Campeonatos",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Modalidade",
                table: "Campeonatos",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Regras",
                table: "Campeonatos",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Time_CapitaoId",
                table: "Time",
                column: "CapitaoId",
                unique: true,
                filter: "[CapitaoId] IS NOT NULL");
        }
    }
}
