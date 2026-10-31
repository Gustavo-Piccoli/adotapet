using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AdotaPet.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddAdopterRequestsIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_AdoptionRequests_AdopterId_CreatedAt",
                table: "AdoptionRequests",
                columns: new[] { "AdopterId", "CreatedAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_AdoptionRequests_AdopterId_CreatedAt",
                table: "AdoptionRequests");
        }
    }
}
