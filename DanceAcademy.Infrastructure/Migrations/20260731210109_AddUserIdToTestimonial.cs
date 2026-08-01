using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DanceAcademy.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddUserIdToTestimonial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Los testimonios existentes se crearon desde el panel de Admin (flujo que este
            // cambio elimina) y no tienen un usuario real que los respalde — no hay un UserId
            // válido al que asignarlos. Se eliminan en vez de backfillear con un usuario falso,
            // que sería exactamente el problema que este cambio busca evitar.
            migrationBuilder.Sql(@"DELETE FROM ""Testimonials"";");

            migrationBuilder.AddColumn<Guid>(
                name: "UserId",
                table: "Testimonials",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_Testimonials_UserId",
                table: "Testimonials",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Testimonials_Users_UserId",
                table: "Testimonials",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Testimonials_Users_UserId",
                table: "Testimonials");

            migrationBuilder.DropIndex(
                name: "IX_Testimonials_UserId",
                table: "Testimonials");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Testimonials");
        }
    }
}
