/*
 Guesthouse Gajyun - Database schema v1.0 (Microsoft SQL Server 2019+)
 Execute in an EMPTY, pre-created database. Not a migration.
 One OTA CSV row = one reservation when reservation identifier is unique.
 Daily listed price = FULL room price for that night (not per guest).
 OTA `deposit` = UNCLASSIFIED unless the OTA-specific meaning is confirmed.
 Monetary amounts in separate charges, receipts and OTA statement items.
 All guesthouse-local timestamps use local Japanese wall clock (Asia/Tokyo) and DATETIME2.
*/
SET NOCOUNT ON;
GO

CREATE TABLE dbo.OTA (
    OTAID INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    OTAName NVARCHAR(120) NOT NULL UNIQUE,
    LogoUrl NVARCHAR(500) NULL,
    IsActive BIT NOT NULL CONSTRAINT DF_OTA_Active DEFAULT 1
);
GO
CREATE TABLE dbo.BookingStatus (
    BookingStatusID INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    StatusName NVARCHAR(60) NOT NULL UNIQUE,
    IsActive BIT NOT NULL CONSTRAINT DF_BookingStatus_Active DEFAULT 1
);
GO
CREATE TABLE dbo.Country (
    CountryID INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    CountryName NVARCHAR(120) NOT NULL,
    ISO2Code CHAR(2) NULL,
    FlagUrl NVARCHAR(500) NULL
);
GO
CREATE UNIQUE INDEX UX_Country_ISO2Code ON dbo.Country(ISO2Code) WHERE ISO2Code IS NOT NULL;
GO
CREATE TABLE dbo.Room (
    RoomID INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    RoomName NVARCHAR(100) NOT NULL UNIQUE,
    IsActive BIT NOT NULL CONSTRAINT DF_Room_Active DEFAULT 1
);
GO
CREATE TABLE dbo.Bed (
    BedID INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    RoomID INT NOT NULL,
    BedLabel NVARCHAR(80) NOT NULL,
    IsActive BIT NOT NULL CONSTRAINT DF_Bed_Active DEFAULT 1,
    CONSTRAINT FK_Bed_Room FOREIGN KEY (RoomID) REFERENCES dbo.Room(RoomID),
    CONSTRAINT UQ_Bed_Room_Label UNIQUE (RoomID, BedLabel),
    CONSTRAINT UQ_Bed_BedRoom UNIQUE (BedID, RoomID)
);
GO
CREATE TABLE dbo.PaymentMethod (
    PaymentMethodID INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    MethodName NVARCHAR(80) NOT NULL UNIQUE,
    IsActive BIT NOT NULL CONSTRAINT DF_PaymentMethod_Active DEFAULT 1
);
GO
CREATE TABLE dbo.DocumentType (
    DocumentTypeID INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    DocumentTypeName NVARCHAR(100) NOT NULL UNIQUE,
    IsActive BIT NOT NULL CONSTRAINT DF_DocumentType_Active DEFAULT 1
);
GO
CREATE TABLE dbo.ExtraService (
    ExtraServiceID INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    ServiceName NVARCHAR(100) NOT NULL UNIQUE,
    DefaultUnitPrice DECIMAL(12,2) NOT NULL CONSTRAINT DF_ExtraService_Price DEFAULT 0,
    CurrencyCode CHAR(3) NOT NULL CONSTRAINT DF_ExtraService_Currency DEFAULT 'JPY',
    IsActive BIT NOT NULL CONSTRAINT DF_ExtraService_Active DEFAULT 1,
    CONSTRAINT CK_ExtraService_Price CHECK (DefaultUnitPrice >= 0)
);
GO
CREATE TABLE dbo.ReservationGroup (
    GroupID BIGINT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    ExternalGroupReference NVARCHAR(120) NULL,
    Notes NVARCHAR(1000) NULL
);
GO
CREATE TABLE dbo.Customer (
    CustomerID BIGINT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    FullName NVARCHAR(255) NOT NULL,
    Nickname NVARCHAR(255) NULL,
    CountryID INT NULL,
    PostalAddress NVARCHAR(500) NULL,
    PhoneNumber NVARCHAR(50) NULL,
    Email NVARCHAR(254) NULL,
    IsRegularCustomer BIT NOT NULL CONSTRAINT DF_Customer_Regular DEFAULT 0,
    CreatedAt DATETIME2(0) NOT NULL CONSTRAINT DF_Customer_Created DEFAULT SYSDATETIME(),
    CONSTRAINT FK_Customer_Country FOREIGN KEY (CountryID) REFERENCES dbo.Country(CountryID)
);
GO
CREATE TABLE dbo.Reservation (
    ReservationID BIGINT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    OTAID INT NOT NULL,
    ExternalReservationNumber NVARCHAR(120) NULL,
    GroupID BIGINT NULL,
    BookingStatusID INT NOT NULL,
    BookedAt DATETIME2(0) NULL,
    ModifiedAtSource DATETIME2(0) NULL,
    CheckInDate DATE NOT NULL,
    CheckOutDate DATE NOT NULL,
    ExpectedGuestCount SMALLINT NULL,
    ActualCheckInAt DATETIME2(0) NULL,
    CheckInStatus NVARCHAR(20) NOT NULL CONSTRAINT DF_Reservation_CheckInStatus DEFAULT 'Pending',
    IsActive BIT NOT NULL CONSTRAINT DF_Reservation_Active DEFAULT 1,
    IsModified BIT NOT NULL CONSTRAINT DF_Reservation_Modified DEFAULT 0,
    ConsecutiveStayInfo NVARCHAR(MAX) NULL,
    RoomRateInfo NVARCHAR(MAX) NULL,
    BookingDetails NVARCHAR(MAX) NULL,
    CreatedAt DATETIME2(0) NOT NULL CONSTRAINT DF_Reservation_Created DEFAULT SYSDATETIME(),
    UpdatedAt DATETIME2(0) NOT NULL CONSTRAINT DF_Reservation_Updated DEFAULT SYSDATETIME(),
    CONSTRAINT FK_Reservation_OTA FOREIGN KEY (OTAID) REFERENCES dbo.OTA(OTAID),
    CONSTRAINT FK_Reservation_Group FOREIGN KEY (GroupID) REFERENCES dbo.ReservationGroup(GroupID),
    CONSTRAINT FK_Reservation_BookingStatus FOREIGN KEY (BookingStatusID) REFERENCES dbo.BookingStatus(BookingStatusID),
    CONSTRAINT CK_Reservation_Dates CHECK (CheckOutDate > CheckInDate),
    CONSTRAINT CK_Reservation_Guests CHECK (ExpectedGuestCount IS NULL OR ExpectedGuestCount > 0),
    CONSTRAINT CK_Reservation_CheckInStatus CHECK (CheckInStatus IN ('Pending','CheckedIn','NoShow')),
    CONSTRAINT CK_Reservation_CheckInTime CHECK
       ((CheckInStatus = 'CheckedIn' AND ActualCheckInAt IS NOT NULL)
        OR (CheckInStatus <> 'CheckedIn' AND ActualCheckInAt IS NULL))
);
GO
-- One reservation identifier must not be duplicated FOR THE SAME OTA.
CREATE UNIQUE INDEX UX_Reservation_OTA_ExternalNumber
    ON dbo.Reservation(OTAID, ExternalReservationNumber)
    WHERE ExternalReservationNumber IS NOT NULL;
