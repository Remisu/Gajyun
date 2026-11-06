IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261106083537_InitialSchemaV13'
)
BEGIN
    CREATE TABLE [dbo].[BookingStatus] (
        [BookingStatusID] int NOT NULL IDENTITY,
        [StatusName] nvarchar(60) NOT NULL,
        [IsActive] bit NOT NULL CONSTRAINT [DF_BookingStatus_Active] DEFAULT CAST(1 AS bit),
        CONSTRAINT [PK_BookingStatus] PRIMARY KEY ([BookingStatusID])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261106083537_InitialSchemaV13'
)
BEGIN
    CREATE TABLE [dbo].[Country] (
        [CountryID] int NOT NULL IDENTITY,
        [CountryName] nvarchar(120) NOT NULL,
        [ISO2Code] char(2) NULL,
        [FlagUrl] nvarchar(500) NULL,
        CONSTRAINT [PK_Country] PRIMARY KEY ([CountryID])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261106083537_InitialSchemaV13'
)
BEGIN
    CREATE TABLE [dbo].[DocumentType] (
        [DocumentTypeID] int NOT NULL IDENTITY,
        [DocumentTypeName] nvarchar(100) NOT NULL,
        [IsActive] bit NOT NULL CONSTRAINT [DF_DocumentType_Active] DEFAULT CAST(1 AS bit),
        CONSTRAINT [PK_DocumentType] PRIMARY KEY ([DocumentTypeID])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261106083537_InitialSchemaV13'
)
BEGIN
    CREATE TABLE [dbo].[ExtraService] (
        [ExtraServiceID] int NOT NULL IDENTITY,
        [ServiceName] nvarchar(100) NOT NULL,
        [DefaultUnitPrice] decimal(12,2) NOT NULL CONSTRAINT [DF_ExtraService_Price] DEFAULT 0.0,
        [CurrencyCode] char(3) NOT NULL CONSTRAINT [DF_ExtraService_Currency] DEFAULT 'JPY',
        [IsActive] bit NOT NULL CONSTRAINT [DF_ExtraService_Active] DEFAULT CAST(1 AS bit),
        CONSTRAINT [PK_ExtraService] PRIMARY KEY ([ExtraServiceID]),
        CONSTRAINT [CK_ExtraService_Price] CHECK ([DefaultUnitPrice] >= 0)
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261106083537_InitialSchemaV13'
)
BEGIN
    CREATE TABLE [dbo].[OTA] (
        [OTAID] int NOT NULL IDENTITY,
        [OTAName] nvarchar(120) NOT NULL,
        [LogoUrl] nvarchar(500) NULL,
        [IsActive] bit NOT NULL CONSTRAINT [DF_OTA_Active] DEFAULT CAST(1 AS bit),
        CONSTRAINT [PK_OTA] PRIMARY KEY ([OTAID])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261106083537_InitialSchemaV13'
)
BEGIN
    CREATE TABLE [dbo].[PaymentMethod] (
        [PaymentMethodID] int NOT NULL IDENTITY,
        [MethodName] nvarchar(80) NOT NULL,
        [IsActive] bit NOT NULL CONSTRAINT [DF_PaymentMethod_Active] DEFAULT CAST(1 AS bit),
        CONSTRAINT [PK_PaymentMethod] PRIMARY KEY ([PaymentMethodID])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261106083537_InitialSchemaV13'
)
BEGIN
    CREATE TABLE [dbo].[ReservationGroup] (
        [GroupID] bigint NOT NULL IDENTITY,
        [ExternalGroupReference] nvarchar(120) NULL,
        [LogoUrl] nvarchar(500) NULL,
        [Notes] nvarchar(1000) NULL,
        CONSTRAINT [PK_ReservationGroup] PRIMARY KEY ([GroupID])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261106083537_InitialSchemaV13'
)
BEGIN
    CREATE TABLE [dbo].[TaxPolicy] (
        [TaxPolicyID] int NOT NULL IDENTITY,
        [PolicyName] nvarchar(160) NOT NULL,
        [ValidFrom] date NOT NULL,
        [ValidTo] date NULL,
        [IsActive] bit NOT NULL CONSTRAINT [DF_TaxPolicy_Active] DEFAULT CAST(1 AS bit),
        [Notes] nvarchar(1000) NULL,
        CONSTRAINT [PK_TaxPolicy] PRIMARY KEY ([TaxPolicyID]),
        CONSTRAINT [CK_TaxPolicy_Dates] CHECK ([ValidTo] IS NULL OR [ValidTo] > [ValidFrom])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261106083537_InitialSchemaV13'
)
BEGIN
    CREATE TABLE [dbo].[Customer] (
        [CustomerID] bigint NOT NULL IDENTITY,
        [FullName] nvarchar(255) NOT NULL,
        [Nickname] nvarchar(255) NULL,
        [CountryID] int NULL,
        [PostalAddress] nvarchar(500) NULL,
        [PhoneNumber] nvarchar(50) NULL,
        [Email] nvarchar(254) NULL,
        [IsRegularCustomer] bit NOT NULL CONSTRAINT [DF_Customer_Regular] DEFAULT CAST(0 AS bit),
        [CreatedAt] datetime2(0) NOT NULL CONSTRAINT [DF_Customer_Created] DEFAULT (SYSDATETIME()),
        CONSTRAINT [PK_Customer] PRIMARY KEY ([CustomerID]),
        CONSTRAINT [FK_Customer_Country] FOREIGN KEY ([CountryID]) REFERENCES [dbo].[Country] ([CountryID])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261106083537_InitialSchemaV13'
)
BEGIN
    CREATE TABLE [dbo].[ImportBatch] (
        [ImportBatchID] bigint NOT NULL IDENTITY,
        [OTAID] int NOT NULL,
        [SourceFileName] nvarchar(255) NULL,
        [ImportedAt] datetime2(0) NOT NULL CONSTRAINT [DF_ImportBatch_Imported] DEFAULT (SYSDATETIME()),
        [SourceFileHash] varchar(64) NULL,
        [SourceEncoding] nvarchar(100) NULL,
        [SourceFileContent] varbinary(max) NULL,
        [ParserVersion] nvarchar(40) NULL,
        CONSTRAINT [PK_ImportBatch] PRIMARY KEY ([ImportBatchID]),
        CONSTRAINT [FK_ImportBatch_OTA] FOREIGN KEY ([OTAID]) REFERENCES [dbo].[OTA] ([OTAID])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261106083537_InitialSchemaV13'
)
BEGIN
    CREATE TABLE [dbo].[Reservation] (
        [ReservationID] bigint NOT NULL IDENTITY,
        [OTAID] int NOT NULL,
        [ExternalReservationNumber] nvarchar(120) NULL,
        [GroupID] bigint NULL,
        [BookingStatusID] int NOT NULL,
        [SourcePaymentMethodID] int NULL,
        [BookedAt] datetime2(0) NULL,
        [ModifiedAtSource] datetime2(0) NULL,
        [CheckInDate] date NOT NULL,
        [CheckOutDate] date NOT NULL,
        [ExpectedGuestCount] smallint NULL,
        [ActualCheckInAt] datetime2(0) NULL,
        [CheckInStatus] nvarchar(20) NOT NULL CONSTRAINT [DF_Reservation_CheckInStatus] DEFAULT N'Pending',
        [IsActive] bit NOT NULL CONSTRAINT [DF_Reservation_Active] DEFAULT CAST(1 AS bit),
        [IsModified] bit NOT NULL CONSTRAINT [DF_Reservation_Modified] DEFAULT CAST(0 AS bit),
        [ConsecutiveStayInfo] nvarchar(max) NULL,
        [RoomRateInfo] nvarchar(max) NULL,
        [BookingDetails] nvarchar(max) NULL,
        [CreatedAt] datetime2(0) NOT NULL CONSTRAINT [DF_Reservation_Created] DEFAULT (SYSDATETIME()),
        [UpdatedAt] datetime2(0) NOT NULL CONSTRAINT [DF_Reservation_Updated] DEFAULT (SYSDATETIME()),
        CONSTRAINT [PK_Reservation] PRIMARY KEY ([ReservationID]),
        CONSTRAINT [CK_Reservation_CheckInStatus] CHECK ([CheckInStatus] IN ('Pending','CheckedIn','NoShow')),
        CONSTRAINT [CK_Reservation_CheckInTime] CHECK (([CheckInStatus] = 'CheckedIn' AND [ActualCheckInAt] IS NOT NULL) OR ([CheckInStatus] <> 'CheckedIn' AND [ActualCheckInAt] IS NULL)),
        CONSTRAINT [CK_Reservation_Dates] CHECK ([CheckOutDate] > [CheckInDate]),
        CONSTRAINT [CK_Reservation_Guests] CHECK ([ExpectedGuestCount] IS NULL OR [ExpectedGuestCount] > 0),
        CONSTRAINT [FK_Reservation_BookingStatus] FOREIGN KEY ([BookingStatusID]) REFERENCES [dbo].[BookingStatus] ([BookingStatusID]),
        CONSTRAINT [FK_Reservation_Group] FOREIGN KEY ([GroupID]) REFERENCES [dbo].[ReservationGroup] ([GroupID]),
        CONSTRAINT [FK_Reservation_OTA] FOREIGN KEY ([OTAID]) REFERENCES [dbo].[OTA] ([OTAID]),
        CONSTRAINT [FK_Reservation_SourcePaymentMethod] FOREIGN KEY ([SourcePaymentMethodID]) REFERENCES [dbo].[PaymentMethod] ([PaymentMethodID])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261106083537_InitialSchemaV13'
)
BEGIN
    CREATE TABLE [dbo].[Room] (
        [RoomID] int NOT NULL IDENTITY,
        [RoomName] nvarchar(100) NOT NULL,
        [Capacity] smallint NOT NULL,
        [DefaultTaxPolicyID] int NULL,
        [IsActive] bit NOT NULL CONSTRAINT [DF_Room_Active] DEFAULT CAST(1 AS bit),
        CONSTRAINT [PK_Room] PRIMARY KEY ([RoomID]),
        CONSTRAINT [CK_Room_Capacity] CHECK ([Capacity] > 0),
        CONSTRAINT [FK_Room_DefaultTaxPolicy] FOREIGN KEY ([DefaultTaxPolicyID]) REFERENCES [dbo].[TaxPolicy] ([TaxPolicyID])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261106083537_InitialSchemaV13'
)
BEGIN
    CREATE TABLE [dbo].[TaxBracket] (
        [TaxBracketID] int NOT NULL IDENTITY,
        [TaxPolicyID] int NOT NULL,
        [MinPriceInclusive] decimal(12,2) NOT NULL,
        [MaxPriceExclusive] decimal(12,2) NULL,
        [TaxPerPersonNight] decimal(12,2) NOT NULL,
        CONSTRAINT [PK_TaxBracket] PRIMARY KEY ([TaxBracketID]),
        CONSTRAINT [CK_TaxBracket_Amount] CHECK ([TaxPerPersonNight] >= 0),
        CONSTRAINT [CK_TaxBracket_Range] CHECK ([MinPriceInclusive] >= 0 AND ([MaxPriceExclusive] IS NULL OR [MaxPriceExclusive] > [MinPriceInclusive])),
        CONSTRAINT [FK_TaxBracket_TaxPolicy] FOREIGN KEY ([TaxPolicyID]) REFERENCES [dbo].[TaxPolicy] ([TaxPolicyID])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261106083537_InitialSchemaV13'
)
BEGIN
    CREATE TABLE [dbo].[CustomerDocument] (
        [DocumentID] bigint NOT NULL IDENTITY,
        [CustomerID] bigint NOT NULL,
        [DocumentTypeID] int NOT NULL,
        [FileStorageKey] nvarchar(1000) NULL,
        [PhotoStorageKey] nvarchar(1000) NULL,
        [ExpiryDate] date NULL,
        [CreatedAt] datetime2(0) NOT NULL CONSTRAINT [DF_CustomerDocument_Created] DEFAULT (SYSDATETIME()),
        CONSTRAINT [PK_CustomerDocument] PRIMARY KEY ([DocumentID]),
        CONSTRAINT [FK_CustomerDocument_Customer] FOREIGN KEY ([CustomerID]) REFERENCES [dbo].[Customer] ([CustomerID]),
        CONSTRAINT [FK_CustomerDocument_Type] FOREIGN KEY ([DocumentTypeID]) REFERENCES [dbo].[DocumentType] ([DocumentTypeID])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261106083537_InitialSchemaV13'
)
BEGIN
    CREATE TABLE [dbo].[Stay] (
        [StayID] bigint NOT NULL IDENTITY,
        [CustomerID] bigint NOT NULL,
        [StartDate] date NOT NULL,
        [EndDate] date NOT NULL,
        [Notes] nvarchar(1000) NULL,
        [CreatedAt] datetime2(0) NOT NULL CONSTRAINT [DF_Stay_Created] DEFAULT (SYSDATETIME()),
        CONSTRAINT [PK_Stay] PRIMARY KEY ([StayID]),
        CONSTRAINT [CK_Stay_Dates] CHECK ([EndDate] > [StartDate]),
        CONSTRAINT [FK_Stay_Customer] FOREIGN KEY ([CustomerID]) REFERENCES [dbo].[Customer] ([CustomerID])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261106083537_InitialSchemaV13'
)
BEGIN
    CREATE TABLE [dbo].[ImportRow] (
        [ImportRowID] bigint NOT NULL IDENTITY,
        [ImportBatchID] bigint NOT NULL,
        [SourceRowNumber] int NOT NULL,
        [ReservationID] bigint NULL,
        [RawDetails] nvarchar(max) NULL,
        [RawPayload] nvarchar(max) NOT NULL,
        [ParseStatus] nvarchar(20) NOT NULL CONSTRAINT [DF_ImportRow_Status] DEFAULT N'Pending',
        [ParseError] nvarchar(2000) NULL,
        [SourceRowHash] varchar(64) NULL,
        CONSTRAINT [PK_ImportRow] PRIMARY KEY ([ImportRowID]),
        CONSTRAINT [UQ_ImportRow_BatchNumber] UNIQUE ([ImportBatchID], [SourceRowNumber]),
        CONSTRAINT [CK_ImportRow_Status] CHECK ([ParseStatus] IN ('Pending','Processed','Failed','Skipped')),
        CONSTRAINT [FK_ImportRow_Batch] FOREIGN KEY ([ImportBatchID]) REFERENCES [dbo].[ImportBatch] ([ImportBatchID]),
        CONSTRAINT [FK_ImportRow_Reservation] FOREIGN KEY ([ReservationID]) REFERENCES [dbo].[Reservation] ([ReservationID])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261106083537_InitialSchemaV13'
)
BEGIN
    CREATE TABLE [dbo].[OTAStatementItem] (
        [OTAStatementItemID] bigint NOT NULL IDENTITY,
        [ReservationID] bigint NOT NULL,
        [RawLabel] nvarchar(100) NOT NULL,
        [RawText] nvarchar(500) NULL,
        [Amount] decimal(12,2) NOT NULL,
        [CurrencyCode] char(3) NOT NULL CONSTRAINT [DF_OTAStatementItem_Currency] DEFAULT 'JPY',
        [Classification] nvarchar(25) NOT NULL CONSTRAINT [DF_OTAStatementItem_Class] DEFAULT N'Unclassified',
        [ResolutionNotes] nvarchar(500) NULL,
        [ImportedAt] datetime2(0) NOT NULL CONSTRAINT [DF_OTAStatementItem_Imported] DEFAULT (SYSDATETIME()),
        [SourceLineKey] nvarchar(150) NULL,
        CONSTRAINT [PK_OTAStatementItem] PRIMARY KEY ([OTAStatementItemID]),
        CONSTRAINT [CK_OTAStatementItem_Amount] CHECK ([Amount] >= 0),
        CONSTRAINT [CK_OTAStatementItem_Classification] CHECK ([Classification] IN ('Unclassified','Commission','GuestDeposit','Payout','TaxWithheld','Other')),
        CONSTRAINT [FK_OTAStatementItem_Reservation] FOREIGN KEY ([ReservationID]) REFERENCES [dbo].[Reservation] ([ReservationID])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261106083537_InitialSchemaV13'
)
BEGIN
    CREATE TABLE [dbo].[ReservationCharge] (
        [ChargeID] bigint NOT NULL IDENTITY,
        [ReservationID] bigint NOT NULL,
        [ExtraServiceID] int NULL,
        [ChargeType] nvarchar(30) NOT NULL,
        [ServiceDate] date NULL,
        [Description] nvarchar(500) NULL,
        [Quantity] decimal(10,2) NOT NULL CONSTRAINT [DF_ReservationCharge_Quantity] DEFAULT 1.0,
        [UnitPrice] decimal(12,2) NOT NULL,
        [Amount] AS CONVERT(DECIMAL(18,2), [Quantity] * [UnitPrice]) PERSISTED,
        [CurrencyCode] char(3) NOT NULL CONSTRAINT [DF_ReservationCharge_Currency] DEFAULT 'JPY',
        [SourceLineKey] nvarchar(150) NULL,
        [CreatedAt] datetime2(0) NOT NULL CONSTRAINT [DF_ReservationCharge_Created] DEFAULT (SYSDATETIME()),
        CONSTRAINT [PK_ReservationCharge] PRIMARY KEY ([ChargeID]),
        CONSTRAINT [CK_ReservationCharge_Quantity] CHECK ([Quantity] > 0),
        CONSTRAINT [CK_ReservationCharge_SignedPrice] CHECK ([ChargeType] IN ('Discount','Adjustment') OR [UnitPrice] >= 0),
        CONSTRAINT [CK_ReservationCharge_Type] CHECK ([ChargeType] IN ('Accommodation','Breakfast','Laundry','Towel','LateCheckIn','Tax','Discount','Adjustment','Other')),
        CONSTRAINT [FK_ReservationCharge_Extra] FOREIGN KEY ([ExtraServiceID]) REFERENCES [dbo].[ExtraService] ([ExtraServiceID]),
        CONSTRAINT [FK_ReservationCharge_Reservation] FOREIGN KEY ([ReservationID]) REFERENCES [dbo].[Reservation] ([ReservationID])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261106083537_InitialSchemaV13'
)
BEGIN
    CREATE TABLE [dbo].[ReservationCustomer] (
        [ReservationID] bigint NOT NULL,
        [CustomerID] bigint NOT NULL,
        [IsLeadGuest] bit NOT NULL CONSTRAINT [DF_ReservationCustomer_Lead] DEFAULT CAST(0 AS bit),
        [AddedAt] datetime2(0) NOT NULL CONSTRAINT [DF_ReservationCustomer_Added] DEFAULT (SYSDATETIME()),
        CONSTRAINT [PK_ReservationCustomer] PRIMARY KEY ([ReservationID], [CustomerID]),
        CONSTRAINT [FK_ReservationCustomer_Customer] FOREIGN KEY ([CustomerID]) REFERENCES [dbo].[Customer] ([CustomerID]),
        CONSTRAINT [FK_ReservationCustomer_Reservation] FOREIGN KEY ([ReservationID]) REFERENCES [dbo].[Reservation] ([ReservationID])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261106083537_InitialSchemaV13'
)
BEGIN
    CREATE TABLE [dbo].[ReservationPayment] (
        [PaymentID] bigint NOT NULL IDENTITY,
        [ReservationID] bigint NOT NULL,
        [PaymentMethodID] int NULL,
        [Direction] nvarchar(10) NOT NULL,
        [CounterpartyType] nvarchar(20) NOT NULL,
        [PaymentKind] nvarchar(25) NOT NULL,
        [Amount] decimal(12,2) NOT NULL,
        [CurrencyCode] char(3) NOT NULL CONSTRAINT [DF_ReservationPayment_Currency] DEFAULT 'JPY',
        [PaidAt] datetime2(0) NULL,
        [ExternalReference] nvarchar(150) NULL,
        [Notes] nvarchar(500) NULL,
        CONSTRAINT [PK_ReservationPayment] PRIMARY KEY ([PaymentID]),
        CONSTRAINT [CK_ReservationPayment_Amount] CHECK ([Amount] > 0),
        CONSTRAINT [CK_ReservationPayment_Direction] CHECK ([Direction] IN ('Inflow','Outflow')),
        CONSTRAINT [CK_ReservationPayment_Kind] CHECK ([PaymentKind] IN ('Payment','Refund','Payout','Other')),
        CONSTRAINT [CK_ReservationPayment_Party] CHECK ([CounterpartyType] IN ('Guest','OTA','Guesthouse','Other')),
        CONSTRAINT [FK_ReservationPayment_Method] FOREIGN KEY ([PaymentMethodID]) REFERENCES [dbo].[PaymentMethod] ([PaymentMethodID]),
        CONSTRAINT [FK_ReservationPayment_Reservation] FOREIGN KEY ([ReservationID]) REFERENCES [dbo].[Reservation] ([ReservationID])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261106083537_InitialSchemaV13'
)
BEGIN
    CREATE TABLE [dbo].[Bed] (
        [BedID] int NOT NULL IDENTITY,
        [RoomID] int NOT NULL,
        [BedLabel] nvarchar(80) NOT NULL,
        [IsActive] bit NOT NULL CONSTRAINT [DF_Bed_Active] DEFAULT CAST(1 AS bit),
        CONSTRAINT [PK_Bed] PRIMARY KEY ([BedID]),
        CONSTRAINT [UQ_Bed_BedRoom] UNIQUE ([BedID], [RoomID]),
        CONSTRAINT [UQ_Bed_Room_Label] UNIQUE ([RoomID], [BedLabel]),
        CONSTRAINT [FK_Bed_Room] FOREIGN KEY ([RoomID]) REFERENCES [dbo].[Room] ([RoomID])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261106083537_InitialSchemaV13'
)
BEGIN
    CREATE TABLE [dbo].[OTARoomMapping] (
        [OTARoomMappingID] int NOT NULL IDENTITY,
        [OTAID] int NULL,
        [RoomID] int NOT NULL,
        [ExternalRoomName] nvarchar(255) NOT NULL,
        [NormalizedName] nvarchar(255) COLLATE Latin1_General_100_BIN2 NOT NULL,
        [IsActive] bit NOT NULL CONSTRAINT [DF_OTARoomMapping_Active] DEFAULT CAST(1 AS bit),
        [CreatedAt] datetime2(0) NOT NULL CONSTRAINT [DF_OTARoomMapping_Created] DEFAULT (SYSDATETIME()),
        CONSTRAINT [PK_OTARoomMapping] PRIMARY KEY ([OTARoomMappingID]),
        CONSTRAINT [FK_OTARoomMapping_OTA] FOREIGN KEY ([OTAID]) REFERENCES [dbo].[OTA] ([OTAID]),
        CONSTRAINT [FK_OTARoomMapping_Room] FOREIGN KEY ([RoomID]) REFERENCES [dbo].[Room] ([RoomID])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261106083537_InitialSchemaV13'
)
BEGIN
    CREATE TABLE [dbo].[ReservationTax] (
        [ReservationTaxID] bigint NOT NULL IDENTITY,
        [ReservationID] bigint NOT NULL,
        [ServiceDate] date NOT NULL,
        [RoomID] int NULL,
        [LegalTaxPolicyID] int NULL,
        [AppliedTaxPolicyID] int NULL,
        [GuestCount] smallint NULL,
        [TaxablePricePerPerson] decimal(12,2) NULL,
        [LegalTaxAmount] decimal(12,2) NULL,
        [ChargedTaxAmount] decimal(12,2) NULL,
        [CalculationStatus] nvarchar(24) NOT NULL CONSTRAINT [DF_ReservationTax_Status] DEFAULT N'PendingReview',
        [Notes] nvarchar(1000) NULL,
        [CalculatedAt] datetime2(0) NULL,
        CONSTRAINT [PK_ReservationTax] PRIMARY KEY ([ReservationTaxID]),
        CONSTRAINT [CK_ReservationTax_Amounts] CHECK (([TaxablePricePerPerson] IS NULL OR [TaxablePricePerPerson] >= 0) AND ([LegalTaxAmount] IS NULL OR [LegalTaxAmount] >= 0) AND ([ChargedTaxAmount] IS NULL OR [ChargedTaxAmount] >= 0)),
        CONSTRAINT [CK_ReservationTax_Guests] CHECK ([GuestCount] IS NULL OR [GuestCount] > 0),
        CONSTRAINT [CK_ReservationTax_Status] CHECK ([CalculationStatus] IN ('PendingReview','Calculated','Confirmed','NotApplicable')),
        CONSTRAINT [FK_ReservationTax_AppliedPolicy] FOREIGN KEY ([AppliedTaxPolicyID]) REFERENCES [dbo].[TaxPolicy] ([TaxPolicyID]),
        CONSTRAINT [FK_ReservationTax_LegalPolicy] FOREIGN KEY ([LegalTaxPolicyID]) REFERENCES [dbo].[TaxPolicy] ([TaxPolicyID]),
        CONSTRAINT [FK_ReservationTax_Reservation] FOREIGN KEY ([ReservationID]) REFERENCES [dbo].[Reservation] ([ReservationID]),
        CONSTRAINT [FK_ReservationTax_Room] FOREIGN KEY ([RoomID]) REFERENCES [dbo].[Room] ([RoomID])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261106083537_InitialSchemaV13'
)
BEGIN
    CREATE TABLE [dbo].[StayMemo] (
        [MemoID] bigint NOT NULL IDENTITY,
        [StayID] bigint NOT NULL,
        [MemoText] nvarchar(max) NOT NULL,
        [CreatedAt] datetime2(0) NOT NULL CONSTRAINT [DF_StayMemo_Created] DEFAULT (SYSDATETIME()),
        CONSTRAINT [PK_StayMemo] PRIMARY KEY ([MemoID]),
        CONSTRAINT [FK_StayMemo_Stay] FOREIGN KEY ([StayID]) REFERENCES [dbo].[Stay] ([StayID])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261106083537_InitialSchemaV13'
)
BEGIN
    CREATE TABLE [dbo].[StayReservation] (
        [StayID] bigint NOT NULL,
        [ReservationID] bigint NOT NULL,
        [AddedAt] datetime2(0) NOT NULL CONSTRAINT [DF_StayReservation_Added] DEFAULT (SYSDATETIME()),
        CONSTRAINT [PK_StayReservation] PRIMARY KEY ([StayID], [ReservationID]),
        CONSTRAINT [FK_StayReservation_Reservation] FOREIGN KEY ([ReservationID]) REFERENCES [dbo].[Reservation] ([ReservationID]),
        CONSTRAINT [FK_StayReservation_Stay] FOREIGN KEY ([StayID]) REFERENCES [dbo].[Stay] ([StayID])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261106083537_InitialSchemaV13'
)
BEGIN
    CREATE TABLE [dbo].[PaymentAllocation] (
        [PaymentID] bigint NOT NULL,
        [ChargeID] bigint NOT NULL,
        [AllocatedAmount] decimal(12,2) NOT NULL,
        CONSTRAINT [PK_PaymentAllocation] PRIMARY KEY ([PaymentID], [ChargeID]),
        CONSTRAINT [CK_PaymentAllocation_Positive] CHECK ([AllocatedAmount] > 0),
        CONSTRAINT [FK_PaymentAllocation_Charge] FOREIGN KEY ([ChargeID]) REFERENCES [dbo].[ReservationCharge] ([ChargeID]),
        CONSTRAINT [FK_PaymentAllocation_Payment] FOREIGN KEY ([PaymentID]) REFERENCES [dbo].[ReservationPayment] ([PaymentID])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261106083537_InitialSchemaV13'
)
BEGIN
    CREATE TABLE [dbo].[ReservationAccommodation] (
        [ReservationAccommodationID] bigint NOT NULL IDENTITY,
        [ReservationID] bigint NOT NULL,
        [RoomID] int NOT NULL,
        [BedID] int NULL,
        [StartDate] date NOT NULL,
        [EndDate] date NOT NULL,
        [Notes] nvarchar(300) NULL,
        CONSTRAINT [PK_ReservationAccommodation] PRIMARY KEY ([ReservationAccommodationID]),
        CONSTRAINT [CK_ReservationAccommodation_Dates] CHECK ([EndDate] > [StartDate]),
        CONSTRAINT [FK_ReservationAccommodation_BedRoom] FOREIGN KEY ([BedID], [RoomID]) REFERENCES [dbo].[Bed] ([BedID], [RoomID]),
        CONSTRAINT [FK_ReservationAccommodation_Reservation] FOREIGN KEY ([ReservationID]) REFERENCES [dbo].[Reservation] ([ReservationID]),
        CONSTRAINT [FK_ReservationAccommodation_Room] FOREIGN KEY ([RoomID]) REFERENCES [dbo].[Room] ([RoomID])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261106083537_InitialSchemaV13'
)
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'CountryID', N'CountryName', N'FlagUrl', N'ISO2Code') AND [object_id] = OBJECT_ID(N'[dbo].[Country]'))
        SET IDENTITY_INSERT [dbo].[Country] ON;
    EXEC(N'INSERT INTO [dbo].[Country] ([CountryID], [CountryName], [FlagUrl], [ISO2Code])
    VALUES (1, N''Andorra'', N''pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/ad.png'', ''AD''),
    (2, N''United Arab Emirates'', N''pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/ae.png'', ''AE''),
    (3, N''Afghanistan'', N''pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/af.png'', ''AF''),
    (4, N''Antigua & Barbuda'', N''pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/ag.png'', ''AG''),
    (5, N''Anguilla'', N''pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/ai.png'', ''AI''),
    (6, N''Albania'', N''pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/al.png'', ''AL''),
    (7, N''Armenia'', N''pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/am.png'', ''AM''),
    (8, N''Angola'', N''pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/ao.png'', ''AO''),
    (9, N''Antarctica'', N''pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/aq.png'', ''AQ''),
    (10, N''Argentina'', N''pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/ar.png'', ''AR''),
    (11, N''American Samoa'', N''pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/as.png'', ''AS''),
    (12, N''Austria'', N''pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/at.png'', ''AT''),
    (13, N''Australia'', N''pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/au.png'', ''AU''),
    (14, N''Aruba'', N''pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/aw.png'', ''AW''),
    (15, N''Åland Islands'', N''pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/ax.png'', ''AX''),
    (16, N''Azerbaijan'', N''pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/az.png'', ''AZ''),
    (17, N''Bosnia & Herzegovina'', N''pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/ba.png'', ''BA''),
    (18, N''Barbados'', N''pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/bb.png'', ''BB''),
    (19, N''Bangladesh'', N''pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/bd.png'', ''BD''),
    (20, N''Belgium'', N''pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/be.png'', ''BE''),
    (21, N''Burkina Faso'', N''pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/bf.png'', ''BF''),
    (22, N''Bulgaria'', N''pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/bg.png'', ''BG''),
    (23, N''Bahrain'', N''pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/bh.png'', ''BH''),
    (24, N''Burundi'', N''pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/bi.png'', ''BI''),
    (25, N''Benin'', N''pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/bj.png'', ''BJ''),
    (26, N''St. Barthélemy'', N''pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/bl.png'', ''BL''),
    (27, N''Bermuda'', N''pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/bm.png'', ''BM''),
    (28, N''Brunei'', N''pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/bn.png'', ''BN''),
    (29, N''Bolivia'', N''pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/bo.png'', ''BO''),
    (30, N''Bonaire, Sint Eustatius and Saba'', N''pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/bq.png'', ''BQ''),
    (31, N''Brazil'', N''pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/br.png'', ''BR''),
    (32, N''Bahamas'', N''pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/bs.png'', ''BS''),
    (33, N''Bhutan'', N''pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/bt.png'', ''BT''),
    (34, N''Bouvet Island'', N''pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/bv.png'', ''BV''),
    (35, N''Botswana'', N''pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/bw.png'', ''BW''),
    (36, N''Belarus'', N''pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/by.png'', ''BY''),
    (37, N''Belize'', N''pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/bz.png'', ''BZ''),
    (38, N''Canada'', N''pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/ca.png'', ''CA''),
    (39, N''Cocos (Keeling) Islands'', N''pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/cc.png'', ''CC''),
    (40, N''Congo (DRC)'', N''pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/cd.png'', ''CD''),
    (41, N''Central African Republic'', N''pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/cf.png'', ''CF''),
    (42, N''Congo'', N''pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/cg.png'', ''CG'');
    INSERT INTO [dbo].[Country] ([CountryID], [CountryName], [FlagUrl], [ISO2Code])
    VALUES (43, N''Switzerland'', N''pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/ch.png'', ''CH''),
    (44, N''Côte d’Ivoire'', N''pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/ci.png'', ''CI''),
    (45, N''Cook Islands'', N''pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/ck.png'', ''CK''),
    (46, N''Chile'', N''pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/cl.png'', ''CL''),
    (47, N''Cameroon'', N''pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/cm.png'', ''CM''),
    (48, N''China'', N''pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/cn.png'', ''CN''),
    (49, N''Colombia'', N''pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/co.png'', ''CO''),
    (50, N''Costa Rica'', N''pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/cr.png'', ''CR''),
    (51, N''Cuba'', N''pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/cu.png'', ''CU''),
    (52, N''Cabo Verde'', N''pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/cv.png'', ''CV''),
    (53, N''Curaçao'', N''pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/cw.png'', ''CW''),
    (54, N''Christmas Island'', N''pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/cx.png'', ''CX''),
    (55, N''Cyprus'', N''pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/cy.png'', ''CY''),
    (56, N''Czechia'', N''pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/cz.png'', ''CZ''),
    (57, N''Germany'', N''pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/de.png'', ''DE''),
    (58, N''Djibouti'', N''pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/dj.png'', ''DJ''),
    (59, N''Denmark'', N''pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/dk.png'', ''DK''),
    (60, N''Dominica'', N''pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/dm.png'', ''DM''),
    (61, N''Dominican Republic'', N''pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/do.png'', ''DO''),
    (62, N''Algeria'', N''pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/dz.png'', ''DZ''),
    (63, N''Ecuador'', N''pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/ec.png'', ''EC''),
    (64, N''Estonia'', N''pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/ee.png'', ''EE''),
    (65, N''Egypt'', N''pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/eg.png'', ''EG''),
    (66, N''Western Sahara'', N''pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/eh.png'', ''EH''),
    (67, N''Eritrea'', N''pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/er.png'', ''ER''),
    (68, N''Spain'', N''pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/es.png'', ''ES''),
    (69, N''Ethiopia'', N''pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/et.png'', ''ET''),
    (70, N''Finland'', N''pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/fi.png'', ''FI''),
    (71, N''Fiji'', N''pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/fj.png'', ''FJ''),
    (72, N''Falkland Islands'', N''pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/fk.png'', ''FK''),
    (73, N''Micronesia'', N''pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/fm.png'', ''FM''),
    (74, N''Faroe Islands'', N''pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/fo.png'', ''FO''),
    (75, N''France'', N''pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/fr.png'', ''FR''),
    (76, N''Gabon'', N''pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/ga.png'', ''GA''),
    (77, N''United Kingdom'', N''pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/gb.png'', ''GB''),
    (78, N''Grenada'', N''pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/gd.png'', ''GD''),
    (79, N''Georgia'', N''pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/ge.png'', ''GE''),
    (80, N''French Guiana'', N''pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/gf.png'', ''GF''),
    (81, N''Guernsey'', N''pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/gg.png'', ''GG''),
    (82, N''Ghana'', N''pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/gh.png'', ''GH''),
    (83, N''Gibraltar'', N''pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/gi.png'', ''GI''),
    (84, N''Greenland'', N''pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/gl.png'', ''GL'');
    INSERT INTO [dbo].[Country] ([CountryID], [CountryName], [FlagUrl], [ISO2Code])
    VALUES (85, N''Gambia'', N''pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/gm.png'', ''GM''),
    (86, N''Guinea'', N''pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/gn.png'', ''GN''),
    (87, N''Guadeloupe'', N''pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/gp.png'', ''GP''),
    (88, N''Equatorial Guinea'', N''pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/gq.png'', ''GQ''),
    (89, N''Greece'', N''pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/gr.png'', ''GR''),
    (90, N''South Georgia and the South Sandwich Islands'', N''pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/gs.png'', ''GS''),
    (91, N''Guatemala'', N''pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/gt.png'', ''GT''),
    (92, N''Guam'', N''pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/gu.png'', ''GU''),
    (93, N''Guinea-Bissau'', N''pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/gw.png'', ''GW''),
    (94, N''Guyana'', N''pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/gy.png'', ''GY''),
    (95, N''Hong Kong SAR'', N''pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/hk.png'', ''HK''),
    (96, N''Heard Island and McDonald Islands'', N''pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/hm.png'', ''HM''),
    (97, N''Honduras'', N''pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/hn.png'', ''HN''),
    (98, N''Croatia'', N''pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/hr.png'', ''HR''),
    (99, N''Haiti'', N''pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/ht.png'', ''HT''),
    (100, N''Hungary'', N''pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/hu.png'', ''HU''),
    (101, N''Indonesia'', N''pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/id.png'', ''ID''),
    (102, N''Ireland'', N''pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/ie.png'', ''IE''),
    (103, N''Israel'', N''pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/il.png'', ''IL''),
    (104, N''Isle of Man'', N''pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/im.png'', ''IM''),
    (105, N''India'', N''pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/in.png'', ''IN''),
    (106, N''British Indian Ocean Territory'', N''pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/io.png'', ''IO''),
    (107, N''Iraq'', N''pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/iq.png'', ''IQ''),
    (108, N''Iran'', N''pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/ir.png'', ''IR''),
    (109, N''Iceland'', N''pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/is.png'', ''IS''),
    (110, N''Italy'', N''pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/it.png'', ''IT''),
    (111, N''Jersey'', N''pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/je.png'', ''JE''),
    (112, N''Jamaica'', N''pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/jm.png'', ''JM''),
    (113, N''Jordan'', N''pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/jo.png'', ''JO''),
    (114, N''Japan'', N''pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/jp.png'', ''JP''),
    (115, N''Kenya'', N''pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/ke.png'', ''KE''),
    (116, N''Kyrgyzstan'', N''pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/kg.png'', ''KG''),
    (117, N''Cambodia'', N''pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/kh.png'', ''KH''),
    (118, N''Kiribati'', N''pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/ki.png'', ''KI''),
    (119, N''Comoros'', N''pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/km.png'', ''KM''),
    (120, N''St. Kitts & Nevis'', N''pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/kn.png'', ''KN''),
    (121, N''North Korea'', N''pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/kp.png'', ''KP''),
    (122, N''Korea'', N''pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/kr.png'', ''KR''),
    (123, N''Kuwait'', N''pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/kw.png'', ''KW''),
    (124, N''Cayman Islands'', N''pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/ky.png'', ''KY''),
    (125, N''Kazakhstan'', N''pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/kz.png'', ''KZ''),
    (126, N''Laos'', N''pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/la.png'', ''LA'');
    INSERT INTO [dbo].[Country] ([CountryID], [CountryName], [FlagUrl], [ISO2Code])
    VALUES (127, N''Lebanon'', N''pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/lb.png'', ''LB''),
    (128, N''St. Lucia'', N''pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/lc.png'', ''LC''),
    (129, N''Liechtenstein'', N''pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/li.png'', ''LI''),
    (130, N''Sri Lanka'', N''pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/lk.png'', ''LK''),
    (131, N''Liberia'', N''pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/lr.png'', ''LR''),
    (132, N''Lesotho'', N''pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/ls.png'', ''LS''),
    (133, N''Lithuania'', N''pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/lt.png'', ''LT''),
    (134, N''Luxembourg'', N''pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/lu.png'', ''LU''),
    (135, N''Latvia'', N''pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/lv.png'', ''LV''),
    (136, N''Libya'', N''pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/ly.png'', ''LY''),
    (137, N''Morocco'', N''pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/ma.png'', ''MA''),
    (138, N''Monaco'', N''pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/mc.png'', ''MC''),
    (139, N''Moldova'', N''pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/md.png'', ''MD''),
    (140, N''Montenegro'', N''pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/me.png'', ''ME''),
    (141, N''St. Martin'', N''pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/mf.png'', ''MF''),
    (142, N''Madagascar'', N''pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/mg.png'', ''MG''),
    (143, N''Marshall Islands'', N''pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/mh.png'', ''MH''),
    (144, N''North Macedonia'', N''pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/mk.png'', ''MK''),
    (145, N''Mali'', N''pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/ml.png'', ''ML''),
    (146, N''Myanmar'', N''pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/mm.png'', ''MM''),
    (147, N''Mongolia'', N''pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/mn.png'', ''MN''),
    (148, N''Macao SAR'', N''pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/mo.png'', ''MO''),
    (149, N''Northern Mariana Islands'', N''pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/mp.png'', ''MP''),
    (150, N''Martinique'', N''pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/mq.png'', ''MQ''),
    (151, N''Mauritania'', N''pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/mr.png'', ''MR''),
    (152, N''Montserrat'', N''pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/ms.png'', ''MS''),
    (153, N''Malta'', N''pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/mt.png'', ''MT''),
    (154, N''Mauritius'', N''pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/mu.png'', ''MU''),
    (155, N''Maldives'', N''pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/mv.png'', ''MV''),
    (156, N''Malawi'', N''pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/mw.png'', ''MW''),
    (157, N''Mexico'', N''pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/mx.png'', ''MX''),
    (158, N''Malaysia'', N''pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/my.png'', ''MY''),
    (159, N''Mozambique'', N''pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/mz.png'', ''MZ''),
    (160, N''Namibia'', N''pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/na.png'', ''NA''),
    (161, N''New Caledonia'', N''pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/nc.png'', ''NC''),
    (162, N''Niger'', N''pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/ne.png'', ''NE''),
    (163, N''Norfolk Island'', N''pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/nf.png'', ''NF''),
    (164, N''Nigeria'', N''pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/ng.png'', ''NG''),
    (165, N''Nicaragua'', N''pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/ni.png'', ''NI''),
    (166, N''Netherlands'', N''pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/nl.png'', ''NL''),
    (167, N''Norway'', N''pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/no.png'', ''NO''),
    (168, N''Nepal'', N''pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/np.png'', ''NP'');
    INSERT INTO [dbo].[Country] ([CountryID], [CountryName], [FlagUrl], [ISO2Code])
    VALUES (169, N''Nauru'', N''pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/nr.png'', ''NR''),
    (170, N''Niue'', N''pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/nu.png'', ''NU''),
    (171, N''New Zealand'', N''pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/nz.png'', ''NZ''),
    (172, N''Oman'', N''pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/om.png'', ''OM''),
    (173, N''Panama'', N''pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/pa.png'', ''PA''),
    (174, N''Peru'', N''pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/pe.png'', ''PE''),
    (175, N''French Polynesia'', N''pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/pf.png'', ''PF''),
    (176, N''Papua New Guinea'', N''pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/pg.png'', ''PG''),
    (177, N''Philippines'', N''pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/ph.png'', ''PH''),
    (178, N''Pakistan'', N''pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/pk.png'', ''PK''),
    (179, N''Poland'', N''pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/pl.png'', ''PL''),
    (180, N''St. Pierre & Miquelon'', N''pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/pm.png'', ''PM''),
    (181, N''Pitcairn Islands'', N''pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/pn.png'', ''PN''),
    (182, N''Puerto Rico'', N''pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/pr.png'', ''PR''),
    (183, N''Palestinian Authority'', N''pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/ps.png'', ''PS''),
    (184, N''Portugal'', N''pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/pt.png'', ''PT''),
    (185, N''Palau'', N''pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/pw.png'', ''PW''),
    (186, N''Paraguay'', N''pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/py.png'', ''PY''),
    (187, N''Qatar'', N''pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/qa.png'', ''QA''),
    (188, N''Réunion'', N''pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/re.png'', ''RE''),
    (189, N''Romania'', N''pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/ro.png'', ''RO''),
    (190, N''Serbia'', N''pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/rs.png'', ''RS''),
    (191, N''Russia'', N''pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/ru.png'', ''RU''),
    (192, N''Rwanda'', N''pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/rw.png'', ''RW''),
    (193, N''Saudi Arabia'', N''pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/sa.png'', ''SA''),
    (194, N''Solomon Islands'', N''pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/sb.png'', ''SB''),
    (195, N''Seychelles'', N''pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/sc.png'', ''SC''),
    (196, N''Sudan'', N''pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/sd.png'', ''SD''),
    (197, N''Sweden'', N''pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/se.png'', ''SE''),
    (198, N''Singapore'', N''pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/sg.png'', ''SG''),
    (199, N''St Helena, Ascension, Tristan da Cunha'', N''pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/sh.png'', ''SH''),
    (200, N''Slovenia'', N''pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/si.png'', ''SI''),
    (201, N''Svalbard & Jan Mayen'', N''pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/sj.png'', ''SJ''),
    (202, N''Slovakia'', N''pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/sk.png'', ''SK''),
    (203, N''Sierra Leone'', N''pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/sl.png'', ''SL''),
    (204, N''San Marino'', N''pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/sm.png'', ''SM''),
    (205, N''Senegal'', N''pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/sn.png'', ''SN''),
    (206, N''Somalia'', N''pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/so.png'', ''SO''),
    (207, N''Suriname'', N''pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/sr.png'', ''SR''),
    (208, N''South Sudan'', N''pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/ss.png'', ''SS''),
    (209, N''São Tomé & Príncipe'', N''pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/st.png'', ''ST''),
    (210, N''El Salvador'', N''pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/sv.png'', ''SV'');
    INSERT INTO [dbo].[Country] ([CountryID], [CountryName], [FlagUrl], [ISO2Code])
    VALUES (211, N''Sint Maarten'', N''pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/sx.png'', ''SX''),
    (212, N''Syria'', N''pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/sy.png'', ''SY''),
    (213, N''Eswatini'', N''pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/sz.png'', ''SZ''),
    (214, N''Turks & Caicos Islands'', N''pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/tc.png'', ''TC''),
    (215, N''Chad'', N''pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/td.png'', ''TD''),
    (216, N''French Southern Territories'', N''pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/tf.png'', ''TF''),
    (217, N''Togo'', N''pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/tg.png'', ''TG''),
    (218, N''Thailand'', N''pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/th.png'', ''TH''),
    (219, N''Tajikistan'', N''pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/tj.png'', ''TJ''),
    (220, N''Tokelau'', N''pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/tk.png'', ''TK''),
    (221, N''Timor-Leste'', N''pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/tl.png'', ''TL''),
    (222, N''Turkmenistan'', N''pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/tm.png'', ''TM''),
    (223, N''Tunisia'', N''pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/tn.png'', ''TN''),
    (224, N''Tonga'', N''pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/to.png'', ''TO''),
    (225, N''Türkiye'', N''pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/tr.png'', ''TR''),
    (226, N''Trinidad & Tobago'', N''pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/tt.png'', ''TT''),
    (227, N''Tuvalu'', N''pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/tv.png'', ''TV''),
    (228, N''Taiwan'', N''pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/tw.png'', ''TW''),
    (229, N''Tanzania'', N''pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/tz.png'', ''TZ''),
    (230, N''Ukraine'', N''pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/ua.png'', ''UA''),
    (231, N''Uganda'', N''pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/ug.png'', ''UG''),
    (232, N''U.S. Outlying Islands'', N''pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/um.png'', ''UM''),
    (233, N''United States'', N''pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/us.png'', ''US''),
    (234, N''Uruguay'', N''pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/uy.png'', ''UY''),
    (235, N''Uzbekistan'', N''pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/uz.png'', ''UZ''),
    (236, N''Vatican City'', N''pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/va.png'', ''VA''),
    (237, N''St. Vincent & Grenadines'', N''pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/vc.png'', ''VC''),
    (238, N''Venezuela'', N''pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/ve.png'', ''VE''),
    (239, N''British Virgin Islands'', N''pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/vg.png'', ''VG''),
    (240, N''U.S. Virgin Islands'', N''pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/vi.png'', ''VI''),
    (241, N''Vietnam'', N''pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/vn.png'', ''VN''),
    (242, N''Vanuatu'', N''pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/vu.png'', ''VU''),
    (243, N''Wallis & Futuna'', N''pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/wf.png'', ''WF''),
    (244, N''Samoa'', N''pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/ws.png'', ''WS''),
    (245, N''Kosovo (user-assigned XK code)'', N''pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/xk.png'', ''XK''),
    (246, N''Yemen'', N''pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/ye.png'', ''YE''),
    (247, N''Mayotte'', N''pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/yt.png'', ''YT''),
    (248, N''South Africa'', N''pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/za.png'', ''ZA''),
    (249, N''Zambia'', N''pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/zm.png'', ''ZM''),
    (250, N''Zimbabwe'', N''pack://application:,,,/Gajyun.Desktop;component/Assets/Flags/zw.png'', ''ZW'')');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'CountryID', N'CountryName', N'FlagUrl', N'ISO2Code') AND [object_id] = OBJECT_ID(N'[dbo].[Country]'))
        SET IDENTITY_INSERT [dbo].[Country] OFF;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261106083537_InitialSchemaV13'
)
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'DocumentTypeID', N'DocumentTypeName', N'IsActive') AND [object_id] = OBJECT_ID(N'[dbo].[DocumentType]'))
        SET IDENTITY_INSERT [dbo].[DocumentType] ON;
    EXEC(N'INSERT INTO [dbo].[DocumentType] ([DocumentTypeID], [DocumentTypeName], [IsActive])
    VALUES (1, N''Passport'', CAST(1 AS bit)),
    (2, N''Driver''''s license'', CAST(1 AS bit)),
    (3, N''Foreign residence card'', CAST(1 AS bit)),
    (4, N''Other'', CAST(1 AS bit))');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'DocumentTypeID', N'DocumentTypeName', N'IsActive') AND [object_id] = OBJECT_ID(N'[dbo].[DocumentType]'))
        SET IDENTITY_INSERT [dbo].[DocumentType] OFF;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261106083537_InitialSchemaV13'
)
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'OTAID', N'IsActive', N'LogoUrl', N'OTAName') AND [object_id] = OBJECT_ID(N'[dbo].[OTA]'))
        SET IDENTITY_INSERT [dbo].[OTA] ON;
    EXEC(N'INSERT INTO [dbo].[OTA] ([OTAID], [IsActive], [LogoUrl], [OTAName])
    VALUES (1, CAST(1 AS bit), N''pack://application:,,,/Gajyun.Desktop;component/Assets/Logos/Booking-Logo.png'', N''Booking.com''),
    (2, CAST(1 AS bit), N''pack://application:,,,/Gajyun.Desktop;component/Assets/Logos/Agoda-logo.png'', N''Agoda''),
    (3, CAST(1 AS bit), N''pack://application:,,,/Gajyun.Desktop;component/Assets/Logos/hostelworld-seeklogo.png'', N''Hostelworld''),
    (4, CAST(1 AS bit), N''pack://application:,,,/Gajyun.Desktop;component/Assets/Logos/gajyun-logo.png'', N''予約情報入力(直接販売)'')');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'OTAID', N'IsActive', N'LogoUrl', N'OTAName') AND [object_id] = OBJECT_ID(N'[dbo].[OTA]'))
        SET IDENTITY_INSERT [dbo].[OTA] OFF;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261106083537_InitialSchemaV13'
)
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'PaymentMethodID', N'IsActive', N'MethodName') AND [object_id] = OBJECT_ID(N'[dbo].[PaymentMethod]'))
        SET IDENTITY_INSERT [dbo].[PaymentMethod] ON;
    EXEC(N'INSERT INTO [dbo].[PaymentMethod] ([PaymentMethodID], [IsActive], [MethodName])
    VALUES (1, CAST(1 AS bit), N''Cash''),
    (2, CAST(1 AS bit), N''Credit Card''),
    (3, CAST(1 AS bit), N''Bank Transfer''),
    (4, CAST(1 AS bit), N''Other''),
    (5, CAST(1 AS bit), N''Internet payment'')');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'PaymentMethodID', N'IsActive', N'MethodName') AND [object_id] = OBJECT_ID(N'[dbo].[PaymentMethod]'))
        SET IDENTITY_INSERT [dbo].[PaymentMethod] OFF;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261106083537_InitialSchemaV13'
)
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'TaxPolicyID', N'IsActive', N'Notes', N'PolicyName', N'ValidFrom', N'ValidTo') AND [object_id] = OBJECT_ID(N'[dbo].[TaxPolicy]'))
        SET IDENTITY_INSERT [dbo].[TaxPolicy] ON;
    EXEC(N'INSERT INTO [dbo].[TaxPolicy] ([TaxPolicyID], [IsActive], [Notes], [PolicyName], [ValidFrom], [ValidTo])
    VALUES (1, CAST(1 AS bit), N''Verified Kyoto City rates; staff approved 2026-11-06. Meals and consumption tax excluded. https://www.city.kyoto.lg.jp/gyozai/page/0000236942.html'', N''Kyoto City Accommodation Tax 2025'', ''2018-10-01'', ''2026-03-01''),
    (2, CAST(1 AS bit), N''Verified Kyoto City rates; staff approved 2026-11-06. Meals and consumption tax excluded. https://www.city.kyoto.lg.jp/gyozai/page/0000236942.html'', N''Kyoto City Accommodation Tax 2026'', ''2026-03-01'', NULL)');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'TaxPolicyID', N'IsActive', N'Notes', N'PolicyName', N'ValidFrom', N'ValidTo') AND [object_id] = OBJECT_ID(N'[dbo].[TaxPolicy]'))
        SET IDENTITY_INSERT [dbo].[TaxPolicy] OFF;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261106083537_InitialSchemaV13'
)
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'TaxBracketID', N'MaxPriceExclusive', N'MinPriceInclusive', N'TaxPerPersonNight', N'TaxPolicyID') AND [object_id] = OBJECT_ID(N'[dbo].[TaxBracket]'))
        SET IDENTITY_INSERT [dbo].[TaxBracket] ON;
    EXEC(N'INSERT INTO [dbo].[TaxBracket] ([TaxBracketID], [MaxPriceExclusive], [MinPriceInclusive], [TaxPerPersonNight], [TaxPolicyID])
    VALUES (1, 20000.0, 0.0, 200.0, 1),
    (2, 50000.0, 20000.0, 500.0, 1),
    (3, NULL, 50000.0, 1000.0, 1),
    (4, 6000.0, 0.0, 200.0, 2),
    (5, 20000.0, 6000.0, 400.0, 2),
    (6, 50000.0, 20000.0, 1000.0, 2),
    (7, 100000.0, 50000.0, 4000.0, 2),
    (8, NULL, 100000.0, 10000.0, 2)');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'TaxBracketID', N'MaxPriceExclusive', N'MinPriceInclusive', N'TaxPerPersonNight', N'TaxPolicyID') AND [object_id] = OBJECT_ID(N'[dbo].[TaxBracket]'))
        SET IDENTITY_INSERT [dbo].[TaxBracket] OFF;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261106083537_InitialSchemaV13'
)
BEGIN
    CREATE UNIQUE INDEX [IX_BookingStatus_StatusName] ON [dbo].[BookingStatus] ([StatusName]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261106083537_InitialSchemaV13'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [UX_Country_ISO2Code] ON [dbo].[Country] ([ISO2Code]) WHERE [ISO2Code] IS NOT NULL');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261106083537_InitialSchemaV13'
)
BEGIN
    CREATE INDEX [IX_Customer_CountryID] ON [dbo].[Customer] ([CountryID]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261106083537_InitialSchemaV13'
)
BEGIN
    CREATE INDEX [IX_CustomerDocument_CustomerID] ON [dbo].[CustomerDocument] ([CustomerID]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261106083537_InitialSchemaV13'
)
BEGIN
    CREATE INDEX [IX_CustomerDocument_DocumentTypeID] ON [dbo].[CustomerDocument] ([DocumentTypeID]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261106083537_InitialSchemaV13'
)
BEGIN
    CREATE UNIQUE INDEX [IX_DocumentType_DocumentTypeName] ON [dbo].[DocumentType] ([DocumentTypeName]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261106083537_InitialSchemaV13'
)
BEGIN
    CREATE UNIQUE INDEX [IX_ExtraService_ServiceName] ON [dbo].[ExtraService] ([ServiceName]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261106083537_InitialSchemaV13'
)
BEGIN
    CREATE INDEX [IX_ImportBatch_OTAID] ON [dbo].[ImportBatch] ([OTAID]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261106083537_InitialSchemaV13'
)
BEGIN
    CREATE INDEX [IX_ImportRow_ReservationID_SourceRowHash] ON [dbo].[ImportRow] ([ReservationID], [SourceRowHash]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261106083537_InitialSchemaV13'
)
BEGIN
    CREATE UNIQUE INDEX [IX_OTA_OTAName] ON [dbo].[OTA] ([OTAName]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261106083537_InitialSchemaV13'
)
BEGIN
    CREATE INDEX [IX_OTARoomMapping_NormalizedName] ON [dbo].[OTARoomMapping] ([NormalizedName]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261106083537_InitialSchemaV13'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [UX_OTARoomMapping_GenericRoomName] ON [dbo].[OTARoomMapping] ([RoomID], [NormalizedName]) WHERE [OTAID] IS NULL AND [IsActive] = 1');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261106083537_InitialSchemaV13'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [UX_OTARoomMapping_OTAName] ON [dbo].[OTARoomMapping] ([OTAID], [NormalizedName]) WHERE [OTAID] IS NOT NULL AND [IsActive] = 1');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261106083537_InitialSchemaV13'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [UX_OTAStatementItem_SourceLine] ON [dbo].[OTAStatementItem] ([ReservationID], [SourceLineKey]) WHERE [SourceLineKey] IS NOT NULL');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261106083537_InitialSchemaV13'
)
BEGIN
    CREATE INDEX [IX_PaymentAllocation_ChargeID] ON [dbo].[PaymentAllocation] ([ChargeID]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261106083537_InitialSchemaV13'
)
BEGIN
    CREATE UNIQUE INDEX [IX_PaymentMethod_MethodName] ON [dbo].[PaymentMethod] ([MethodName]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261106083537_InitialSchemaV13'
)
BEGIN
    CREATE INDEX [IX_Reservation_BookedAt] ON [dbo].[Reservation] ([BookedAt]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261106083537_InitialSchemaV13'
)
BEGIN
    CREATE INDEX [IX_Reservation_BookingStatusID] ON [dbo].[Reservation] ([BookingStatusID]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261106083537_InitialSchemaV13'
)
BEGIN
    CREATE INDEX [IX_Reservation_CheckInDate] ON [dbo].[Reservation] ([CheckInDate]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261106083537_InitialSchemaV13'
)
BEGIN
    CREATE INDEX [IX_Reservation_CheckOutDate] ON [dbo].[Reservation] ([CheckOutDate]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261106083537_InitialSchemaV13'
)
BEGIN
    CREATE INDEX [IX_Reservation_GroupID] ON [dbo].[Reservation] ([GroupID]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261106083537_InitialSchemaV13'
)
BEGIN
    CREATE INDEX [IX_Reservation_SourcePaymentMethodID] ON [dbo].[Reservation] ([SourcePaymentMethodID]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261106083537_InitialSchemaV13'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [UX_Reservation_OTA_ExternalNumber] ON [dbo].[Reservation] ([OTAID], [ExternalReservationNumber]) WHERE [ExternalReservationNumber] IS NOT NULL');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261106083537_InitialSchemaV13'
)
BEGIN
    CREATE INDEX [IX_ReservationAccommodation_BedID_RoomID] ON [dbo].[ReservationAccommodation] ([BedID], [RoomID]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261106083537_InitialSchemaV13'
)
BEGIN
    CREATE INDEX [IX_ReservationAccommodation_Inventory] ON [dbo].[ReservationAccommodation] ([RoomID], [BedID], [StartDate], [EndDate]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261106083537_InitialSchemaV13'
)
BEGIN
    CREATE INDEX [IX_ReservationAccommodation_ReservationID] ON [dbo].[ReservationAccommodation] ([ReservationID]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261106083537_InitialSchemaV13'
)
BEGIN
    CREATE INDEX [IX_ReservationCharge_ExtraServiceID] ON [dbo].[ReservationCharge] ([ExtraServiceID]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261106083537_InitialSchemaV13'
)
BEGIN
    CREATE INDEX [IX_ReservationCharge_Reservation] ON [dbo].[ReservationCharge] ([ReservationID]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261106083537_InitialSchemaV13'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [UX_ReservationCharge_SourceLine] ON [dbo].[ReservationCharge] ([ReservationID], [SourceLineKey]) WHERE [SourceLineKey] IS NOT NULL');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261106083537_InitialSchemaV13'
)
BEGIN
    CREATE INDEX [IX_ReservationCustomer_CustomerID] ON [dbo].[ReservationCustomer] ([CustomerID]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261106083537_InitialSchemaV13'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [UX_ReservationCustomer_OneLead] ON [dbo].[ReservationCustomer] ([ReservationID]) WHERE [IsLeadGuest] = 1');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261106083537_InitialSchemaV13'
)
BEGIN
    CREATE INDEX [IX_ReservationPayment_PaymentMethodID] ON [dbo].[ReservationPayment] ([PaymentMethodID]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261106083537_InitialSchemaV13'
)
BEGIN
    CREATE INDEX [IX_ReservationPayment_Reservation] ON [dbo].[ReservationPayment] ([ReservationID]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261106083537_InitialSchemaV13'
)
BEGIN
    CREATE INDEX [IX_ReservationTax_AppliedTaxPolicyID] ON [dbo].[ReservationTax] ([AppliedTaxPolicyID]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261106083537_InitialSchemaV13'
)
BEGIN
    CREATE INDEX [IX_ReservationTax_LegalTaxPolicyID] ON [dbo].[ReservationTax] ([LegalTaxPolicyID]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261106083537_InitialSchemaV13'
)
BEGIN
    CREATE INDEX [IX_ReservationTax_ReservationID_ServiceDate] ON [dbo].[ReservationTax] ([ReservationID], [ServiceDate]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261106083537_InitialSchemaV13'
)
BEGIN
    CREATE INDEX [IX_ReservationTax_RoomID] ON [dbo].[ReservationTax] ([RoomID]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261106083537_InitialSchemaV13'
)
BEGIN
    CREATE INDEX [IX_Room_DefaultTaxPolicyID] ON [dbo].[Room] ([DefaultTaxPolicyID]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261106083537_InitialSchemaV13'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Room_RoomName] ON [dbo].[Room] ([RoomName]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261106083537_InitialSchemaV13'
)
BEGIN
    CREATE INDEX [IX_Stay_CustomerID] ON [dbo].[Stay] ([CustomerID]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261106083537_InitialSchemaV13'
)
BEGIN
    CREATE INDEX [IX_StayMemo_StayID] ON [dbo].[StayMemo] ([StayID]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261106083537_InitialSchemaV13'
)
BEGIN
    CREATE INDEX [IX_StayReservation_ReservationID] ON [dbo].[StayReservation] ([ReservationID]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261106083537_InitialSchemaV13'
)
BEGIN
    CREATE UNIQUE INDEX [IX_TaxBracket_TaxPolicyID_MinPriceInclusive] ON [dbo].[TaxBracket] ([TaxPolicyID], [MinPriceInclusive]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261106083537_InitialSchemaV13'
)
BEGIN
    CREATE UNIQUE INDEX [IX_TaxPolicy_PolicyName] ON [dbo].[TaxPolicy] ([PolicyName]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261106083537_InitialSchemaV13'
)
BEGIN
    EXEC(N'CREATE VIEW dbo.vwReservationFinancialSummary AS
    WITH C AS (
        SELECT ReservationID, CurrencyCode, SUM(Amount) AS TotalCharged
        FROM dbo.ReservationCharge GROUP BY ReservationID, CurrencyCode
    ), P AS (
        SELECT ReservationID, CurrencyCode,
            SUM(CASE WHEN Direction = ''Inflow'' THEN Amount ELSE -Amount END) AS NetCashMovement
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
    LEFT JOIN P ON P.ReservationID = K.ReservationID AND P.CurrencyCode = K.CurrencyCode;');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261106083537_InitialSchemaV13'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261106083537_InitialSchemaV13', N'10.0.14');
END;

COMMIT;
GO

