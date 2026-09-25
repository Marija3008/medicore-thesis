using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MediCore.Api.Migrations
{
    /// <inheritdoc />
    public partial class RenameDeletedaAtToDeletedAt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "DeletedaAt",
                table: "MedicalDocuments",
                newName: "DeletedAt");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "DeletedAt",
                table: "MedicalDocuments",
                newName: "DeletedaAt");
        }
    }
}
