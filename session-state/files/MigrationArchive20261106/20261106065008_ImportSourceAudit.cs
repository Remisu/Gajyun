using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Gajyun.Data.Migrations
{
    /// <inheritdoc />
    public partial class ImportSourceAudit : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ImportRow_ReservationID",
                schema: "dbo",
                table: "ImportRow");

            migrationBuilder.AddColumn<string>(
                name: "SourceRowHash",
                schema: "dbo",
                table: "ImportRow",
                type: "varchar(64)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ParserVersion",
                schema: "dbo",
                table: "ImportBatch",
                type: "nvarchar(40)",
                maxLength: 40,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SourceEncoding",
                schema: "dbo",
                table: "ImportBatch",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "SourceFileContent",
                schema: "dbo",
                table: "ImportBatch",
                type: "varbinary(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SourceFileHash",
                schema: "dbo",
                table: "ImportBatch",
                type: "varchar(64)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ImportRow_ReservationID_SourceRowHash",
                schema: "dbo",
                table: "ImportRow",
                columns: new[] { "ReservationID", "SourceRowHash" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ImportRow_ReservationID_SourceRowHash",
                schema: "dbo",
                table: "ImportRow");

            migrationBuilder.DropColumn(
                name: "SourceRowHash",
                schema: "dbo",
                table: "ImportRow");

            migrationBuilder.DropColumn(
                name: "ParserVersion",
                schema: "dbo",
                table: "ImportBatch");

            migrationBuilder.DropColumn(
                name: "SourceEncoding",
                schema: "dbo",
                table: "ImportBatch");

            migrationBuilder.DropColumn(
                name: "SourceFileContent",
                schema: "dbo",
                table: "ImportBatch");

            migrationBuilder.DropColumn(
                name: "SourceFileHash",
                schema: "dbo",
                table: "ImportBatch");

            migrationBuilder.CreateIndex(
                name: "IX_ImportRow_ReservationID",
                schema: "dbo",
                table: "ImportRow",
                column: "ReservationID");
        }
    }
}
