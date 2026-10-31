using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AdotPet.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddAdoptionRequestRowVersion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "AdoptionRequests",
                type: "rowversion",
                rowVersion: true,
                nullable: false,
                defaultValue: new byte[0]);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "AdoptionRequests");
        }
    }
}
