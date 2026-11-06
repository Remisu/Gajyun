using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Gajyun.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialSchemaV13 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "dbo");

            migrationBuilder.CreateTable(
                name: "BookingStatus",
                schema: "dbo",
                columns: table => new
                {
                    BookingStatusID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StatusName = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                        .Annotation("Relational:DefaultConstraintName", "DF_BookingStatus_Active")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BookingStatus", x => x.BookingStatusID);
                });

            migrationBuilder.CreateTable(
                name: "Country",
                schema: "dbo",
                columns: table => new
                {
                    CountryID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CountryName = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    ISO2Code = table.Column<string>(type: "char(2)", nullable: true),
                    FlagUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Country", x => x.CountryID);
                });

            migrationBuilder.CreateTable(
                name: "DocumentType",
                schema: "dbo",
                columns: table => new
                {
                    DocumentTypeID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DocumentTypeName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                        .Annotation("Relational:DefaultConstraintName", "DF_DocumentType_Active")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DocumentType", x => x.DocumentTypeID);
                });

            migrationBuilder.CreateTable(
                name: "ExtraService",
                schema: "dbo",
                columns: table => new
                {
                    ExtraServiceID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ServiceName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    DefaultUnitPrice = table.Column<decimal>(type: "decimal(12,2)", precision: 12, scale: 2, nullable: false, defaultValue: 0m)
                        .Annotation("Relational:DefaultConstraintName", "DF_ExtraService_Price"),
                    CurrencyCode = table.Column<string>(type: "char(3)", nullable: false, defaultValue: "JPY")
                        .Annotation("Relational:DefaultConstraintName", "DF_ExtraService_Currency"),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                        .Annotation("Relational:DefaultConstraintName", "DF_ExtraService_Active")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExtraService", x => x.ExtraServiceID);
                    table.CheckConstraint("CK_ExtraService_Price", "[DefaultUnitPrice] >= 0");
                });

            migrationBuilder.CreateTable(
                name: "OTA",
                schema: "dbo",
                columns: table => new
                {
                    OTAID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OTAName = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    LogoUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                        .Annotation("Relational:DefaultConstraintName", "DF_OTA_Active")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OTA", x => x.OTAID);
                });

            migrationBuilder.CreateTable(
                name: "PaymentMethod",
                schema: "dbo",
                columns: table => new
                {
                    PaymentMethodID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MethodName = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                        .Annotation("Relational:DefaultConstraintName", "DF_PaymentMethod_Active")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaymentMethod", x => x.PaymentMethodID);
                });

            migrationBuilder.CreateTable(
                name: "ReservationGroup",
                schema: "dbo",
                columns: table => new
                {
                    GroupID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ExternalGroupReference = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: true),
                    LogoUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReservationGroup", x => x.GroupID);
                });

            migrationBuilder.CreateTable(
                name: "TaxPolicy",
                schema: "dbo",
                columns: table => new
                {
                    TaxPolicyID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PolicyName = table.Column<string>(type: "nvarchar(160)", maxLength: 160, nullable: false),
                    ValidFrom = table.Column<DateOnly>(type: "date", nullable: false),
                    ValidTo = table.Column<DateOnly>(type: "date", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                        .Annotation("Relational:DefaultConstraintName", "DF_TaxPolicy_Active"),
                    Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TaxPolicy", x => x.TaxPolicyID);
                    table.CheckConstraint("CK_TaxPolicy_Dates", "[ValidTo] IS NULL OR [ValidTo] > [ValidFrom]");
                });

            migrationBuilder.CreateTable(
                name: "Customer",
                schema: "dbo",
                columns: table => new
                {
                    CustomerID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FullName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Nickname = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    CountryID = table.Column<int>(type: "int", nullable: true),
                    PostalAddress = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    PhoneNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(254)", maxLength: 254, nullable: true),
                    IsRegularCustomer = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
                        .Annotation("Relational:DefaultConstraintName", "DF_Customer_Regular"),
                    CreatedAt = table.Column<DateTime>(type: "datetime2(0)", nullable: false, defaultValueSql: "SYSDATETIME()")
                        .Annotation("Relational:DefaultConstraintName", "DF_Customer_Created")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Customer", x => x.CustomerID);
                    table.ForeignKey(
                        name: "FK_Customer_Country",
                        column: x => x.CountryID,
                        principalSchema: "dbo",
                        principalTable: "Country",
                        principalColumn: "CountryID");
                });

            migrationBuilder.CreateTable(
                name: "ImportBatch",
                schema: "dbo",
                columns: table => new
                {
                    ImportBatchID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OTAID = table.Column<int>(type: "int", nullable: false),
                    SourceFileName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    ImportedAt = table.Column<DateTime>(type: "datetime2(0)", nullable: false, defaultValueSql: "SYSDATETIME()")
                        .Annotation("Relational:DefaultConstraintName", "DF_ImportBatch_Imported"),
                    SourceFileHash = table.Column<string>(type: "varchar(64)", nullable: true),
                    SourceEncoding = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    SourceFileContent = table.Column<byte[]>(type: "varbinary(max)", nullable: true),
                    ParserVersion = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ImportBatch", x => x.ImportBatchID);
                    table.ForeignKey(
                        name: "FK_ImportBatch_OTA",
                        column: x => x.OTAID,
                        principalSchema: "dbo",
                        principalTable: "OTA",
                        principalColumn: "OTAID");
                });

            migrationBuilder.CreateTable(
                name: "Reservation",
                schema: "dbo",
                columns: table => new
                {
                    ReservationID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OTAID = table.Column<int>(type: "int", nullable: false),
                    ExternalReservationNumber = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: true),
                    GroupID = table.Column<long>(type: "bigint", nullable: true),
                    BookingStatusID = table.Column<int>(type: "int", nullable: false),
                    SourcePaymentMethodID = table.Column<int>(type: "int", nullable: true),
                    BookedAt = table.Column<DateTime>(type: "datetime2(0)", nullable: true),
                    ModifiedAtSource = table.Column<DateTime>(type: "datetime2(0)", nullable: true),
                    CheckInDate = table.Column<DateOnly>(type: "date", nullable: false),
                    CheckOutDate = table.Column<DateOnly>(type: "date", nullable: false),
                    ExpectedGuestCount = table.Column<short>(type: "smallint", nullable: true),
                    ActualCheckInAt = table.Column<DateTime>(type: "datetime2(0)", nullable: true),
                    CheckInStatus = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "Pending")
                        .Annotation("Relational:DefaultConstraintName", "DF_Reservation_CheckInStatus"),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                        .Annotation("Relational:DefaultConstraintName", "DF_Reservation_Active"),
                    IsModified = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
                        .Annotation("Relational:DefaultConstraintName", "DF_Reservation_Modified"),
                    ConsecutiveStayInfo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RoomRateInfo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BookingDetails = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2(0)", nullable: false, defaultValueSql: "SYSDATETIME()")
                        .Annotation("Relational:DefaultConstraintName", "DF_Reservation_Created"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2(0)", nullable: false, defaultValueSql: "SYSDATETIME()")
                        .Annotation("Relational:DefaultConstraintName", "DF_Reservation_Updated")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Reservation", x => x.ReservationID);
                    table.CheckConstraint("CK_Reservation_CheckInStatus", "[CheckInStatus] IN ('Pending','CheckedIn','NoShow')");
                    table.CheckConstraint("CK_Reservation_CheckInTime", "([CheckInStatus] = 'CheckedIn' AND [ActualCheckInAt] IS NOT NULL) OR ([CheckInStatus] <> 'CheckedIn' AND [ActualCheckInAt] IS NULL)");
                    table.CheckConstraint("CK_Reservation_Dates", "[CheckOutDate] > [CheckInDate]");
                    table.CheckConstraint("CK_Reservation_Guests", "[ExpectedGuestCount] IS NULL OR [ExpectedGuestCount] > 0");
                    table.ForeignKey(
                        name: "FK_Reservation_BookingStatus",
                        column: x => x.BookingStatusID,
                        principalSchema: "dbo",
                        principalTable: "BookingStatus",
                        principalColumn: "BookingStatusID");
                    table.ForeignKey(
                        name: "FK_Reservation_Group",
                        column: x => x.GroupID,
                        principalSchema: "dbo",
                        principalTable: "ReservationGroup",
                        principalColumn: "GroupID");
                    table.ForeignKey(
                        name: "FK_Reservation_OTA",
                        column: x => x.OTAID,
                        principalSchema: "dbo",
                        principalTable: "OTA",
                        principalColumn: "OTAID");
                    table.ForeignKey(
                        name: "FK_Reservation_SourcePaymentMethod",
                        column: x => x.SourcePaymentMethodID,
                        principalSchema: "dbo",
                        principalTable: "PaymentMethod",
                        principalColumn: "PaymentMethodID");
                });

            migrationBuilder.CreateTable(
                name: "Room",
                schema: "dbo",
                columns: table => new
                {
                    RoomID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RoomName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Capacity = table.Column<short>(type: "smallint", nullable: false),
                    DefaultTaxPolicyID = table.Column<int>(type: "int", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                        .Annotation("Relational:DefaultConstraintName", "DF_Room_Active")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Room", x => x.RoomID);
                    table.CheckConstraint("CK_Room_Capacity", "[Capacity] > 0");
                    table.ForeignKey(
                        name: "FK_Room_DefaultTaxPolicy",
                        column: x => x.DefaultTaxPolicyID,
                        principalSchema: "dbo",
                        principalTable: "TaxPolicy",
                        principalColumn: "TaxPolicyID");
                });

            migrationBuilder.CreateTable(
                name: "TaxBracket",
                schema: "dbo",
                columns: table => new
                {
                    TaxBracketID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TaxPolicyID = table.Column<int>(type: "int", nullable: false),
                    MinPriceInclusive = table.Column<decimal>(type: "decimal(12,2)", precision: 12, scale: 2, nullable: false),
                    MaxPriceExclusive = table.Column<decimal>(type: "decimal(12,2)", precision: 12, scale: 2, nullable: true),
                    TaxPerPersonNight = table.Column<decimal>(type: "decimal(12,2)", precision: 12, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TaxBracket", x => x.TaxBracketID);
                    table.CheckConstraint("CK_TaxBracket_Amount", "[TaxPerPersonNight] >= 0");
                    table.CheckConstraint("CK_TaxBracket_Range", "[MinPriceInclusive] >= 0 AND ([MaxPriceExclusive] IS NULL OR [MaxPriceExclusive] > [MinPriceInclusive])");
                    table.ForeignKey(
                        name: "FK_TaxBracket_TaxPolicy",
                        column: x => x.TaxPolicyID,
                        principalSchema: "dbo",
                        principalTable: "TaxPolicy",
                        principalColumn: "TaxPolicyID");
                });

            migrationBuilder.CreateTable(
                name: "CustomerDocument",
                schema: "dbo",
                columns: table => new
                {
                    DocumentID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CustomerID = table.Column<long>(type: "bigint", nullable: false),
                    DocumentTypeID = table.Column<int>(type: "int", nullable: false),
                    FileStorageKey = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    PhotoStorageKey = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    ExpiryDate = table.Column<DateOnly>(type: "date", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2(0)", nullable: false, defaultValueSql: "SYSDATETIME()")
                        .Annotation("Relational:DefaultConstraintName", "DF_CustomerDocument_Created")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomerDocument", x => x.DocumentID);
                    table.ForeignKey(
                        name: "FK_CustomerDocument_Customer",
                        column: x => x.CustomerID,
                        principalSchema: "dbo",
                        principalTable: "Customer",
                        principalColumn: "CustomerID");
                    table.ForeignKey(
                        name: "FK_CustomerDocument_Type",
                        column: x => x.DocumentTypeID,
                        principalSchema: "dbo",
                        principalTable: "DocumentType",
                        principalColumn: "DocumentTypeID");
                });

            migrationBuilder.CreateTable(
                name: "Stay",
                schema: "dbo",
                columns: table => new
                {
                    StayID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CustomerID = table.Column<long>(type: "bigint", nullable: false),
                    StartDate = table.Column<DateOnly>(type: "date", nullable: false),
                    EndDate = table.Column<DateOnly>(type: "date", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2(0)", nullable: false, defaultValueSql: "SYSDATETIME()")
                        .Annotation("Relational:DefaultConstraintName", "DF_Stay_Created")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Stay", x => x.StayID);
                    table.CheckConstraint("CK_Stay_Dates", "[EndDate] > [StartDate]");
                    table.ForeignKey(
                        name: "FK_Stay_Customer",
                        column: x => x.CustomerID,
                        principalSchema: "dbo",
                        principalTable: "Customer",
                        principalColumn: "CustomerID");
                });

            migrationBuilder.CreateTable(
                name: "ImportRow",
                schema: "dbo",
                columns: table => new
                {
                    ImportRowID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ImportBatchID = table.Column<long>(type: "bigint", nullable: false),
                    SourceRowNumber = table.Column<int>(type: "int", nullable: false),
                    ReservationID = table.Column<long>(type: "bigint", nullable: true),
                    RawDetails = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RawPayload = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ParseStatus = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "Pending")
                        .Annotation("Relational:DefaultConstraintName", "DF_ImportRow_Status"),
                    ParseError = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    SourceRowHash = table.Column<string>(type: "varchar(64)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ImportRow", x => x.ImportRowID);
                    table.UniqueConstraint("UQ_ImportRow_BatchNumber", x => new { x.ImportBatchID, x.SourceRowNumber });
                    table.CheckConstraint("CK_ImportRow_Status", "[ParseStatus] IN ('Pending','Processed','Failed','Skipped')");
                    table.ForeignKey(
                        name: "FK_ImportRow_Batch",
                        column: x => x.ImportBatchID,
                        principalSchema: "dbo",
                        principalTable: "ImportBatch",
                        principalColumn: "ImportBatchID");
                    table.ForeignKey(
                        name: "FK_ImportRow_Reservation",
                        column: x => x.ReservationID,
                        principalSchema: "dbo",
                        principalTable: "Reservation",
                        principalColumn: "ReservationID");
                });

            migrationBuilder.CreateTable(
                name: "OTAStatementItem",
                schema: "dbo",
                columns: table => new
                {
                    OTAStatementItemID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ReservationID = table.Column<long>(type: "bigint", nullable: false),
                    RawLabel = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    RawText = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Amount = table.Column<decimal>(type: "decimal(12,2)", precision: 12, scale: 2, nullable: false),
                    CurrencyCode = table.Column<string>(type: "char(3)", nullable: false, defaultValue: "JPY")
                        .Annotation("Relational:DefaultConstraintName", "DF_OTAStatementItem_Currency"),
                    Classification = table.Column<string>(type: "nvarchar(25)", maxLength: 25, nullable: false, defaultValue: "Unclassified")
                        .Annotation("Relational:DefaultConstraintName", "DF_OTAStatementItem_Class"),
                    ResolutionNotes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ImportedAt = table.Column<DateTime>(type: "datetime2(0)", nullable: false, defaultValueSql: "SYSDATETIME()")
                        .Annotation("Relational:DefaultConstraintName", "DF_OTAStatementItem_Imported"),
                    SourceLineKey = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OTAStatementItem", x => x.OTAStatementItemID);
                    table.CheckConstraint("CK_OTAStatementItem_Amount", "[Amount] >= 0");
                    table.CheckConstraint("CK_OTAStatementItem_Classification", "[Classification] IN ('Unclassified','Commission','GuestDeposit','Payout','TaxWithheld','Other')");
                    table.ForeignKey(
                        name: "FK_OTAStatementItem_Reservation",
                        column: x => x.ReservationID,
                        principalSchema: "dbo",
                        principalTable: "Reservation",
                        principalColumn: "ReservationID");
                });

            migrationBuilder.CreateTable(
                name: "ReservationCharge",
                schema: "dbo",
                columns: table => new
                {
                    ChargeID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ReservationID = table.Column<long>(type: "bigint", nullable: false),
                    ExtraServiceID = table.Column<int>(type: "int", nullable: true),
                    ChargeType = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    ServiceDate = table.Column<DateOnly>(type: "date", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Quantity = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false, defaultValue: 1m)
                        .Annotation("Relational:DefaultConstraintName", "DF_ReservationCharge_Quantity"),
                    UnitPrice = table.Column<decimal>(type: "decimal(12,2)", precision: 12, scale: 2, nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false, computedColumnSql: "CONVERT(DECIMAL(18,2), [Quantity] * [UnitPrice])", stored: true),
                    CurrencyCode = table.Column<string>(type: "char(3)", nullable: false, defaultValue: "JPY")
                        .Annotation("Relational:DefaultConstraintName", "DF_ReservationCharge_Currency"),
                    SourceLineKey = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2(0)", nullable: false, defaultValueSql: "SYSDATETIME()")
                        .Annotation("Relational:DefaultConstraintName", "DF_ReservationCharge_Created")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReservationCharge", x => x.ChargeID);
                    table.CheckConstraint("CK_ReservationCharge_Quantity", "[Quantity] > 0");
                    table.CheckConstraint("CK_ReservationCharge_SignedPrice", "[ChargeType] IN ('Discount','Adjustment') OR [UnitPrice] >= 0");
                    table.CheckConstraint("CK_ReservationCharge_Type", "[ChargeType] IN ('Accommodation','Breakfast','Laundry','Towel','LateCheckIn','Tax','Discount','Adjustment','Other')");
                    table.ForeignKey(
                        name: "FK_ReservationCharge_Extra",
                        column: x => x.ExtraServiceID,
                        principalSchema: "dbo",
                        principalTable: "ExtraService",
                        principalColumn: "ExtraServiceID");
                    table.ForeignKey(
                        name: "FK_ReservationCharge_Reservation",
                        column: x => x.ReservationID,
                        principalSchema: "dbo",
                        principalTable: "Reservation",
                        principalColumn: "ReservationID");
                });

            migrationBuilder.CreateTable(
                name: "ReservationCustomer",
                schema: "dbo",
                columns: table => new
                {
                    ReservationID = table.Column<long>(type: "bigint", nullable: false),
                    CustomerID = table.Column<long>(type: "bigint", nullable: false),
                    IsLeadGuest = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
                        .Annotation("Relational:DefaultConstraintName", "DF_ReservationCustomer_Lead"),
                    AddedAt = table.Column<DateTime>(type: "datetime2(0)", nullable: false, defaultValueSql: "SYSDATETIME()")
                        .Annotation("Relational:DefaultConstraintName", "DF_ReservationCustomer_Added")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReservationCustomer", x => new { x.ReservationID, x.CustomerID });
                    table.ForeignKey(
                        name: "FK_ReservationCustomer_Customer",
                        column: x => x.CustomerID,
                        principalSchema: "dbo",
                        principalTable: "Customer",
                        principalColumn: "CustomerID");
                    table.ForeignKey(
                        name: "FK_ReservationCustomer_Reservation",
                        column: x => x.ReservationID,
                        principalSchema: "dbo",
                        principalTable: "Reservation",
                        principalColumn: "ReservationID");
                });

            migrationBuilder.CreateTable(
                name: "ReservationPayment",
                schema: "dbo",
                columns: table => new
                {
                    PaymentID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ReservationID = table.Column<long>(type: "bigint", nullable: false),
                    PaymentMethodID = table.Column<int>(type: "int", nullable: true),
                    Direction = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    CounterpartyType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    PaymentKind = table.Column<string>(type: "nvarchar(25)", maxLength: 25, nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(12,2)", precision: 12, scale: 2, nullable: false),
                    CurrencyCode = table.Column<string>(type: "char(3)", nullable: false, defaultValue: "JPY")
                        .Annotation("Relational:DefaultConstraintName", "DF_ReservationPayment_Currency"),
                    PaidAt = table.Column<DateTime>(type: "datetime2(0)", nullable: true),
                    ExternalReference = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReservationPayment", x => x.PaymentID);
                    table.CheckConstraint("CK_ReservationPayment_Amount", "[Amount] > 0");
                    table.CheckConstraint("CK_ReservationPayment_Direction", "[Direction] IN ('Inflow','Outflow')");
                    table.CheckConstraint("CK_ReservationPayment_Kind", "[PaymentKind] IN ('Payment','Refund','Payout','Other')");
                    table.CheckConstraint("CK_ReservationPayment_Party", "[CounterpartyType] IN ('Guest','OTA','Guesthouse','Other')");
                    table.ForeignKey(
                        name: "FK_ReservationPayment_Method",
                        column: x => x.PaymentMethodID,
                        principalSchema: "dbo",
                        principalTable: "PaymentMethod",
                        principalColumn: "PaymentMethodID");
                    table.ForeignKey(
                        name: "FK_ReservationPayment_Reservation",
                        column: x => x.ReservationID,
                        principalSchema: "dbo",
                        principalTable: "Reservation",
                        principalColumn: "ReservationID");
                });

            migrationBuilder.CreateTable(
                name: "Bed",
                schema: "dbo",
                columns: table => new
                {
                    BedID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RoomID = table.Column<int>(type: "int", nullable: false),
                    BedLabel = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                        .Annotation("Relational:DefaultConstraintName", "DF_Bed_Active")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Bed", x => x.BedID);
                    table.UniqueConstraint("UQ_Bed_BedRoom", x => new { x.BedID, x.RoomID });
                    table.UniqueConstraint("UQ_Bed_Room_Label", x => new { x.RoomID, x.BedLabel });
                    table.ForeignKey(
                        name: "FK_Bed_Room",
                        column: x => x.RoomID,
                        principalSchema: "dbo",
                        principalTable: "Room",
                        principalColumn: "RoomID");
                });

            migrationBuilder.CreateTable(
                name: "OTARoomMapping",
                schema: "dbo",
                columns: table => new
                {
                    OTARoomMappingID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OTAID = table.Column<int>(type: "int", nullable: true),
                    RoomID = table.Column<int>(type: "int", nullable: false),
                    ExternalRoomName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    NormalizedName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false, collation: "Latin1_General_100_BIN2"),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                        .Annotation("Relational:DefaultConstraintName", "DF_OTARoomMapping_Active"),
                    CreatedAt = table.Column<DateTime>(type: "datetime2(0)", nullable: false, defaultValueSql: "SYSDATETIME()")
                        .Annotation("Relational:DefaultConstraintName", "DF_OTARoomMapping_Created")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OTARoomMapping", x => x.OTARoomMappingID);
                    table.ForeignKey(
                        name: "FK_OTARoomMapping_OTA",
                        column: x => x.OTAID,
                        principalSchema: "dbo",
                        principalTable: "OTA",
                        principalColumn: "OTAID");
                    table.ForeignKey(
                        name: "FK_OTARoomMapping_Room",
                        column: x => x.RoomID,
                        principalSchema: "dbo",
                        principalTable: "Room",
                        principalColumn: "RoomID");
                });

            migrationBuilder.CreateTable(
                name: "ReservationTax",
                schema: "dbo",
                columns: table => new
                {
                    ReservationTaxID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ReservationID = table.Column<long>(type: "bigint", nullable: false),
                    ServiceDate = table.Column<DateOnly>(type: "date", nullable: false),
                    RoomID = table.Column<int>(type: "int", nullable: true),
                    LegalTaxPolicyID = table.Column<int>(type: "int", nullable: true),
                    AppliedTaxPolicyID = table.Column<int>(type: "int", nullable: true),
                    GuestCount = table.Column<short>(type: "smallint", nullable: true),
                    TaxablePricePerPerson = table.Column<decimal>(type: "decimal(12,2)", precision: 12, scale: 2, nullable: true),
                    LegalTaxAmount = table.Column<decimal>(type: "decimal(12,2)", precision: 12, scale: 2, nullable: true),
                    ChargedTaxAmount = table.Column<decimal>(type: "decimal(12,2)", precision: 12, scale: 2, nullable: true),
                    CalculationStatus = table.Column<string>(type: "nvarchar(24)", maxLength: 24, nullable: false, defaultValue: "PendingReview")
                        .Annotation("Relational:DefaultConstraintName", "DF_ReservationTax_Status"),
                    Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CalculatedAt = table.Column<DateTime>(type: "datetime2(0)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReservationTax", x => x.ReservationTaxID);
                    table.CheckConstraint("CK_ReservationTax_Amounts", "([TaxablePricePerPerson] IS NULL OR [TaxablePricePerPerson] >= 0) AND ([LegalTaxAmount] IS NULL OR [LegalTaxAmount] >= 0) AND ([ChargedTaxAmount] IS NULL OR [ChargedTaxAmount] >= 0)");
                    table.CheckConstraint("CK_ReservationTax_Guests", "[GuestCount] IS NULL OR [GuestCount] > 0");
                    table.CheckConstraint("CK_ReservationTax_Status", "[CalculationStatus] IN ('PendingReview','Calculated','Confirmed','NotApplicable')");
                    table.ForeignKey(
                        name: "FK_ReservationTax_AppliedPolicy",
                        column: x => x.AppliedTaxPolicyID,
                        principalSchema: "dbo",
                        principalTable: "TaxPolicy",
                        principalColumn: "TaxPolicyID");
                    table.ForeignKey(
                        name: "FK_ReservationTax_LegalPolicy",
                        column: x => x.LegalTaxPolicyID,
                        principalSchema: "dbo",
                        principalTable: "TaxPolicy",
                        principalColumn: "TaxPolicyID");
                    table.ForeignKey(
                        name: "FK_ReservationTax_Reservation",
                        column: x => x.ReservationID,
                        principalSchema: "dbo",
                        principalTable: "Reservation",
                        principalColumn: "ReservationID");
                    table.ForeignKey(
                        name: "FK_ReservationTax_Room",
                        column: x => x.RoomID,
                        principalSchema: "dbo",
                        principalTable: "Room",
                        principalColumn: "RoomID");
                });

            migrationBuilder.CreateTable(
                name: "StayMemo",
                schema: "dbo",
                columns: table => new
                {
                    MemoID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StayID = table.Column<long>(type: "bigint", nullable: false),
                    MemoText = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2(0)", nullable: false, defaultValueSql: "SYSDATETIME()")
                        .Annotation("Relational:DefaultConstraintName", "DF_StayMemo_Created")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StayMemo", x => x.MemoID);
                    table.ForeignKey(
                        name: "FK_StayMemo_Stay",
                        column: x => x.StayID,
                        principalSchema: "dbo",
                        principalTable: "Stay",
                        principalColumn: "StayID");
                });

            migrationBuilder.CreateTable(
                name: "StayReservation",
                schema: "dbo",
                columns: table => new
                {
                    StayID = table.Column<long>(type: "bigint", nullable: false),
                    ReservationID = table.Column<long>(type: "bigint", nullable: false),
                    AddedAt = table.Column<DateTime>(type: "datetime2(0)", nullable: false, defaultValueSql: "SYSDATETIME()")
                        .Annotation("Relational:DefaultConstraintName", "DF_StayReservation_Added")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StayReservation", x => new { x.StayID, x.ReservationID });
                    table.ForeignKey(
                        name: "FK_StayReservation_Reservation",
                        column: x => x.ReservationID,
                        principalSchema: "dbo",
                        principalTable: "Reservation",
                        principalColumn: "ReservationID");
                    table.ForeignKey(
                        name: "FK_StayReservation_Stay",
                        column: x => x.StayID,
                        principalSchema: "dbo",
                        principalTable: "Stay",
                        principalColumn: "StayID");
                });

            migrationBuilder.CreateTable(
                name: "PaymentAllocation",
                schema: "dbo",
                columns: table => new
                {
                    PaymentID = table.Column<long>(type: "bigint", nullable: false),
                    ChargeID = table.Column<long>(type: "bigint", nullable: false),
                    AllocatedAmount = table.Column<decimal>(type: "decimal(12,2)", precision: 12, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaymentAllocation", x => new { x.PaymentID, x.ChargeID });
                    table.CheckConstraint("CK_PaymentAllocation_Positive", "[AllocatedAmount] > 0");
                    table.ForeignKey(
                        name: "FK_PaymentAllocation_Charge",
                        column: x => x.ChargeID,
                        principalSchema: "dbo",
                        principalTable: "ReservationCharge",
                        principalColumn: "ChargeID");
                    table.ForeignKey(
                        name: "FK_PaymentAllocation_Payment",
                        column: x => x.PaymentID,
                        principalSchema: "dbo",
                        principalTable: "ReservationPayment",
                        principalColumn: "PaymentID");
                });

            migrationBuilder.CreateTable(
                name: "ReservationAccommodation",
                schema: "dbo",
                columns: table => new
                {
                    ReservationAccommodationID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ReservationID = table.Column<long>(type: "bigint", nullable: false),
                    RoomID = table.Column<int>(type: "int", nullable: false),
                    BedID = table.Column<int>(type: "int", nullable: true),
                    StartDate = table.Column<DateOnly>(type: "date", nullable: false),
                    EndDate = table.Column<DateOnly>(type: "date", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReservationAccommodation", x => x.ReservationAccommodationID);
                    table.CheckConstraint("CK_ReservationAccommodation_Dates", "[EndDate] > [StartDate]");
                    table.ForeignKey(
                        name: "FK_ReservationAccommodation_BedRoom",
                        columns: x => new { x.BedID, x.RoomID },
                        principalSchema: "dbo",
                        principalTable: "Bed",
                        principalColumns: new[] { "BedID", "RoomID" });
                    table.ForeignKey(
                        name: "FK_ReservationAccommodation_Reservation",
                        column: x => x.ReservationID,
                        principalSchema: "dbo",
                        principalTable: "Reservation",
                        principalColumn: "ReservationID");
                    table.ForeignKey(
                        name: "FK_ReservationAccommodation_Room",
                        column: x => x.RoomID,
                        principalSchema: "dbo",
                        principalTable: "Room",
                        principalColumn: "RoomID");
                });

            migrationBuilder.InsertData(
                schema: "dbo",
                table: "Country",
                columns: new[] { "CountryID", "CountryName", "FlagUrl", "ISO2Code" },
                values: new object[,]
                {
                    { 1, "Andorra", "pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/ad.png", "AD" },
                    { 2, "United Arab Emirates", "pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/ae.png", "AE" },
                    { 3, "Afghanistan", "pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/af.png", "AF" },
                    { 4, "Antigua & Barbuda", "pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/ag.png", "AG" },
                    { 5, "Anguilla", "pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/ai.png", "AI" },
                    { 6, "Albania", "pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/al.png", "AL" },
                    { 7, "Armenia", "pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/am.png", "AM" },
                    { 8, "Angola", "pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/ao.png", "AO" },
                    { 9, "Antarctica", "pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/aq.png", "AQ" },
                    { 10, "Argentina", "pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/ar.png", "AR" },
                    { 11, "American Samoa", "pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/as.png", "AS" },
                    { 12, "Austria", "pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/at.png", "AT" },
                    { 13, "Australia", "pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/au.png", "AU" },
                    { 14, "Aruba", "pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/aw.png", "AW" },
                    { 15, "Åland Islands", "pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/ax.png", "AX" },
                    { 16, "Azerbaijan", "pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/az.png", "AZ" },
                    { 17, "Bosnia & Herzegovina", "pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/ba.png", "BA" },
                    { 18, "Barbados", "pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/bb.png", "BB" },
                    { 19, "Bangladesh", "pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/bd.png", "BD" },
                    { 20, "Belgium", "pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/be.png", "BE" },
                    { 21, "Burkina Faso", "pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/bf.png", "BF" },
                    { 22, "Bulgaria", "pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/bg.png", "BG" },
                    { 23, "Bahrain", "pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/bh.png", "BH" },
                    { 24, "Burundi", "pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/bi.png", "BI" },
                    { 25, "Benin", "pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/bj.png", "BJ" },
                    { 26, "St. Barthélemy", "pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/bl.png", "BL" },
                    { 27, "Bermuda", "pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/bm.png", "BM" },
                    { 28, "Brunei", "pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/bn.png", "BN" },
                    { 29, "Bolivia", "pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/bo.png", "BO" },
                    { 30, "Bonaire, Sint Eustatius and Saba", "pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/bq.png", "BQ" },
                    { 31, "Brazil", "pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/br.png", "BR" },
                    { 32, "Bahamas", "pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/bs.png", "BS" },
                    { 33, "Bhutan", "pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/bt.png", "BT" },
                    { 34, "Bouvet Island", "pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/bv.png", "BV" },
                    { 35, "Botswana", "pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/bw.png", "BW" },
                    { 36, "Belarus", "pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/by.png", "BY" },
                    { 37, "Belize", "pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/bz.png", "BZ" },
                    { 38, "Canada", "pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/ca.png", "CA" },
                    { 39, "Cocos (Keeling) Islands", "pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/cc.png", "CC" },
                    { 40, "Congo (DRC)", "pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/cd.png", "CD" },
                    { 41, "Central African Republic", "pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/cf.png", "CF" },
                    { 42, "Congo", "pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/cg.png", "CG" },
                    { 43, "Switzerland", "pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/ch.png", "CH" },
                    { 44, "Côte d’Ivoire", "pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/ci.png", "CI" },
                    { 45, "Cook Islands", "pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/ck.png", "CK" },
                    { 46, "Chile", "pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/cl.png", "CL" },
                    { 47, "Cameroon", "pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/cm.png", "CM" },
                    { 48, "China", "pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/cn.png", "CN" },
                    { 49, "Colombia", "pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/co.png", "CO" },
                    { 50, "Costa Rica", "pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/cr.png", "CR" },
                    { 51, "Cuba", "pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/cu.png", "CU" },
                    { 52, "Cabo Verde", "pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/cv.png", "CV" },
                    { 53, "Curaçao", "pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/cw.png", "CW" },
                    { 54, "Christmas Island", "pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/cx.png", "CX" },
                    { 55, "Cyprus", "pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/cy.png", "CY" },
                    { 56, "Czechia", "pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/cz.png", "CZ" },
                    { 57, "Germany", "pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/de.png", "DE" },
                    { 58, "Djibouti", "pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/dj.png", "DJ" },
                    { 59, "Denmark", "pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/dk.png", "DK" },
                    { 60, "Dominica", "pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/dm.png", "DM" },
                    { 61, "Dominican Republic", "pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/do.png", "DO" },
                    { 62, "Algeria", "pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/dz.png", "DZ" },
                    { 63, "Ecuador", "pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/ec.png", "EC" },
                    { 64, "Estonia", "pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/ee.png", "EE" },
                    { 65, "Egypt", "pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/eg.png", "EG" },
                    { 66, "Western Sahara", "pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/eh.png", "EH" },
                    { 67, "Eritrea", "pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/er.png", "ER" },
                    { 68, "Spain", "pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/es.png", "ES" },
                    { 69, "Ethiopia", "pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/et.png", "ET" },
                    { 70, "Finland", "pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/fi.png", "FI" },
                    { 71, "Fiji", "pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/fj.png", "FJ" },
                    { 72, "Falkland Islands", "pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/fk.png", "FK" },
                    { 73, "Micronesia", "pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/fm.png", "FM" },
                    { 74, "Faroe Islands", "pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/fo.png", "FO" },
                    { 75, "France", "pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/fr.png", "FR" },
                    { 76, "Gabon", "pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/ga.png", "GA" },
                    { 77, "United Kingdom", "pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/gb.png", "GB" },
                    { 78, "Grenada", "pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/gd.png", "GD" },
                    { 79, "Georgia", "pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/ge.png", "GE" },
                    { 80, "French Guiana", "pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/gf.png", "GF" },
                    { 81, "Guernsey", "pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/gg.png", "GG" },
                    { 82, "Ghana", "pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/gh.png", "GH" },
                    { 83, "Gibraltar", "pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/gi.png", "GI" },
                    { 84, "Greenland", "pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/gl.png", "GL" },
                    { 85, "Gambia", "pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/gm.png", "GM" },
                    { 86, "Guinea", "pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/gn.png", "GN" },
                    { 87, "Guadeloupe", "pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/gp.png", "GP" },
                    { 88, "Equatorial Guinea", "pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/gq.png", "GQ" },
                    { 89, "Greece", "pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/gr.png", "GR" },
                    { 90, "South Georgia and the South Sandwich Islands", "pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/gs.png", "GS" },
                    { 91, "Guatemala", "pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/gt.png", "GT" },
                    { 92, "Guam", "pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/gu.png", "GU" },
                    { 93, "Guinea-Bissau", "pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/gw.png", "GW" },
                    { 94, "Guyana", "pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/gy.png", "GY" },
                    { 95, "Hong Kong SAR", "pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/hk.png", "HK" },
                    { 96, "Heard Island and McDonald Islands", "pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/hm.png", "HM" },
                    { 97, "Honduras", "pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/hn.png", "HN" },
                    { 98, "Croatia", "pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/hr.png", "HR" },
                    { 99, "Haiti", "pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/ht.png", "HT" },
                    { 100, "Hungary", "pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/hu.png", "HU" },
                    { 101, "Indonesia", "pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/id.png", "ID" },
                    { 102, "Ireland", "pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/ie.png", "IE" },
                    { 103, "Israel", "pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/il.png", "IL" },
                    { 104, "Isle of Man", "pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/im.png", "IM" },
                    { 105, "India", "pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/in.png", "IN" },
                    { 106, "British Indian Ocean Territory", "pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/io.png", "IO" },
                    { 107, "Iraq", "pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/iq.png", "IQ" },
                    { 108, "Iran", "pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/ir.png", "IR" },
                    { 109, "Iceland", "pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/is.png", "IS" },
                    { 110, "Italy", "pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/it.png", "IT" },
                    { 111, "Jersey", "pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/je.png", "JE" },
                    { 112, "Jamaica", "pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/jm.png", "JM" },
                    { 113, "Jordan", "pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/jo.png", "JO" },
                    { 114, "Japan", "pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/jp.png", "JP" },
                    { 115, "Kenya", "pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/ke.png", "KE" },
                    { 116, "Kyrgyzstan", "pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/kg.png", "KG" },
                    { 117, "Cambodia", "pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/kh.png", "KH" },
                    { 118, "Kiribati", "pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/ki.png", "KI" },
                    { 119, "Comoros", "pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/km.png", "KM" },
                    { 120, "St. Kitts & Nevis", "pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/kn.png", "KN" },
                    { 121, "North Korea", "pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/kp.png", "KP" },
                    { 122, "Korea", "pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/kr.png", "KR" },
                    { 123, "Kuwait", "pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/kw.png", "KW" },
                    { 124, "Cayman Islands", "pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/ky.png", "KY" },
                    { 125, "Kazakhstan", "pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/kz.png", "KZ" },
                    { 126, "Laos", "pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/la.png", "LA" },
                    { 127, "Lebanon", "pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/lb.png", "LB" },
                    { 128, "St. Lucia", "pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/lc.png", "LC" },
                    { 129, "Liechtenstein", "pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/li.png", "LI" },
                    { 130, "Sri Lanka", "pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/lk.png", "LK" },
                    { 131, "Liberia", "pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/lr.png", "LR" },
                    { 132, "Lesotho", "pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/ls.png", "LS" },
                    { 133, "Lithuania", "pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/lt.png", "LT" },
                    { 134, "Luxembourg", "pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/lu.png", "LU" },
                    { 135, "Latvia", "pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/lv.png", "LV" },
                    { 136, "Libya", "pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/ly.png", "LY" },
                    { 137, "Morocco", "pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/ma.png", "MA" },
                    { 138, "Monaco", "pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/mc.png", "MC" },
                    { 139, "Moldova", "pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/md.png", "MD" },
                    { 140, "Montenegro", "pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/me.png", "ME" },
                    { 141, "St. Martin", "pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/mf.png", "MF" },
                    { 142, "Madagascar", "pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/mg.png", "MG" },
                    { 143, "Marshall Islands", "pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/mh.png", "MH" },
                    { 144, "North Macedonia", "pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/mk.png", "MK" },
                    { 145, "Mali", "pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/ml.png", "ML" },
                    { 146, "Myanmar", "pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/mm.png", "MM" },
                    { 147, "Mongolia", "pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/mn.png", "MN" },
                    { 148, "Macao SAR", "pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/mo.png", "MO" },
                    { 149, "Northern Mariana Islands", "pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/mp.png", "MP" },
                    { 150, "Martinique", "pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/mq.png", "MQ" },
                    { 151, "Mauritania", "pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/mr.png", "MR" },
                    { 152, "Montserrat", "pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/ms.png", "MS" },
                    { 153, "Malta", "pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/mt.png", "MT" },
                    { 154, "Mauritius", "pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/mu.png", "MU" },
                    { 155, "Maldives", "pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/mv.png", "MV" },
                    { 156, "Malawi", "pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/mw.png", "MW" },
                    { 157, "Mexico", "pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/mx.png", "MX" },
                    { 158, "Malaysia", "pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/my.png", "MY" },
                    { 159, "Mozambique", "pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/mz.png", "MZ" },
                    { 160, "Namibia", "pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/na.png", "NA" },
                    { 161, "New Caledonia", "pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/nc.png", "NC" },
                    { 162, "Niger", "pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/ne.png", "NE" },
                    { 163, "Norfolk Island", "pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/nf.png", "NF" },
                    { 164, "Nigeria", "pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/ng.png", "NG" },
                    { 165, "Nicaragua", "pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/ni.png", "NI" },
                    { 166, "Netherlands", "pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/nl.png", "NL" },
                    { 167, "Norway", "pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/no.png", "NO" },
                    { 168, "Nepal", "pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/np.png", "NP" },
                    { 169, "Nauru", "pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/nr.png", "NR" },
                    { 170, "Niue", "pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/nu.png", "NU" },
                    { 171, "New Zealand", "pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/nz.png", "NZ" },
                    { 172, "Oman", "pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/om.png", "OM" },
                    { 173, "Panama", "pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/pa.png", "PA" },
                    { 174, "Peru", "pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/pe.png", "PE" },
                    { 175, "French Polynesia", "pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/pf.png", "PF" },
                    { 176, "Papua New Guinea", "pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/pg.png", "PG" },
                    { 177, "Philippines", "pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/ph.png", "PH" },
                    { 178, "Pakistan", "pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/pk.png", "PK" },
                    { 179, "Poland", "pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/pl.png", "PL" },
                    { 180, "St. Pierre & Miquelon", "pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/pm.png", "PM" },
                    { 181, "Pitcairn Islands", "pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/pn.png", "PN" },
                    { 182, "Puerto Rico", "pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/pr.png", "PR" },
                    { 183, "Palestinian Authority", "pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/ps.png", "PS" },
                    { 184, "Portugal", "pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/pt.png", "PT" },
                    { 185, "Palau", "pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/pw.png", "PW" },
                    { 186, "Paraguay", "pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/py.png", "PY" },
                    { 187, "Qatar", "pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/qa.png", "QA" },
                    { 188, "Réunion", "pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/re.png", "RE" },
                    { 189, "Romania", "pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/ro.png", "RO" },
                    { 190, "Serbia", "pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/rs.png", "RS" },
                    { 191, "Russia", "pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/ru.png", "RU" },
                    { 192, "Rwanda", "pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/rw.png", "RW" },
                    { 193, "Saudi Arabia", "pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/sa.png", "SA" },
                    { 194, "Solomon Islands", "pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/sb.png", "SB" },
                    { 195, "Seychelles", "pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/sc.png", "SC" },
                    { 196, "Sudan", "pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/sd.png", "SD" },
                    { 197, "Sweden", "pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/se.png", "SE" },
                    { 198, "Singapore", "pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/sg.png", "SG" },
                    { 199, "St Helena, Ascension, Tristan da Cunha", "pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/sh.png", "SH" },
                    { 200, "Slovenia", "pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/si.png", "SI" },
                    { 201, "Svalbard & Jan Mayen", "pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/sj.png", "SJ" },
                    { 202, "Slovakia", "pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/sk.png", "SK" },
                    { 203, "Sierra Leone", "pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/sl.png", "SL" },
                    { 204, "San Marino", "pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/sm.png", "SM" },
                    { 205, "Senegal", "pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/sn.png", "SN" },
                    { 206, "Somalia", "pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/so.png", "SO" },
                    { 207, "Suriname", "pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/sr.png", "SR" },
                    { 208, "South Sudan", "pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/ss.png", "SS" },
                    { 209, "São Tomé & Príncipe", "pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/st.png", "ST" },
                    { 210, "El Salvador", "pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/sv.png", "SV" },
                    { 211, "Sint Maarten", "pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/sx.png", "SX" },
                    { 212, "Syria", "pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/sy.png", "SY" },
                    { 213, "Eswatini", "pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/sz.png", "SZ" },
                    { 214, "Turks & Caicos Islands", "pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/tc.png", "TC" },
                    { 215, "Chad", "pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/td.png", "TD" },
                    { 216, "French Southern Territories", "pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/tf.png", "TF" },
                    { 217, "Togo", "pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/tg.png", "TG" },
                    { 218, "Thailand", "pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/th.png", "TH" },
                    { 219, "Tajikistan", "pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/tj.png", "TJ" },
                    { 220, "Tokelau", "pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/tk.png", "TK" },
                    { 221, "Timor-Leste", "pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/tl.png", "TL" },
                    { 222, "Turkmenistan", "pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/tm.png", "TM" },
                    { 223, "Tunisia", "pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/tn.png", "TN" },
                    { 224, "Tonga", "pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/to.png", "TO" },
                    { 225, "Türkiye", "pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/tr.png", "TR" },
                    { 226, "Trinidad & Tobago", "pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/tt.png", "TT" },
                    { 227, "Tuvalu", "pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/tv.png", "TV" },
                    { 228, "Taiwan", "pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/tw.png", "TW" },
                    { 229, "Tanzania", "pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/tz.png", "TZ" },
                    { 230, "Ukraine", "pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/ua.png", "UA" },
                    { 231, "Uganda", "pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/ug.png", "UG" },
                    { 232, "U.S. Outlying Islands", "pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/um.png", "UM" },
                    { 233, "United States", "pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/us.png", "US" },
                    { 234, "Uruguay", "pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/uy.png", "UY" },
                    { 235, "Uzbekistan", "pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/uz.png", "UZ" },
                    { 236, "Vatican City", "pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/va.png", "VA" },
                    { 237, "St. Vincent & Grenadines", "pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/vc.png", "VC" },
                    { 238, "Venezuela", "pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/ve.png", "VE" },
                    { 239, "British Virgin Islands", "pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/vg.png", "VG" },
                    { 240, "U.S. Virgin Islands", "pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/vi.png", "VI" },
                    { 241, "Vietnam", "pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/vn.png", "VN" },
                    { 242, "Vanuatu", "pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/vu.png", "VU" },
                    { 243, "Wallis & Futuna", "pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/wf.png", "WF" },
                    { 244, "Samoa", "pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/ws.png", "WS" },
                    { 245, "Kosovo (user-assigned XK code)", "pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/xk.png", "XK" },
                    { 246, "Yemen", "pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/ye.png", "YE" },
                    { 247, "Mayotte", "pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/yt.png", "YT" },
                    { 248, "South Africa", "pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/za.png", "ZA" },
                    { 249, "Zambia", "pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/zm.png", "ZM" },
                    { 250, "Zimbabwe", "pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/zw.png", "ZW" }
                });

            migrationBuilder.InsertData(
                schema: "dbo",
                table: "DocumentType",
                columns: new[] { "DocumentTypeID", "DocumentTypeName", "IsActive" },
                values: new object[,]
                {
                    { 1, "Passport", true },
                    { 2, "Driver's license", true },
                    { 3, "Foreign residence card", true },
                    { 4, "Other", true }
                });

            migrationBuilder.InsertData(
                schema: "dbo",
                table: "OTA",
                columns: new[] { "OTAID", "IsActive", "LogoUrl", "OTAName" },
                values: new object[,]
                {
                    { 1, true, "pack://application:,,,/Gajyun.Desktop;component/Assets/Logos/Booking-Logo.png", "Booking.com" },
                    { 2, true, "pack://application:,,,/Gajyun.Desktop;component/Assets/Logos/Agoda-logo.png", "Agoda" },
                    { 3, true, "pack://application:,,,/Gajyun.Desktop;component/Assets/Logos/hostelworld-seeklogo.png", "Hostelworld" },
                    { 4, true, "pack://application:,,,/Gajyun.Desktop;component/Assets/Logos/gajyun-logo.png", "予約情報入力(直接販売)" }
                });

            migrationBuilder.InsertData(
                schema: "dbo",
                table: "PaymentMethod",
                columns: new[] { "PaymentMethodID", "IsActive", "MethodName" },
                values: new object[,]
                {
                    { 1, true, "Cash" },
                    { 2, true, "Credit Card" },
                    { 3, true, "Bank Transfer" },
                    { 4, true, "Other" },
                    { 5, true, "Internet payment" }
                });

            migrationBuilder.InsertData(
                schema: "dbo",
                table: "TaxPolicy",
                columns: new[] { "TaxPolicyID", "IsActive", "Notes", "PolicyName", "ValidFrom", "ValidTo" },
                values: new object[,]
                {
                    { 1, true, "Verified Kyoto City rates; staff approved 2026-11-06. Meals and consumption tax excluded. https://www.city.kyoto.lg.jp/gyozai/page/0000236942.html", "Kyoto City Accommodation Tax 2025", new DateOnly(2018, 10, 1), new DateOnly(2026, 3, 1) },
                    { 2, true, "Verified Kyoto City rates; staff approved 2026-11-06. Meals and consumption tax excluded. https://www.city.kyoto.lg.jp/gyozai/page/0000236942.html", "Kyoto City Accommodation Tax 2026", new DateOnly(2026, 3, 1), null }
                });

            migrationBuilder.InsertData(
                schema: "dbo",
                table: "TaxBracket",
                columns: new[] { "TaxBracketID", "MaxPriceExclusive", "MinPriceInclusive", "TaxPerPersonNight", "TaxPolicyID" },
                values: new object[,]
                {
                    { 1, 20000m, 0m, 200m, 1 },
                    { 2, 50000m, 20000m, 500m, 1 },
                    { 3, null, 50000m, 1000m, 1 },
                    { 4, 6000m, 0m, 200m, 2 },
                    { 5, 20000m, 6000m, 400m, 2 },
                    { 6, 50000m, 20000m, 1000m, 2 },
                    { 7, 100000m, 50000m, 4000m, 2 },
                    { 8, null, 100000m, 10000m, 2 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_BookingStatus_StatusName",
                schema: "dbo",
                table: "BookingStatus",
                column: "StatusName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UX_Country_ISO2Code",
                schema: "dbo",
                table: "Country",
                column: "ISO2Code",
                unique: true,
                filter: "[ISO2Code] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Customer_CountryID",
                schema: "dbo",
                table: "Customer",
                column: "CountryID");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerDocument_CustomerID",
                schema: "dbo",
                table: "CustomerDocument",
                column: "CustomerID");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerDocument_DocumentTypeID",
                schema: "dbo",
                table: "CustomerDocument",
                column: "DocumentTypeID");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentType_DocumentTypeName",
                schema: "dbo",
                table: "DocumentType",
                column: "DocumentTypeName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ExtraService_ServiceName",
                schema: "dbo",
                table: "ExtraService",
                column: "ServiceName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ImportBatch_OTAID",
                schema: "dbo",
                table: "ImportBatch",
                column: "OTAID");

            migrationBuilder.CreateIndex(
                name: "IX_ImportRow_ReservationID_SourceRowHash",
                schema: "dbo",
                table: "ImportRow",
                columns: new[] { "ReservationID", "SourceRowHash" });

            migrationBuilder.CreateIndex(
                name: "IX_OTA_OTAName",
                schema: "dbo",
                table: "OTA",
                column: "OTAName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OTARoomMapping_NormalizedName",
                schema: "dbo",
                table: "OTARoomMapping",
                column: "NormalizedName");

            migrationBuilder.CreateIndex(
                name: "UX_OTARoomMapping_GenericRoomName",
                schema: "dbo",
                table: "OTARoomMapping",
                columns: new[] { "RoomID", "NormalizedName" },
                unique: true,
                filter: "[OTAID] IS NULL AND [IsActive] = 1");

            migrationBuilder.CreateIndex(
                name: "UX_OTARoomMapping_OTAName",
                schema: "dbo",
                table: "OTARoomMapping",
                columns: new[] { "OTAID", "NormalizedName" },
                unique: true,
                filter: "[OTAID] IS NOT NULL AND [IsActive] = 1");

            migrationBuilder.CreateIndex(
                name: "UX_OTAStatementItem_SourceLine",
                schema: "dbo",
                table: "OTAStatementItem",
                columns: new[] { "ReservationID", "SourceLineKey" },
                unique: true,
                filter: "[SourceLineKey] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentAllocation_ChargeID",
                schema: "dbo",
                table: "PaymentAllocation",
                column: "ChargeID");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentMethod_MethodName",
                schema: "dbo",
                table: "PaymentMethod",
                column: "MethodName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Reservation_BookedAt",
                schema: "dbo",
                table: "Reservation",
                column: "BookedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Reservation_BookingStatusID",
                schema: "dbo",
                table: "Reservation",
                column: "BookingStatusID");

            migrationBuilder.CreateIndex(
                name: "IX_Reservation_CheckInDate",
                schema: "dbo",
                table: "Reservation",
                column: "CheckInDate");

            migrationBuilder.CreateIndex(
                name: "IX_Reservation_CheckOutDate",
                schema: "dbo",
                table: "Reservation",
                column: "CheckOutDate");

            migrationBuilder.CreateIndex(
                name: "IX_Reservation_GroupID",
                schema: "dbo",
                table: "Reservation",
                column: "GroupID");

            migrationBuilder.CreateIndex(
                name: "IX_Reservation_SourcePaymentMethodID",
                schema: "dbo",
                table: "Reservation",
                column: "SourcePaymentMethodID");

            migrationBuilder.CreateIndex(
                name: "UX_Reservation_OTA_ExternalNumber",
                schema: "dbo",
                table: "Reservation",
                columns: new[] { "OTAID", "ExternalReservationNumber" },
                unique: true,
                filter: "[ExternalReservationNumber] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_ReservationAccommodation_BedID_RoomID",
                schema: "dbo",
                table: "ReservationAccommodation",
                columns: new[] { "BedID", "RoomID" });

            migrationBuilder.CreateIndex(
                name: "IX_ReservationAccommodation_Inventory",
                schema: "dbo",
                table: "ReservationAccommodation",
                columns: new[] { "RoomID", "BedID", "StartDate", "EndDate" });

            migrationBuilder.CreateIndex(
                name: "IX_ReservationAccommodation_ReservationID",
                schema: "dbo",
                table: "ReservationAccommodation",
                column: "ReservationID");

            migrationBuilder.CreateIndex(
                name: "IX_ReservationCharge_ExtraServiceID",
                schema: "dbo",
                table: "ReservationCharge",
                column: "ExtraServiceID");

            migrationBuilder.CreateIndex(
                name: "IX_ReservationCharge_Reservation",
                schema: "dbo",
                table: "ReservationCharge",
                column: "ReservationID");

            migrationBuilder.CreateIndex(
                name: "UX_ReservationCharge_SourceLine",
                schema: "dbo",
                table: "ReservationCharge",
                columns: new[] { "ReservationID", "SourceLineKey" },
                unique: true,
                filter: "[SourceLineKey] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_ReservationCustomer_CustomerID",
                schema: "dbo",
                table: "ReservationCustomer",
                column: "CustomerID");

            migrationBuilder.CreateIndex(
                name: "UX_ReservationCustomer_OneLead",
                schema: "dbo",
                table: "ReservationCustomer",
                column: "ReservationID",
                unique: true,
                filter: "[IsLeadGuest] = 1");

            migrationBuilder.CreateIndex(
                name: "IX_ReservationPayment_PaymentMethodID",
                schema: "dbo",
                table: "ReservationPayment",
                column: "PaymentMethodID");

            migrationBuilder.CreateIndex(
                name: "IX_ReservationPayment_Reservation",
                schema: "dbo",
                table: "ReservationPayment",
                column: "ReservationID");

            migrationBuilder.CreateIndex(
                name: "IX_ReservationTax_AppliedTaxPolicyID",
                schema: "dbo",
                table: "ReservationTax",
                column: "AppliedTaxPolicyID");

            migrationBuilder.CreateIndex(
                name: "IX_ReservationTax_LegalTaxPolicyID",
                schema: "dbo",
                table: "ReservationTax",
                column: "LegalTaxPolicyID");

            migrationBuilder.CreateIndex(
                name: "IX_ReservationTax_ReservationID_ServiceDate",
                schema: "dbo",
                table: "ReservationTax",
                columns: new[] { "ReservationID", "ServiceDate" });

            migrationBuilder.CreateIndex(
                name: "IX_ReservationTax_RoomID",
                schema: "dbo",
                table: "ReservationTax",
                column: "RoomID");

            migrationBuilder.CreateIndex(
                name: "IX_Room_DefaultTaxPolicyID",
                schema: "dbo",
                table: "Room",
                column: "DefaultTaxPolicyID");

            migrationBuilder.CreateIndex(
                name: "IX_Room_RoomName",
                schema: "dbo",
                table: "Room",
                column: "RoomName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Stay_CustomerID",
                schema: "dbo",
                table: "Stay",
                column: "CustomerID");

            migrationBuilder.CreateIndex(
                name: "IX_StayMemo_StayID",
                schema: "dbo",
                table: "StayMemo",
                column: "StayID");

            migrationBuilder.CreateIndex(
                name: "IX_StayReservation_ReservationID",
                schema: "dbo",
                table: "StayReservation",
                column: "ReservationID");

            migrationBuilder.CreateIndex(
                name: "IX_TaxBracket_TaxPolicyID_MinPriceInclusive",
                schema: "dbo",
                table: "TaxBracket",
                columns: new[] { "TaxPolicyID", "MinPriceInclusive" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TaxPolicy_PolicyName",
                schema: "dbo",
                table: "TaxPolicy",
                column: "PolicyName",
                unique: true);

            const string financialSummaryView = """
                CREATE VIEW dbo.vwReservationFinancialSummary AS
                WITH C AS (
                    SELECT ReservationID, CurrencyCode, SUM(Amount) AS TotalCharged
                    FROM dbo.ReservationCharge GROUP BY ReservationID, CurrencyCode
                ), P AS (
                    SELECT ReservationID, CurrencyCode,
                        SUM(CASE WHEN Direction = 'Inflow' THEN Amount ELSE -Amount END) AS NetCashMovement
                    FROM dbo.ReservationPayment GROUP BY ReservationID, CurrencyCode
                ), Keys AS (
                    SELECT ReservationID, CurrencyCode FROM C
                    UNION
                    SELECT ReservationID, CurrencyCode FROM P
                )
                SELECT R.ReservationID, K.CurrencyCode,
                    ISNULL(C.TotalCharged, 0) AS TotalCharged,
                    ISNULL(P.NetCashMovement, 0) AS NetCashMovement,
                    ISNULL(C.TotalCharged, 0) - ISNULL(P.NetCashMovement, 0) AS UnreconciledDifference
                FROM dbo.Reservation R
                JOIN Keys K ON K.ReservationID = R.ReservationID
                LEFT JOIN C ON C.ReservationID = K.ReservationID AND C.CurrencyCode = K.CurrencyCode
                LEFT JOIN P ON P.ReservationID = K.ReservationID AND P.CurrencyCode = K.CurrencyCode;
                """;
            migrationBuilder.Sql($"EXEC(N'{financialSummaryView.Replace("'", "''")}');");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP VIEW dbo.vwReservationFinancialSummary;");
            migrationBuilder.DropTable(
                name: "CustomerDocument",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "ImportRow",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "OTARoomMapping",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "OTAStatementItem",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "PaymentAllocation",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "ReservationAccommodation",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "ReservationCustomer",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "ReservationTax",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "StayMemo",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "StayReservation",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "TaxBracket",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "DocumentType",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "ImportBatch",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "ReservationCharge",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "ReservationPayment",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "Bed",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "Stay",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "ExtraService",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "Reservation",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "Room",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "Customer",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "BookingStatus",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "ReservationGroup",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "OTA",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "PaymentMethod",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "TaxPolicy",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "Country",
                schema: "dbo");
        }
    }
}
