using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Anamel.DL.Migrations
{
    /// <inheritdoc />
    public partial class addrate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Rate",
                table: "Products",
                type: "int",
                nullable: false,
                defaultValue: 3);

            migrationBuilder.Sql(@"
                UPDATE Products 
                SET Rate = 3 + (ABS(CHECKSUM(NEWID())) % 3)
                WHERE Rate = 3;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Rate",
                table: "Products");
        }
    }
}
