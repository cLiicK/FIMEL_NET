using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fimel.Models.Migrations
{
    /// <inheritdoc />
    public partial class AgregaCatalogoExamenes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CategoriasExamen",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Orden = table.Column<int>(type: "int", nullable: false),
                    Vigente = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CategoriasExamen", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TiposExamen",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CategoriaExamenId = table.Column<int>(type: "int", nullable: false),
                    NombreExamen = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CodigoFonasa = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Orden = table.Column<int>(type: "int", nullable: false),
                    Vigente = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TiposExamen", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TiposExamen_CategoriasExamen_CategoriaExamenId",
                        column: x => x.CategoriaExamenId,
                        principalTable: "CategoriasExamen",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TiposExamen_CategoriaExamenId",
                table: "TiposExamen",
                column: "CategoriaExamenId");

            // ── SEED: Categorías ──────────────────────────────────────────────
            migrationBuilder.Sql(@"
SET IDENTITY_INSERT CategoriasExamen ON;
INSERT INTO CategoriasExamen (Id, Nombre, Orden, Vigente, FechaCreacion) VALUES
(1,  N'Hematología',                        1,  'S', GETDATE()),
(2,  N'Coagulación',                        2,  'S', GETDATE()),
(3,  N'Bioquímica',                         3,  'S', GETDATE()),
(4,  N'Perfil lipídico',                    4,  'S', GETDATE()),
(5,  N'Electrolitos plasmáticos',           5,  'S', GETDATE()),
(6,  N'Perfil hepático',                    6,  'S', GETDATE()),
(7,  N'Perfil tiroideo',                    7,  'S', GETDATE()),
(8,  N'Perfil férrico',                     8,  'S', GETDATE()),
(9,  N'Examen de orina',                    9,  'S', GETDATE()),
(10, N'Infecciones urinarias',              10, 'S', GETDATE()),
(11, N'Infecciones de transmisión sexual',  11, 'S', GETDATE()),
(12, N'Exámenes ginecológicos',             12, 'S', GETDATE()),
(13, N'Embarazo',                           13, 'S', GETDATE()),
(14, N'Infecciones del embarazo',           14, 'S', GETDATE()),
(15, N'Cultivos obstétricos',               15, 'S', GETDATE()),
(16, N'Fertilidad y hormonas',              16, 'S', GETDATE()),
(17, N'Vitaminas',                          17, 'S', GETDATE()),
(18, N'Inmunología',                        18, 'S', GETDATE()),
(19, N'Otros',                              19, 'S', GETDATE());
SET IDENTITY_INSERT CategoriasExamen OFF;
");

            // ── SEED: Exámenes ────────────────────────────────────────────────
            migrationBuilder.Sql(@"
SET IDENTITY_INSERT TiposExamen ON;
INSERT INTO TiposExamen (Id, CategoriaExamenId, NombreExamen, CodigoFonasa, Orden, Vigente, FechaCreacion) VALUES
-- Hematología (1)
(1,  1, N'Hemograma',                                NULL, 1,  'S', GETDATE()),
(2,  1, N'Hematocrito',                              NULL, 2,  'S', GETDATE()),
(3,  1, N'Hemoglobina',                              NULL, 3,  'S', GETDATE()),
(4,  1, N'Recuento de plaquetas',                    NULL, 4,  'S', GETDATE()),
(5,  1, N'Recuento de reticulocitos',                NULL, 5,  'S', GETDATE()),
(6,  1, N'VHS',                                      NULL, 6,  'S', GETDATE()),
(7,  1, N'Proteína C Reactiva (PCR)',                NULL, 7,  'S', GETDATE()),
-- Coagulación (2)
(8,  2, N'TP (Tiempo de Protrombina)',               NULL, 1,  'S', GETDATE()),
(9,  2, N'INR',                                      NULL, 2,  'S', GETDATE()),
(10, 2, N'TTPA',                                     NULL, 3,  'S', GETDATE()),
(11, 2, N'Fibrinógeno',                              NULL, 4,  'S', GETDATE()),
-- Bioquímica (3)
(12, 3, N'Glicemia',                                 NULL, 1,  'S', GETDATE()),
(13, 3, N'Glicemia en ayunas',                       NULL, 2,  'S', GETDATE()),
(14, 3, N'Curva de tolerancia a la glucosa 75 g',    NULL, 3,  'S', GETDATE()),
(15, 3, N'Hemoglobina glicosilada (HbA1c)',          NULL, 4,  'S', GETDATE()),
(16, 3, N'Urea',                                     NULL, 5,  'S', GETDATE()),
(17, 3, N'Creatinina',                               NULL, 6,  'S', GETDATE()),
(18, 3, N'Ácido úrico',                              NULL, 7,  'S', GETDATE()),
-- Perfil lipídico (4)
(19, 4, N'Colesterol total',                         NULL, 1,  'S', GETDATE()),
(20, 4, N'HDL',                                      NULL, 2,  'S', GETDATE()),
(21, 4, N'LDL',                                      NULL, 3,  'S', GETDATE()),
(22, 4, N'Triglicéridos',                            NULL, 4,  'S', GETDATE()),
-- Electrolitos plasmáticos (5)
(23, 5, N'Sodio',                                    NULL, 1,  'S', GETDATE()),
(24, 5, N'Potasio',                                  NULL, 2,  'S', GETDATE()),
(25, 5, N'Cloro',                                    NULL, 3,  'S', GETDATE()),
(26, 5, N'Calcio',                                   NULL, 4,  'S', GETDATE()),
(27, 5, N'Magnesio',                                 NULL, 5,  'S', GETDATE()),
-- Perfil hepático (6)
(28, 6, N'AST (GOT)',                                NULL, 1,  'S', GETDATE()),
(29, 6, N'ALT (GPT)',                                NULL, 2,  'S', GETDATE()),
(30, 6, N'Fosfatasa alcalina',                       NULL, 3,  'S', GETDATE()),
(31, 6, N'GGT',                                      NULL, 4,  'S', GETDATE()),
(32, 6, N'Bilirrubina total',                        NULL, 5,  'S', GETDATE()),
(33, 6, N'Bilirrubina directa',                      NULL, 6,  'S', GETDATE()),
(34, 6, N'Albúmina',                                 NULL, 7,  'S', GETDATE()),
(35, 6, N'Proteínas totales',                        NULL, 8,  'S', GETDATE()),
-- Perfil tiroideo (7)
(36, 7, N'TSH',                                      NULL, 1,  'S', GETDATE()),
(37, 7, N'T4 Libre',                                 NULL, 2,  'S', GETDATE()),
(38, 7, N'T3',                                       NULL, 3,  'S', GETDATE()),
-- Perfil férrico (8)
(39, 8, N'Ferritina',                                NULL, 1,  'S', GETDATE()),
(40, 8, N'Hierro sérico',                            NULL, 2,  'S', GETDATE()),
(41, 8, N'Transferrina',                             NULL, 3,  'S', GETDATE()),
(42, 8, N'Saturación de transferrina',               NULL, 4,  'S', GETDATE()),
-- Examen de orina (9)
(43, 9, N'Orina completa',                           NULL, 1,  'S', GETDATE()),
(44, 9, N'Sedimento urinario',                       NULL, 2,  'S', GETDATE()),
(45, 9, N'Urocultivo',                               NULL, 3,  'S', GETDATE()),
(46, 9, N'Proteinuria',                              NULL, 4,  'S', GETDATE()),
(47, 9, N'Relación proteína/creatinina',             NULL, 5,  'S', GETDATE()),
(48, 9, N'Microalbuminuria',                         NULL, 6,  'S', GETDATE()),
-- Infecciones urinarias (10)
(49, 10, N'Urocultivo con antibiograma',             NULL, 1,  'S', GETDATE()),
-- Infecciones de transmisión sexual (11)
(50, 11, N'VIH',                                     NULL, 1,  'S', GETDATE()),
(51, 11, N'VDRL',                                    NULL, 2,  'S', GETDATE()),
(52, 11, N'RPR',                                     NULL, 3,  'S', GETDATE()),
(53, 11, N'TPPA',                                    NULL, 4,  'S', GETDATE()),
(54, 11, N'FTA-ABS',                                 NULL, 5,  'S', GETDATE()),
(55, 11, N'HBsAg',                                   NULL, 6,  'S', GETDATE()),
(56, 11, N'Anti-HBs',                                NULL, 7,  'S', GETDATE()),
(57, 11, N'Anti-HBc',                                NULL, 8,  'S', GETDATE()),
(58, 11, N'Hepatitis C',                             NULL, 9,  'S', GETDATE()),
(59, 11, N'Chlamydia trachomatis (PCR)',              NULL, 10, 'S', GETDATE()),
(60, 11, N'Neisseria gonorrhoeae (PCR)',              NULL, 11, 'S', GETDATE()),
(61, 11, N'Mycoplasma genitalium (PCR)',              NULL, 12, 'S', GETDATE()),
(62, 11, N'Trichomonas vaginalis (PCR)',              NULL, 13, 'S', GETDATE()),
(63, 11, N'Herpes Simplex (PCR)',                    NULL, 14, 'S', GETDATE()),
(64, 11, N'Cultivo para Gonococo',                   NULL, 15, 'S', GETDATE()),
-- Exámenes ginecológicos (12)
(65, 12, N'Papanicolaou',                            NULL, 1,  'S', GETDATE()),
(66, 12, N'PCR VPH',                                 NULL, 2,  'S', GETDATE()),
(67, 12, N'Genotipificación VPH',                    NULL, 3,  'S', GETDATE()),
(68, 12, N'Cultivo vaginal',                         NULL, 4,  'S', GETDATE()),
(69, 12, N'Cultivo endocervical',                    NULL, 5,  'S', GETDATE()),
(70, 12, N'Frotis vaginal',                          NULL, 6,  'S', GETDATE()),
(71, 12, N'Tinción de Gram',                         NULL, 7,  'S', GETDATE()),
(72, 12, N'Estudio de flujo vaginal',                NULL, 8,  'S', GETDATE()),
(73, 12, N'Test de Nugent',                          NULL, 9,  'S', GETDATE()),
(74, 12, N'pH vaginal',                              NULL, 10, 'S', GETDATE()),
(75, 12, N'Test de aminas',                          NULL, 11, 'S', GETDATE()),
(76, 12, N'Colposcopía',                             NULL, 12, 'S', GETDATE()),
(77, 12, N'Biopsia cervical',                        NULL, 13, 'S', GETDATE()),
-- Embarazo (13)
(78, 13, N'β-hCG cuantitativa',                     NULL, 1,  'S', GETDATE()),
(79, 13, N'β-hCG cualitativa',                      NULL, 2,  'S', GETDATE()),
(80, 13, N'Grupo sanguíneo',                         NULL, 3,  'S', GETDATE()),
(81, 13, N'Factor Rh',                               NULL, 4,  'S', GETDATE()),
(82, 13, N'Coombs indirecto',                        NULL, 5,  'S', GETDATE()),
(83, 13, N'Test de O''Sullivan',                     NULL, 6,  'S', GETDATE()),
(84, 13, N'Curva de tolerancia a la glucosa',        NULL, 7,  'S', GETDATE()),
(85, 13, N'Proteinuria de 24 horas',                 NULL, 8,  'S', GETDATE()),
(86, 13, N'Relación proteína/creatinina urinaria',   NULL, 9,  'S', GETDATE()),
-- Infecciones del embarazo (14)
(87, 14, N'Toxoplasmosis IgG',                       NULL, 1,  'S', GETDATE()),
(88, 14, N'Toxoplasmosis IgM',                       NULL, 2,  'S', GETDATE()),
(89, 14, N'Rubéola IgG',                             NULL, 3,  'S', GETDATE()),
(90, 14, N'Rubéola IgM',                             NULL, 4,  'S', GETDATE()),
(91, 14, N'Citomegalovirus IgG',                     NULL, 5,  'S', GETDATE()),
(92, 14, N'Citomegalovirus IgM',                     NULL, 6,  'S', GETDATE()),
(93, 14, N'Parvovirus B19',                          NULL, 7,  'S', GETDATE()),
(94, 14, N'Chagas',                                  NULL, 8,  'S', GETDATE()),
(95, 14, N'HTLV I-II',                               NULL, 9,  'S', GETDATE()),
-- Cultivos obstétricos (15)
(96, 15, N'Cultivo para Streptococcus agalactiae (GBS)', NULL, 1, 'S', GETDATE()),
-- Fertilidad y hormonas (16)
(97,  16, N'FSH',                                    NULL, 1,  'S', GETDATE()),
(98,  16, N'LH',                                     NULL, 2,  'S', GETDATE()),
(99,  16, N'Estradiol',                              NULL, 3,  'S', GETDATE()),
(100, 16, N'Progesterona',                           NULL, 4,  'S', GETDATE()),
(101, 16, N'Prolactina',                             NULL, 5,  'S', GETDATE()),
(102, 16, N'AMH',                                    NULL, 6,  'S', GETDATE()),
(103, 16, N'Testosterona total',                     NULL, 7,  'S', GETDATE()),
(104, 16, N'Testosterona libre',                     NULL, 8,  'S', GETDATE()),
(105, 16, N'DHEA-S',                                 NULL, 9,  'S', GETDATE()),
(106, 16, N'SHBG',                                   NULL, 10, 'S', GETDATE()),
(107, 16, N'17-OH Progesterona',                     NULL, 11, 'S', GETDATE()),
-- Vitaminas (17)
(108, 17, N'Vitamina D',                             NULL, 1,  'S', GETDATE()),
(109, 17, N'Vitamina B12',                           NULL, 2,  'S', GETDATE()),
(110, 17, N'Ácido fólico',                           NULL, 3,  'S', GETDATE()),
-- Inmunología (18)
(111, 18, N'ANA',                                    NULL, 1,  'S', GETDATE()),
(112, 18, N'Anticardiolipinas',                      NULL, 2,  'S', GETDATE()),
(113, 18, N'Anticoagulante lúpico',                  NULL, 3,  'S', GETDATE()),
(114, 18, N'Beta-2 glicoproteína',                   NULL, 4,  'S', GETDATE()),
(115, 18, N'Factor reumatoide',                      NULL, 5,  'S', GETDATE()),
-- Otros (19)
(116, 19, N'Test rápido de embarazo',                NULL, 1,  'S', GETDATE()),
(117, 19, N'Test rápido VIH',                        NULL, 2,  'S', GETDATE()),
(118, 19, N'Test rápido Sífilis',                    NULL, 3,  'S', GETDATE()),
(119, 19, N'Sangre oculta en deposiciones',          NULL, 4,  'S', GETDATE());
SET IDENTITY_INSERT TiposExamen OFF;
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TiposExamen");

            migrationBuilder.DropTable(
                name: "CategoriasExamen");
        }
    }
}