GO
CREATE INDEX IX_Reservation_CheckInDate ON dbo.Reservation(CheckInDate);
CREATE INDEX IX_Reservation_CheckOutDate ON dbo.Reservation(CheckOutDate);
GO

-- Guests are many-to-many. ExpectedGuestCount remains distinct from identifiable guests.
CREATE TABLE dbo.ReservationCustomer (
    ReservationID BIGINT NOT NULL,
    CustomerID BIGINT NOT NULL,
    IsLeadGuest BIT NOT NULL CONSTRAINT DF_ReservationCustomer_Lead DEFAULT 0,
    AddedAt DATETIME2(0) NOT NULL CONSTRAINT DF_ReservationCustomer_Added DEFAULT SYSDATETIME(),
    CONSTRAINT PK_ReservationCustomer PRIMARY KEY (ReservationID, CustomerID),
    CONSTRAINT FK_ReservationCustomer_Reservation FOREIGN KEY (ReservationID) REFERENCES dbo.Reservation(ReservationID),
    CONSTRAINT FK_ReservationCustomer_Customer FOREIGN KEY (CustomerID) REFERENCES dbo.Customer(CustomerID)
);
GO
CREATE UNIQUE INDEX UX_ReservationCustomer_OneLead
    ON dbo.ReservationCustomer(ReservationID) WHERE IsLeadGuest = 1;
