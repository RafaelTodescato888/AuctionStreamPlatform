using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace User.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateDocumentToContainsCnpj : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Document",
                table: "Users",
                type: "character varying(14)",
                maxLength: 14,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(11)",
                oldMaxLength: 11);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Document",
                table: "Users",
                type: "character varying(11)",
                maxLength: 11,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(14)",
                oldMaxLength: 14);
        }
    }
}
