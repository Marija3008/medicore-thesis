using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace mangocare_api.Migrations
{
    /// <inheritdoc />
    public partial class AddDocumentAndChatOwnership : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "OwnerUserId",
                table: "MedicalDocuments",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OwnerUserId",
                table: "Chats",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_MedicalDocuments_OwnerUserId",
                table: "MedicalDocuments",
                column: "OwnerUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Chats_OwnerUserId",
                table: "Chats",
                column: "OwnerUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Chats_AspNetUsers_OwnerUserId",
                table: "Chats",
                column: "OwnerUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_MedicalDocuments_AspNetUsers_OwnerUserId",
                table: "MedicalDocuments",
                column: "OwnerUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Chats_AspNetUsers_OwnerUserId",
                table: "Chats");

            migrationBuilder.DropForeignKey(
                name: "FK_MedicalDocuments_AspNetUsers_OwnerUserId",
                table: "MedicalDocuments");

            migrationBuilder.DropIndex(
                name: "IX_MedicalDocuments_OwnerUserId",
                table: "MedicalDocuments");

            migrationBuilder.DropIndex(
                name: "IX_Chats_OwnerUserId",
                table: "Chats");

            migrationBuilder.DropColumn(
                name: "OwnerUserId",
                table: "MedicalDocuments");

            migrationBuilder.DropColumn(
                name: "OwnerUserId",
                table: "Chats");
        }
    }
}
