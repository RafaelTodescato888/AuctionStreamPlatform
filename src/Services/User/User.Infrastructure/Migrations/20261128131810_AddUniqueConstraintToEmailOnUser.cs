using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace User.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddUniqueConstraintToEmailOnUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SellersApplcations");

            migrationBuilder.DropIndex(
                name: "IX_UsersProfile_Email",
                table: "UsersProfile");

            migrationBuilder.CreateTable(
                name: "SellersApplications",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    ReviewedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    ReviewComment = table.Column<string>(type: "text", nullable: true),
                    SubmittedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ReviewedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SellersApplications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SellersApplications_Users_ReviewedBy",
                        column: x => x.ReviewedBy,
                        principalTable: "Users",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_SellersApplications_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UsersProfile_Email",
                table: "UsersProfile",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SellersApplications_ReviewedBy",
                table: "SellersApplications",
                column: "ReviewedBy");

            migrationBuilder.CreateIndex(
                name: "IX_SellersApplications_UserId",
                table: "SellersApplications",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SellersApplications");

            migrationBuilder.DropIndex(
                name: "IX_UsersProfile_Email",
                table: "UsersProfile");

            migrationBuilder.CreateTable(
                name: "SellersApplcations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ReviewedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ReviewComment = table.Column<string>(type: "text", nullable: true),
                    ReviewedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    SubmittedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SellersApplcations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SellersApplcations_Users_ReviewedBy",
                        column: x => x.ReviewedBy,
                        principalTable: "Users",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_SellersApplcations_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UsersProfile_Email",
                table: "UsersProfile",
                column: "Email");

            migrationBuilder.CreateIndex(
                name: "IX_SellersApplcations_ReviewedBy",
                table: "SellersApplcations",
                column: "ReviewedBy");

            migrationBuilder.CreateIndex(
                name: "IX_SellersApplcations_UserId",
                table: "SellersApplcations",
                column: "UserId");
        }
    }
}
