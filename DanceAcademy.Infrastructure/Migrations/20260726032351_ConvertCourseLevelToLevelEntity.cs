using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DanceAcademy.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ConvertCourseLevelToLevelEntity : Migration
    {
        private const string BeginnerId = "11111111-1111-1111-1111-111111111111";
        private const string IntermediateId = "22222222-2222-2222-2222-222222222222";
        private const string AdvancedId = "33333333-3333-3333-3333-333333333333";

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Levels",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Order = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Levels", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "Levels",
                columns: new[] { "Id", "Name", "Order", "IsActive", "CreatedAt", "UpdatedAt" },
                values: new object[,]
                {
                    { new Guid(BeginnerId), "Beginner", 1, true, DateTimeOffset.UtcNow, null },
                    { new Guid(IntermediateId), "Intermediate", 2, true, DateTimeOffset.UtcNow, null },
                    { new Guid(AdvancedId), "Advanced", 3, true, DateTimeOffset.UtcNow, null }
                });

            // Columna nullable primero para poder hacer el backfill antes de exigir NOT NULL
            migrationBuilder.AddColumn<Guid>(
                name: "LevelId",
                table: "Courses",
                type: "uuid",
                nullable: true);

            migrationBuilder.Sql($"""
                UPDATE "Courses" SET "LevelId" = CASE "Level"
                    WHEN 'Beginner' THEN '{BeginnerId}'::uuid
                    WHEN 'Intermediate' THEN '{IntermediateId}'::uuid
                    WHEN 'Advanced' THEN '{AdvancedId}'::uuid
                    ELSE '{BeginnerId}'::uuid
                END;
                """);

            migrationBuilder.AlterColumn<Guid>(
                name: "LevelId",
                table: "Courses",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.DropColumn(
                name: "Level",
                table: "Courses");

            migrationBuilder.CreateIndex(
                name: "IX_Courses_LevelId",
                table: "Courses",
                column: "LevelId");

            migrationBuilder.CreateIndex(
                name: "IX_Levels_Name",
                table: "Levels",
                column: "Name",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Courses_Levels_LevelId",
                table: "Courses",
                column: "LevelId",
                principalTable: "Levels",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Courses_Levels_LevelId",
                table: "Courses");

            migrationBuilder.AddColumn<string>(
                name: "Level",
                table: "Courses",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.Sql("""
                UPDATE "Courses" c SET "Level" = l."Name"
                FROM "Levels" l
                WHERE l."Id" = c."LevelId";
                """);

            migrationBuilder.DropIndex(
                name: "IX_Courses_LevelId",
                table: "Courses");

            migrationBuilder.DropColumn(
                name: "LevelId",
                table: "Courses");

            migrationBuilder.DropTable(
                name: "Levels");
        }
    }
}
