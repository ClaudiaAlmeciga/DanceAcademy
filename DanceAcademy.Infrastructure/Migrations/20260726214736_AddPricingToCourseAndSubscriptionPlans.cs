using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DanceAcademy.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPricingToCourseAndSubscriptionPlans : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "Price",
                table: "Courses",
                type: "numeric(10,2)",
                nullable: true);

            // defaultValue "Free" (no "") — así los cursos existentes quedan en un estado válido del enum,
            // coincidiendo con el default de Course.PricingType en el dominio.
            migrationBuilder.AddColumn<string>(
                name: "PricingType",
                table: "Courses",
                type: "character varying(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "Free");

            migrationBuilder.CreateTable(
                name: "SubscriptionPlans",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    Description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    Price = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    BillingPeriodDays = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SubscriptionPlans", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SubscriptionPlans_Name",
                table: "SubscriptionPlans",
                column: "Name",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SubscriptionPlans");

            migrationBuilder.DropColumn(
                name: "Price",
                table: "Courses");

            migrationBuilder.DropColumn(
                name: "PricingType",
                table: "Courses");
        }
    }
}