GO

-- Assign individual beds or a whole room over a specified interval.
-- For an entire private room BedID must be NULL; for a bed use BedID.
-- Pair FK makes sure a selected bed belongs to the selected room.
CREATE TABLE dbo.ReservationAccommodation (
    ReservationAccommodationID BIGINT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    ReservationID BIGINT NOT NULL,
    RoomID INT NOT NULL,
    BedID INT NULL,
    StartDate DATE NOT NULL,
    EndDate DATE NOT NULL,
    Notes NVARCHAR(300) NULL,
    CONSTRAINT FK_ReservationAccommodation_Reservation FOREIGN KEY (ReservationID) REFERENCES dbo.Reservation(ReservationID),
    CONSTRAINT FK_ReservationAccommodation_Room FOREIGN KEY (RoomID) REFERENCES dbo.Room(RoomID),
    CONSTRAINT FK_ReservationAccommodation_BedRoom FOREIGN KEY (BedID,RoomID) REFERENCES dbo.Bed(BedID,RoomID),
    CONSTRAINT CK_ReservationAccommodation_Dates CHECK (EndDate > StartDate)
);
GO
CREATE INDEX IX_ReservationAccommodation_Inventory
    ON dbo.ReservationAccommodation(RoomID, BedID, StartDate, EndDate);
GO

-- Every billable item appears ONCE here. ChargeType may be Accommodation,
-- Breakfast, Laundry, Towel, LateCheckIn, Tax, Discount, Adjustment, Other.
-- For mixed accommodation+breakfast plans, do NOT split amounts without a source breakdown.
-- A negative Amount is allowed ONLY for a discount or adjustment.
CREATE TABLE dbo.ReservationCharge (
    ChargeID BIGINT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    ReservationID BIGINT NOT NULL,
    ExtraServiceID INT NULL,
    ChargeType NVARCHAR(30) NOT NULL,
    ServiceDate DATE NULL,
    Description NVARCHAR(500) NULL,
    Quantity DECIMAL(10,2) NOT NULL CONSTRAINT DF_ReservationCharge_Quantity DEFAULT 1,
    UnitPrice DECIMAL(12,2) NOT NULL,
    Amount AS CONVERT(DECIMAL(18,2), Quantity * UnitPrice) PERSISTED,
    CurrencyCode CHAR(3) NOT NULL CONSTRAINT DF_ReservationCharge_Currency DEFAULT 'JPY',
    SourceLineKey NVARCHAR(150) NULL,
    CreatedAt DATETIME2(0) NOT NULL CONSTRAINT DF_ReservationCharge_Created DEFAULT SYSDATETIME(),
    CONSTRAINT FK_ReservationCharge_Reservation FOREIGN KEY (ReservationID) REFERENCES dbo.Reservation(ReservationID),
    CONSTRAINT FK_ReservationCharge_Extra FOREIGN KEY (ExtraServiceID) REFERENCES dbo.ExtraService(ExtraServiceID),
    CONSTRAINT CK_ReservationCharge_Quantity CHECK (Quantity > 0),
    CONSTRAINT CK_ReservationCharge_Type CHECK
       (ChargeType IN ('Accommodation','Breakfast','Laundry','Towel','LateCheckIn','Tax','Discount','Adjustment','Other')),
    CONSTRAINT CK_ReservationCharge_SignedPrice CHECK
       ((ChargeType IN ('Discount','Adjustment')) OR UnitPrice >= 0)
);
GO
CREATE UNIQUE INDEX UX_ReservationCharge_SourceLine
    ON dbo.ReservationCharge(ReservationID,SourceLineKey)
    WHERE SourceLineKey IS NOT NULL;
