using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Web_Form.Migrations
{
    /// <inheritdoc />
    public partial class ChangeFormIdToGuid : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Drop the foreign key constraint
            migrationBuilder.DropForeignKey(
                name: "FK_tbl_Questions_tbl_Forms",
                table: "tbl_Questions");

            // Convert existing string GUIDs to actual GUIDs
            migrationBuilder.Sql(@"
            UPDATE tbl_Questions
            SET FormId = CAST(FormId AS UNIQUEIDENTIFIER)
            WHERE ISDATE(FormId) = 1
        ");

            // Alter the column type
            migrationBuilder.AlterColumn<Guid>(
                name: "FormId",
                table: "tbl_Questions",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            // Re-create the foreign key constraint
            migrationBuilder.AddForeignKey(
                name: "FK_tbl_Questions_tbl_Forms",
                table: "tbl_Questions",
                column: "FormId",
                principalTable: "tbl_Forms",
                principalColumn: "FormId",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Drop the foreign key constraint
            migrationBuilder.DropForeignKey(
                name: "FK_tbl_Questions_tbl_Forms",
                table: "tbl_Questions");

            // Revert the column type
            migrationBuilder.AlterColumn<string>(
                name: "FormId",
                table: "tbl_Questions",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(Guid));

            // Re-create the foreign key constraint
            migrationBuilder.AddForeignKey(
                name: "FK_tbl_Questions_tbl_Forms",
                table: "tbl_Questions",
                column: "FormId",
                principalTable: "tbl_Forms",
                principalColumn: "FormId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}

