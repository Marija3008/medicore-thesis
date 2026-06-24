using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace mangocare_api.Migrations
{
    /// <inheritdoc />
    public partial class AddDocumentTrash : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedaAt",
                table: "MedicalDocuments",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "MedicalDocuments",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DeletedaAt",
                table: "MedicalDocuments");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "MedicalDocuments");
        }
    }
}