CREATE INDEX IX_ReservationCharge_Reservation ON dbo.ReservationCharge(ReservationID);
GO

-- Actual financial movements, separate from charges.
-- Money flow perspective: positive amount stored with direction Inflow/Outflow.
-- Payer/payee is identified with CounterpartyType; never infer it from 'deposit'.
CREATE TABLE dbo.ReservationPayment (
    PaymentID BIGINT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    ReservationID BIGINT NOT NULL,
    PaymentMethodID INT NULL,
    Direction NVARCHAR(10) NOT NULL,
    CounterpartyType NVARCHAR(20) NOT NULL,
    PaymentKind NVARCHAR(25) NOT NULL,
    Amount DECIMAL(12,2) NOT NULL,
    CurrencyCode CHAR(3) NOT NULL CONSTRAINT DF_ReservationPayment_Currency DEFAULT 'JPY',
    PaidAt DATETIME2(0) NULL,
    ExternalReference NVARCHAR(150) NULL,
    Notes NVARCHAR(500) NULL,
    CONSTRAINT FK_ReservationPayment_Reservation FOREIGN KEY (ReservationID) REFERENCES dbo.Reservation(ReservationID),
    CONSTRAINT FK_ReservationPayment_Method FOREIGN KEY (PaymentMethodID) REFERENCES dbo.PaymentMethod(PaymentMethodID),
    CONSTRAINT CK_ReservationPayment_Direction CHECK (Direction IN ('Inflow','Outflow')),
    CONSTRAINT CK_ReservationPayment_Party CHECK (CounterpartyType IN ('Guest','OTA','Guesthouse','Other')),
    CONSTRAINT CK_ReservationPayment_Kind CHECK (PaymentKind IN ('Payment','Refund','Payout','Other')),
    CONSTRAINT CK_ReservationPayment_Amount CHECK (Amount > 0)
);
GO
CREATE INDEX IX_ReservationPayment_Reservation ON dbo.ReservationPayment(ReservationID);
GO

-- OTA statement/import figures. Unknown 'deposit' is recorded here and never
-- automatically converted to a charge, payment, commission or revenue.
CREATE TABLE dbo.OTAStatementItem (
    OTAStatementItemID BIGINT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    ReservationID BIGINT NOT NULL,
    RawLabel NVARCHAR(100) NOT NULL,
    RawText NVARCHAR(500) NULL,
    Amount DECIMAL(12,2) NOT NULL,
    CurrencyCode CHAR(3) NOT NULL CONSTRAINT DF_OTAStatementItem_Currency DEFAULT 'JPY',
    Classification NVARCHAR(25) NOT NULL CONSTRAINT DF_OTAStatementItem_Class DEFAULT 'Unclassified',
    ResolutionNotes NVARCHAR(500) NULL,
    ImportedAt DATETIME2(0) NOT NULL CONSTRAINT DF_OTAStatementItem_Imported DEFAULT SYSDATETIME(),
    SourceLineKey NVARCHAR(150) NULL,
    CONSTRAINT FK_OTAStatementItem_Reservation FOREIGN KEY (ReservationID) REFERENCES dbo.Reservation(ReservationID),
    CONSTRAINT CK_OTAStatementItem_Classification CHECK
      (Classification IN ('Unclassified','Commission','GuestDeposit','Payout','TaxWithheld','Other')),
    CONSTRAINT CK_OTAStatementItem_Amount CHECK (Amount >= 0)
);
GO
CREATE UNIQUE INDEX UX_OTAStatementItem_SourceLine
    ON dbo.OTAStatementItem(ReservationID,SourceLineKey)
    WHERE SourceLineKey IS NOT NULL;
