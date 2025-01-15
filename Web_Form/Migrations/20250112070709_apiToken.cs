using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Web_Form.Migrations
{
    /// <inheritdoc />
    public partial class apiToken : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TblFormFormId",
                table: "tbl_Response",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TblQuestionQuestionId",
                table: "tbl_Response",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ApiTokens",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Token = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApiTokens", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ApiTokens_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_tbl_Response_TblFormFormId",
                table: "tbl_Response",
                column: "TblFormFormId");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_Response_TblQuestionQuestionId",
                table: "tbl_Response",
                column: "TblQuestionQuestionId");

            migrationBuilder.CreateIndex(
                name: "IX_ApiTokens_UserId",
                table: "ApiTokens",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_tbl_Response_tbl_Forms_TblFormFormId",
                table: "tbl_Response",
                column: "TblFormFormId",
                principalTable: "tbl_Forms",
                principalColumn: "FormID");

            migrationBuilder.AddForeignKey(
                name: "FK_tbl_Response_tbl_Questions_TblQuestionQuestionId",
                table: "tbl_Response",
                column: "TblQuestionQuestionId",
                principalTable: "tbl_Questions",
                principalColumn: "QuestionID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_tbl_Response_tbl_Forms_TblFormFormId",
                table: "tbl_Response");

            migrationBuilder.DropForeignKey(
                name: "FK_tbl_Response_tbl_Questions_TblQuestionQuestionId",
                table: "tbl_Response");

            migrationBuilder.DropTable(
                name: "ApiTokens");

            migrationBuilder.DropIndex(
                name: "IX_tbl_Response_TblFormFormId",
                table: "tbl_Response");

            migrationBuilder.DropIndex(
                name: "IX_tbl_Response_TblQuestionQuestionId",
                table: "tbl_Response");

            migrationBuilder.DropColumn(
                name: "TblFormFormId",
                table: "tbl_Response");

            migrationBuilder.DropColumn(
                name: "TblQuestionQuestionId",
                table: "tbl_Response");
        }
    }
}
