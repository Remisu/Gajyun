using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Gajyun.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialSchemaV11 : Migration
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
                        .Annotation("Relational:DefaultConstraintName", "DF_ImportBatch_Imported")
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
                    ParseError = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true)
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
                name: "ReservationMemo",
                schema: "dbo",
                columns: table => new
                {
                    MemoID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ReservationID = table.Column<long>(type: "bigint", nullable: false),
                    MemoText = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2(0)", nullable: false, defaultValueSql: "SYSDATETIME()")
                        .Annotation("Relational:DefaultConstraintName", "DF_ReservationMemo_Created")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReservationMemo", x => x.MemoID);
                    table.ForeignKey(
                        name: "FK_ReservationMemo_Reservation",
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
                    ExternalRoomCode = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: true, collation: "Latin1_General_100_BIN2"),
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
                name: "IX_ImportRow_ReservationID",
                schema: "dbo",
                table: "ImportRow",
                column: "ReservationID");

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
                name: "UX_OTARoomMapping_Code",
                schema: "dbo",
                table: "OTARoomMapping",
                columns: new[] { "OTAID", "ExternalRoomCode" },
                unique: true,
                filter: "[OTAID] IS NOT NULL AND [ExternalRoomCode] IS NOT NULL AND [IsActive] = 1");

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
                name: "IX_ReservationMemo_ReservationID",
                schema: "dbo",
                table: "ReservationMemo",
                column: "ReservationID");

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
            // CREATE VIEW must run in its own batch, including in idempotent scripts.
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
                name: "ReservationMemo",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "ReservationTax",
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
                name: "PaymentMethod",
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
                name: "TaxPolicy",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "Country",
                schema: "dbo");
        }
    }
}
