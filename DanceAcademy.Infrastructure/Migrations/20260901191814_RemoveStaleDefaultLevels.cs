using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DanceAcademy.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RemoveStaleDefaultLevels : Migration
    {
        private const string BeginnerId = "11111111-1111-1111-1111-111111111111";
        private const string IntermediateId = "22222222-2222-2222-2222-222222222222";
        private const string AdvancedId = "33333333-3333-3333-3333-333333333333";

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // "Beginner"/"Intermediate"/"Advanced" fueron el backfill de la migración
            // ConvertCourseLevelToLevelEntity (2026-07-26), necesario en su momento para migrar
            // la columna Level (string) a la tabla Levels. La app pasó a usar nombres en
            // español ("Principiante"/"Intermedio"/"Avanzado") poco después, así que estos tres
            // quedan como niveles vacíos y duplicados en cualquier base nueva. Se borran solo si
            // ningún curso quedó apuntando a ellos (no debería pasar, pero evita romper un FK).
            migrationBuilder.Sql($"""
                DELETE FROM "Levels"
                WHERE "Id" IN ('{BeginnerId}', '{IntermediateId}', '{AdvancedId}')
                AND NOT EXISTS (SELECT 1 FROM "Courses" WHERE "Courses"."LevelId" = "Levels"."Id");
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql($"""
                INSERT INTO "Levels" ("Id", "Name", "Order", "IsActive", "CreatedAt", "UpdatedAt")
                VALUES
                    ('{BeginnerId}', 'Beginner', 1, true, NOW(), NULL),
                    ('{IntermediateId}', 'Intermediate', 2, true, NOW(), NULL),
                    ('{AdvancedId}', 'Advanced', 3, true, NOW(), NULL)
                ON CONFLICT ("Id") DO NOTHING;
                """);
        }
    }
}