GO

-- Allocate receipts/refunds to charges when item-level paid status matters.
-- Remaining unallocated payments are permitted, e.g. advance payments.
CREATE TABLE dbo.PaymentAllocation (
    PaymentID BIGINT NOT NULL,
    ChargeID BIGINT NOT NULL,
    AllocatedAmount DECIMAL(12,2) NOT NULL,
    CONSTRAINT PK_PaymentAllocation PRIMARY KEY (PaymentID,ChargeID),
    CONSTRAINT FK_PaymentAllocation_Payment FOREIGN KEY (PaymentID) REFERENCES dbo.ReservationPayment(PaymentID),
    CONSTRAINT FK_PaymentAllocation_Charge FOREIGN KEY (ChargeID) REFERENCES dbo.ReservationCharge(ChargeID),
    CONSTRAINT CK_PaymentAllocation_Positive CHECK (AllocatedAmount > 0)
);
GO

CREATE TABLE dbo.ReservationMemo (
    MemoID BIGINT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    ReservationID BIGINT NOT NULL,
    MemoText NVARCHAR(MAX) NOT NULL,
    CreatedAt DATETIME2(0) NOT NULL CONSTRAINT DF_ReservationMemo_Created DEFAULT SYSDATETIME(),
    CONSTRAINT FK_ReservationMemo_Reservation FOREIGN KEY (ReservationID) REFERENCES dbo.Reservation(ReservationID)
);
GO
CREATE TABLE dbo.CustomerDocument (
    DocumentID BIGINT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    CustomerID BIGINT NOT NULL,
    DocumentTypeID INT NOT NULL,
    FileStorageKey NVARCHAR(1000) NULL,
    PhotoStorageKey NVARCHAR(1000) NULL,
    ExpiryDate DATE NULL,
    CreatedAt DATETIME2(0) NOT NULL CONSTRAINT DF_CustomerDocument_Created DEFAULT SYSDATETIME(),
    CONSTRAINT FK_CustomerDocument_Customer FOREIGN KEY (CustomerID) REFERENCES dbo.Customer(CustomerID),
    CONSTRAINT FK_CustomerDocument_Type FOREIGN KEY (DocumentTypeID) REFERENCES dbo.DocumentType(DocumentTypeID)
);
GO

-- Keep complete original source payload for audit, parser improvements and diagnostics.
-- Import batches should be processed transactionally by the application.
CREATE TABLE dbo.ImportBatch (
    ImportBatchID BIGINT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    OTAID INT NOT NULL,
    SourceFileName NVARCHAR(255) NULL,
    ImportedAt DATETIME2(0) NOT NULL CONSTRAINT DF_ImportBatch_Imported DEFAULT SYSDATETIME(),
    CONSTRAINT FK_ImportBatch_OTA FOREIGN KEY (OTAID) REFERENCES dbo.OTA(OTAID)
);
GO
CREATE TABLE dbo.ImportRow (
    ImportRowID BIGINT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    ImportBatchID BIGINT NOT NULL,
    SourceRowNumber INT NOT NULL,
    ReservationID BIGINT NULL,
    RawDetails NVARCHAR(MAX) NULL,
    RawPayload NVARCHAR(MAX) NOT NULL,
    ParseStatus NVARCHAR(20) NOT NULL CONSTRAINT DF_ImportRow_Status DEFAULT 'Pending',
    ParseError NVARCHAR(2000) NULL,
    CONSTRAINT FK_ImportRow_Batch FOREIGN KEY (ImportBatchID) REFERENCES dbo.ImportBatch(ImportBatchID),
    CONSTRAINT FK_ImportRow_Reservation FOREIGN KEY (ReservationID) REFERENCES dbo.Reservation(ReservationID),
    CONSTRAINT UQ_ImportRow_BatchNumber UNIQUE (ImportBatchID,SourceRowNumber),
    CONSTRAINT CK_ImportRow_Status CHECK (ParseStatus IN ('Pending','Processed','Failed','Skipped'))
);
GO

