using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DoAnWeb.Migrations
{
    /// <inheritdoc />
    public partial class AddReminderFieldsToAppointment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsReminder24hSent",
                table: "Appointments",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsReminder2hSent",
                table: "Appointments",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "Reminder24hSentAt",
                table: "Appointments",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "Reminder2hSentAt",
                table: "Appointments",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsReminder24hSent",
                table: "Appointments");

            migrationBuilder.DropColumn(
                name: "IsReminder2hSent",
                table: "Appointments");

            migrationBuilder.DropColumn(
                name: "Reminder24hSentAt",
                table: "Appointments");

            migrationBuilder.DropColumn(
                name: "Reminder2hSentAt",
                table: "Appointments");
        }
    }
}
