using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BloodDonation.Migrations
{
    /// <inheritdoc />
    public partial class DonationDescription : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DonationDescription",
                table: "Users",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DonationDescription",
                table: "Users");
        }
    }
}
