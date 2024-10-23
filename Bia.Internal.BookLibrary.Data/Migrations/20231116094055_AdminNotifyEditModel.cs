using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Bia.Internal.BookLibrary.Data.Migrations
{
    public partial class AdminNotifyEditModel : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_AdminNotifies",
                table: "AdminNotifies");

            migrationBuilder.AddColumn<int>(
                name: "Id",
                table: "AdminNotifies",
                nullable: false,
                defaultValue: 0)
                .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn);

            migrationBuilder.AddPrimaryKey(
                name: "PK_AdminNotifies",
                table: "AdminNotifies",
                column: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_AdminNotifies",
                table: "AdminNotifies");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "AdminNotifies");

            migrationBuilder.AddPrimaryKey(
                name: "PK_AdminNotifies",
                table: "AdminNotifies",
                column: "NotifyId");
        }
    }
}
