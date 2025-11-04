using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace NeoMoto.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialPostgreSQL : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Filiais",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Nome = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    Endereco = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Cidade = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Uf = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Filiais", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Motos",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Placa = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Modelo = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Ano = table.Column<int>(type: "integer", nullable: false),
                    FilialId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Motos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Motos_Filiais_FilialId",
                        column: x => x.FilialId,
                        principalTable: "Filiais",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Manutencoes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    MotoId = table.Column<Guid>(type: "uuid", nullable: false),
                    Data = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Descricao = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Custo = table.Column<decimal>(type: "numeric", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Manutencoes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Manutencoes_Motos_MotoId",
                        column: x => x.MotoId,
                        principalTable: "Motos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Filiais",
                columns: new[] { "Id", "Cidade", "Endereco", "Nome", "Uf" },
                values: new object[,]
                {
                    { new Guid("2229f345-08ad-488a-93ac-8fddf55bdb5a"), "São Paulo", "Rua A, 100", "Filial Centro", "SP" },
                    { new Guid("6b9b2638-ea78-45f3-ae69-c73e749a3792"), "São Paulo", "Av. B, 200", "Filial Zona Sul", "SP" }
                });

            migrationBuilder.InsertData(
                table: "Motos",
                columns: new[] { "Id", "Ano", "FilialId", "Modelo", "Placa" },
                values: new object[,]
                {
                    { new Guid("0d93b0f6-74e3-416a-b5da-cdb0a053ae2d"), 2020, new Guid("6b9b2638-ea78-45f3-ae69-c73e749a3792"), "Honda Biz 125", "IJK7L89" },
                    { new Guid("826a3f70-7d44-4a03-ab8d-0e51283161ce"), 2021, new Guid("2229f345-08ad-488a-93ac-8fddf55bdb5a"), "Yamaha Factor 150", "EFG4H56" },
                    { new Guid("ba0c3d0c-de7a-4232-940e-6c195893d019"), 2022, new Guid("2229f345-08ad-488a-93ac-8fddf55bdb5a"), "Honda CG 160", "ABC1D23" }
                });

            migrationBuilder.InsertData(
                table: "Manutencoes",
                columns: new[] { "Id", "Custo", "Data", "Descricao", "MotoId" },
                values: new object[,]
                {
                    { new Guid("0626348f-c301-4f41-bf6e-bfbdfaf5b29f"), 120m, new DateTime(2025, 9, 14, 20, 20, 55, 614, DateTimeKind.Utc).AddTicks(8743), "Troca de óleo", new Guid("ba0c3d0c-de7a-4232-940e-6c195893d019") },
                    { new Guid("36276531-653f-46eb-9e4e-caa3089fd90e"), 150m, new DateTime(2025, 9, 22, 20, 20, 55, 614, DateTimeKind.Utc).AddTicks(8760), "Pastilha de freio", new Guid("0d93b0f6-74e3-416a-b5da-cdb0a053ae2d") },
                    { new Guid("9118890c-d96e-4e77-8670-5d14e9a1e4e0"), 80m, new DateTime(2025, 9, 19, 20, 20, 55, 614, DateTimeKind.Utc).AddTicks(8755), "Ajuste de corrente", new Guid("ba0c3d0c-de7a-4232-940e-6c195893d019") }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Manutencoes_MotoId",
                table: "Manutencoes",
                column: "MotoId");

            migrationBuilder.CreateIndex(
                name: "IX_Motos_FilialId",
                table: "Motos",
                column: "FilialId");

            migrationBuilder.CreateIndex(
                name: "IX_Motos_Placa",
                table: "Motos",
                column: "Placa",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Manutencoes");

            migrationBuilder.DropTable(
                name: "Motos");

            migrationBuilder.DropTable(
                name: "Filiais");
        }
    }
}
