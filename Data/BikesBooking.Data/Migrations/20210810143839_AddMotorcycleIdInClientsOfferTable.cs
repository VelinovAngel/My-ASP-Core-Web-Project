﻿namespace BikesBooking.Data.Migrations
{
    using Microsoft.EntityFrameworkCore.Migrations;

    public partial class AddMotorcycleIdInClientsOfferTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "MotorcycleId",
                table: "ClientsOffers",
                type: "int",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MotorcycleId",
                table: "ClientsOffers");
        }
    }
}
