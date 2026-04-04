using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DoAnWeb.Migrations
{
    /// <inheritdoc />
    public partial class AddSpecialy : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Specialty",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AveragePatientLoad = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Specialty", x => x.Id);
                });

            migrationBuilder.AddColumn<int>(
                name: "SpecialtyId",
                table: "Doctors",
                type: "int",
                nullable: true);

            migrationBuilder.Sql(
                """
                INSERT INTO Specialty (Name, AveragePatientLoad)
                SELECT DISTINCT LTRIM(RTRIM(Specialty)), 0
                FROM Doctors
                WHERE Specialty IS NOT NULL AND LTRIM(RTRIM(Specialty)) <> ''
                  AND NOT EXISTS (
                      SELECT 1
                      FROM Specialty s
                      WHERE s.Name = LTRIM(RTRIM(Doctors.Specialty))
                  );
                """);

            migrationBuilder.Sql(
                """
                IF NOT EXISTS (SELECT 1 FROM Specialty WHERE Name = N'Noi tong quat')
                BEGIN
                    INSERT INTO Specialty (Name, AveragePatientLoad)
                    VALUES (N'Noi tong quat', 0);
                END
                """);

            migrationBuilder.Sql(
                """
                UPDATE d
                SET d.SpecialtyId = s.Id
                FROM Doctors d
                INNER JOIN Specialty s ON s.Name = LTRIM(RTRIM(d.Specialty));
                """);

            migrationBuilder.Sql(
                """
                UPDATE Doctors
                SET SpecialtyId = (
                    SELECT TOP 1 Id
                    FROM Specialty
                    WHERE Name = N'Noi tong quat'
                )
                WHERE SpecialtyId IS NULL;
                """);

            migrationBuilder.AlterColumn<int>(
                name: "SpecialtyId",
                table: "Doctors",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.DropColumn(
                name: "Specialty",
                table: "Doctors");

            migrationBuilder.CreateIndex(
                name: "IX_Doctors_SpecialtyId",
                table: "Doctors",
                column: "SpecialtyId");

            migrationBuilder.AddForeignKey(
                name: "FK_Doctors_Specialty_SpecialtyId",
                table: "Doctors",
                column: "SpecialtyId",
                principalTable: "Specialty",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Doctors_Specialty_SpecialtyId",
                table: "Doctors");

            migrationBuilder.DropTable(
                name: "Specialty");

            migrationBuilder.DropIndex(
                name: "IX_Doctors_SpecialtyId",
                table: "Doctors");

            migrationBuilder.DropColumn(
                name: "SpecialtyId",
                table: "Doctors");

            migrationBuilder.AddColumn<string>(
                name: "Specialty",
                table: "Doctors",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
