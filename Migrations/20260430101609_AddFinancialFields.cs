using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AutoServiceApp.Migrations
{
    /// <inheritdoc />
    public partial class AddFinancialFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "TotalCost",
                table: "WorkOrders",
                newName: "PartsCost");

            migrationBuilder.AddColumn<DateTime>(
                name: "CompletionDate",
                table: "WorkOrders",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "LaborCost",
                table: "WorkOrders",
                type: "TEXT",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CompletionDate",
                table: "WorkOrders");

            migrationBuilder.DropColumn(
                name: "LaborCost",
                table: "WorkOrders");

            migrationBuilder.RenameColumn(
                name: "PartsCost",
                table: "WorkOrders",
                newName: "TotalCost");
        }
    }
}
