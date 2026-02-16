using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace vizin.Migrations
{
    /// <inheritdoc />
    public partial class AddTotalCostToBooking : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "TotalCost",
                schema: "sistema_locacao",
                table: "tb_booking",
                type: "numeric(18,2)",
                nullable: false,
                defaultValue: 0m);
        }
    }
}
