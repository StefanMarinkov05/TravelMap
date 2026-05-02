using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TravelMap.Migrations
{
    /// <inheritdoc />
    public partial class Init2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "admin-user-id",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "1ae0541d-622b-43da-be76-c1eafbc8f957", "AQAAAAIAAYagAAAAEOgPl0BF7YuZKvxa1MgLnZ95de88KlRQQzG0M4Vk2p52nQog6+DYUCIlS0JX8xqewQ==", "92782197-f397-4df2-9df5-175028b6bebd" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "regular-user-id",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "a94fe70a-8652-471a-8095-84eda54d7635", "AQAAAAIAAYagAAAAECc7HWsKYypBG0B2OAX8Arm7Uj8RY8xzUT1hS1Sz7zoJkLPKa+LS/HSMKpWgSE6PbQ==", "6785a420-52d1-4a32-a49e-a95bdc957563" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "admin-user-id",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "4ba41e78-3242-43e8-a62c-297a8cb85e1f", "AQAAAAIAAYagAAAAEMd4WCUiprj9ylhQMcobcfmPwCFr4LmGXYGNJ73gmfrhS7VkhHeWKgDMqf8dEuSiOg==", "c0694b94-b6fc-4e2a-9abc-5484845d2a2f" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "regular-user-id",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "25254d8d-a8d5-45bd-a0b0-9cf1741d99ea", "AQAAAAIAAYagAAAAEOTAyqKymimbT5OpvfPNj9vwS8Hg38oxCuawlM0AwavgQ5C59Qbx865JUUFIo8YOnw==", "2d0a2e74-cc58-4564-af34-b7d8ce6d700a" });
        }
    }
}