-- Financial view: charges/payments aggregated independently to prevent multiplication
-- when a reservation has N charges and M payments. Amounts are per currency.
-- OTAStatementItem deliberately excluded (ambiguous deposit is informational).
CREATE VIEW dbo.vwReservationFinancialSummary AS
WITH C AS (
    SELECT ReservationID, CurrencyCode, SUM(Amount) AS TotalCharged
    FROM dbo.ReservationCharge GROUP BY ReservationID,CurrencyCode
), P AS (
    SELECT ReservationID, CurrencyCode,
           SUM(CASE WHEN Direction='Inflow' THEN Amount ELSE -Amount END) AS NetCashMovement
    FROM dbo.ReservationPayment GROUP BY ReservationID,CurrencyCode
), Keys AS (
    SELECT ReservationID,CurrencyCode FROM C
    UNION
    SELECT ReservationID,CurrencyCode FROM P
)
SELECT R.ReservationID, K.CurrencyCode,
       ISNULL(C.TotalCharged,0) AS TotalCharged,
       ISNULL(P.NetCashMovement,0) AS NetCashMovement,
       ISNULL(C.TotalCharged,0)-ISNULL(P.NetCashMovement,0) AS UnreconciledDifference
FROM dbo.Reservation R
JOIN Keys K ON K.ReservationID=R.ReservationID
LEFT JOIN C ON C.ReservationID=K.ReservationID AND C.CurrencyCode=K.CurrencyCode
LEFT JOIN P ON P.ReservationID=K.ReservationID AND P.CurrencyCode=K.CurrencyCode;
GO
/* EXAMPLE (illustration, not executable inserts):
  Nightly source lines => 2026-10-20, 10-21, 10-22 each Accommodation
  Qty=1, UnitPrice=5700, CurrencyCode='JPY', Description='breakfast non-refundable'.
  Charge total 17,100 JPY. NOT 17,100 per guest.
  deposit JPY 2052 => OTAStatementItem RawLabel='deposit', Amount=2052,
  Classification='Unclassified'. No 2052 debit, credit or revenue inference.
  A towel rental => one ReservationCharge row ChargeType='Towel', ExtraServiceID=...
  A cash receipt => one ReservationPayment row PaymentKind='Payment', Direction='Inflow'.

 IMPLEMENTATION RULES:
 1) For detailed bookings, RoomRateInfo and BookingDetails are raw descriptive fields;
    DO NOT add them again to totals when nightly charges have been parsed.
 2) At most one lead guest per reservation is enforced; adding a lead guest is optional.
 3) Prevent conflicting room/bed occupancy using a transactional application check or
    a stored procedure with proper locking: SQL Server has no native exclusion constraint.
 4) Ensure reservation accommodation ranges fit the booking range in application logic.
 5) PaymentAllocation: application must validate same reservation/currency, permissible
    payment direction and allocations <= charge/payment, under appropriate locking.
 6) Never overwrite previously agreed financial records on CSV re-import without
    reconciliation. ExternalReservationNumber + OTA uniquely identifies a reservation;
    SourceLineKey is recommended to identify imported daily rates and OTA statement rows.
 7) This is a new-build baseline, not an ALTER/migration script.
 8) Update UpdatedAt in application logic (SQL Server DEFAULT only sets on INSERT).
 9) Store guest IDs and documents securely, with restricted access and retention policies.
 10) For OTA commission, once validated, account for the fee separately; the summary view
     does not attempt to equate net payout to gross sales automatically.
*/
