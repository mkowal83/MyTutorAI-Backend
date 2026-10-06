using System;
using Microsoft.EntityFrameworkCore.Migrations;


namespace MyTutorAI.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddLearningProgress : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Progress",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RecentMistakes = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    HomeworkStatus = table.Column<int>(type: "int", nullable: false),
                    NextFocus = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Progress", x => x.UserId);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Progress");
        }
    }
}
