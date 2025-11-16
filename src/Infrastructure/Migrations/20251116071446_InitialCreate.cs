using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Cities",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cities", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TaxRules",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CityId = table.Column<Guid>(type: "uuid", nullable: false),
                    Year = table.Column<int>(type: "integer", nullable: false),
                    DailyMax = table.Column<decimal>(type: "numeric", nullable: false),
                    SingleChargeMinutes = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TaxRules", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TaxRules_Cities_CityId",
                        column: x => x.CityId,
                        principalTable: "Cities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TollFreeDates",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Date = table.Column<DateOnly>(type: "date", nullable: false),
                    IncludeDayBefore = table.Column<bool>(type: "boolean", nullable: false),
                    TaxRuleId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TollFreeDates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TollFreeDates_TaxRules_TaxRuleId",
                        column: x => x.TaxRuleId,
                        principalTable: "TaxRules",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "TollFreeMonths",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Month = table.Column<byte>(type: "smallint", nullable: false),
                    TaxRuleId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TollFreeMonths", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TollFreeMonths_TaxRules_TaxRuleId",
                        column: x => x.TaxRuleId,
                        principalTable: "TaxRules",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "TollFreeVehicles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    VehicleType = table.Column<byte>(type: "smallint", nullable: false),
                    TaxRuleId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TollFreeVehicles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TollFreeVehicles_TaxRules_TaxRuleId",
                        column: x => x.TaxRuleId,
                        principalTable: "TaxRules",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "TollFreeWeekdays",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Day = table.Column<int>(type: "integer", nullable: false),
                    TaxRuleId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TollFreeWeekdays", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TollFreeWeekdays_TaxRules_TaxRuleId",
                        column: x => x.TaxRuleId,
                        principalTable: "TaxRules",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "TollIntervals",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Start = table.Column<TimeOnly>(type: "time without time zone", nullable: false),
                    End = table.Column<TimeOnly>(type: "time without time zone", nullable: false),
                    Amount = table.Column<decimal>(type: "numeric", nullable: false),
                    TaxRuleId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TollIntervals", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TollIntervals_TaxRules_TaxRuleId",
                        column: x => x.TaxRuleId,
                        principalTable: "TaxRules",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_TaxRules_CityId_Year",
                table: "TaxRules",
                columns: new[] { "CityId", "Year" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TollFreeDates_TaxRuleId",
                table: "TollFreeDates",
                column: "TaxRuleId");

            migrationBuilder.CreateIndex(
                name: "IX_TollFreeMonths_TaxRuleId",
                table: "TollFreeMonths",
                column: "TaxRuleId");

            migrationBuilder.CreateIndex(
                name: "IX_TollFreeVehicles_TaxRuleId",
                table: "TollFreeVehicles",
                column: "TaxRuleId");

            migrationBuilder.CreateIndex(
                name: "IX_TollFreeWeekdays_TaxRuleId",
                table: "TollFreeWeekdays",
                column: "TaxRuleId");

            migrationBuilder.CreateIndex(
                name: "IX_TollIntervals_TaxRuleId",
                table: "TollIntervals",
                column: "TaxRuleId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TollFreeDates");

            migrationBuilder.DropTable(
                name: "TollFreeMonths");

            migrationBuilder.DropTable(
                name: "TollFreeVehicles");

            migrationBuilder.DropTable(
                name: "TollFreeWeekdays");

            migrationBuilder.DropTable(
                name: "TollIntervals");

            migrationBuilder.DropTable(
                name: "TaxRules");

            migrationBuilder.DropTable(
                name: "Cities");
        }
    }
}
