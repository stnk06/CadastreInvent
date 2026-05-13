using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

// ИСПРАВЛЕНО: Теперь пространство имен точно совпадает с вашим Designer.cs
namespace CadastreInvent.Infrastructure.Migrations
{
    public partial class AddValuationMlFeatures : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "HasViolations",
                schema: "valuation",
                table: "property_characteristics",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<byte[]>(
                name: "ModelData",
                schema: "valuation",
                table: "mass_appraisal_models",
                type: "bytea",
                nullable: false,
                defaultValue: new byte[0]);

            // Хирургическая очистка мусора в БД перед сменой типа
            migrationBuilder.Sql("UPDATE valuation.mass_appraisal_models SET \"MetricsJson\" = '{}' WHERE \"MetricsJson\" IS NULL OR \"MetricsJson\" = '';");

            migrationBuilder.Sql("ALTER TABLE valuation.mass_appraisal_models ALTER COLUMN \"MetricsJson\" TYPE jsonb USING \"MetricsJson\"::jsonb;");

            migrationBuilder.AlterColumn<string>(
                name: "MetricsJson",
                schema: "valuation",
                table: "mass_appraisal_models",
                type: "jsonb",
                nullable: false,
                defaultValue: "{}");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HasViolations",
                schema: "valuation",
                table: "property_characteristics");

            migrationBuilder.DropColumn(
                name: "ModelData",
                schema: "valuation",
                table: "mass_appraisal_models");

            migrationBuilder.AlterColumn<string>(
                name: "MetricsJson",
                schema: "valuation",
                table: "mass_appraisal_models",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "jsonb",
                oldDefaultValue: "{}");
        }
    }
}