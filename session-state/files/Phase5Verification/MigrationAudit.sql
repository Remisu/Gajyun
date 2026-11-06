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
    WHERE [MigrationId] = N'20261106054221_InitialSchemaV11'
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
    WHERE [MigrationId] = N'20261106054221_InitialSchemaV11'
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
    WHERE [MigrationId] = N'20261106054221_InitialSchemaV11'
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
    WHERE [MigrationId] = N'20261106054221_InitialSchemaV11'
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
    WHERE [MigrationId] = N'20261106054221_InitialSchemaV11'
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
    WHERE [MigrationId] = N'20261106054221_InitialSchemaV11'
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
    WHERE [MigrationId] = N'20261106054221_InitialSchemaV11'
)
BEGIN
    CREATE TABLE [dbo].[ReservationGroup] (
        [GroupID] bigint NOT NULL IDENTITY,
        [ExternalGroupReference] nvarchar(120) NULL,
        [Notes] nvarchar(1000) NULL,
        CONSTRAINT [PK_ReservationGroup] PRIMARY KEY ([GroupID])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261106054221_InitialSchemaV11'
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
    WHERE [MigrationId] = N'20261106054221_InitialSchemaV11'
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
    WHERE [MigrationId] = N'20261106054221_InitialSchemaV11'
)
BEGIN
    CREATE TABLE [dbo].[ImportBatch] (
        [ImportBatchID] bigint NOT NULL IDENTITY,
        [OTAID] int NOT NULL,
        [SourceFileName] nvarchar(255) NULL,
        [ImportedAt] datetime2(0) NOT NULL CONSTRAINT [DF_ImportBatch_Imported] DEFAULT (SYSDATETIME()),
        CONSTRAINT [PK_ImportBatch] PRIMARY KEY ([ImportBatchID]),
        CONSTRAINT [FK_ImportBatch_OTA] FOREIGN KEY ([OTAID]) REFERENCES [dbo].[OTA] ([OTAID])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261106054221_InitialSchemaV11'
)
BEGIN
    CREATE TABLE [dbo].[Reservation] (
        [ReservationID] bigint NOT NULL IDENTITY,
        [OTAID] int NOT NULL,
        [ExternalReservationNumber] nvarchar(120) NULL,
        [GroupID] bigint NULL,
        [BookingStatusID] int NOT NULL,
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
        CONSTRAINT [FK_Reservation_OTA] FOREIGN KEY ([OTAID]) REFERENCES [dbo].[OTA] ([OTAID])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261106054221_InitialSchemaV11'
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
    WHERE [MigrationId] = N'20261106054221_InitialSchemaV11'
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
    WHERE [MigrationId] = N'20261106054221_InitialSchemaV11'
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
    WHERE [MigrationId] = N'20261106054221_InitialSchemaV11'
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
    WHERE [MigrationId] = N'20261106054221_InitialSchemaV11'
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
        CONSTRAINT [PK_ImportRow] PRIMARY KEY ([ImportRowID]),
        CONSTRAINT [UQ_ImportRow_BatchNumber] UNIQUE ([ImportBatchID], [SourceRowNumber]),
        CONSTRAINT [CK_ImportRow_Status] CHECK ([ParseStatus] IN ('Pending','Processed','Failed','Skipped')),
        CONSTRAINT [FK_ImportRow_Batch] FOREIGN KEY ([ImportBatchID]) REFERENCES [dbo].[ImportBatch] ([ImportBatchID]),
        CONSTRAINT [FK_ImportRow_Reservation] FOREIGN KEY ([ReservationID]) REFERENCES [dbo].[Reservation] ([ReservationID])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261106054221_InitialSchemaV11'
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
    WHERE [MigrationId] = N'20261106054221_InitialSchemaV11'
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
    WHERE [MigrationId] = N'20261106054221_InitialSchemaV11'
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
    WHERE [MigrationId] = N'20261106054221_InitialSchemaV11'
)
BEGIN
    CREATE TABLE [dbo].[ReservationMemo] (
        [MemoID] bigint NOT NULL IDENTITY,
        [ReservationID] bigint NOT NULL,
        [MemoText] nvarchar(max) NOT NULL,
        [CreatedAt] datetime2(0) NOT NULL CONSTRAINT [DF_ReservationMemo_Created] DEFAULT (SYSDATETIME()),
        CONSTRAINT [PK_ReservationMemo] PRIMARY KEY ([MemoID]),
        CONSTRAINT [FK_ReservationMemo_Reservation] FOREIGN KEY ([ReservationID]) REFERENCES [dbo].[Reservation] ([ReservationID])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261106054221_InitialSchemaV11'
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
    WHERE [MigrationId] = N'20261106054221_InitialSchemaV11'
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
    WHERE [MigrationId] = N'20261106054221_InitialSchemaV11'
)
BEGIN
    CREATE TABLE [dbo].[OTARoomMapping] (
        [OTARoomMappingID] int NOT NULL IDENTITY,
        [OTAID] int NULL,
        [RoomID] int NOT NULL,
        [ExternalRoomCode] nvarchar(120) COLLATE Latin1_General_100_BIN2 NULL,
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
    WHERE [MigrationId] = N'20261106054221_InitialSchemaV11'
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
    WHERE [MigrationId] = N'20261106054221_InitialSchemaV11'
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
    WHERE [MigrationId] = N'20261106054221_InitialSchemaV11'
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
    WHERE [MigrationId] = N'20261106054221_InitialSchemaV11'
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
    WHERE [MigrationId] = N'20261106054221_InitialSchemaV11'
)
BEGIN
    CREATE UNIQUE INDEX [IX_BookingStatus_StatusName] ON [dbo].[BookingStatus] ([StatusName]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261106054221_InitialSchemaV11'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [UX_Country_ISO2Code] ON [dbo].[Country] ([ISO2Code]) WHERE [ISO2Code] IS NOT NULL');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261106054221_InitialSchemaV11'
)
BEGIN
    CREATE INDEX [IX_Customer_CountryID] ON [dbo].[Customer] ([CountryID]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261106054221_InitialSchemaV11'
)
BEGIN
    CREATE INDEX [IX_CustomerDocument_CustomerID] ON [dbo].[CustomerDocument] ([CustomerID]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261106054221_InitialSchemaV11'
)
BEGIN
    CREATE INDEX [IX_CustomerDocument_DocumentTypeID] ON [dbo].[CustomerDocument] ([DocumentTypeID]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261106054221_InitialSchemaV11'
)
BEGIN
    CREATE UNIQUE INDEX [IX_DocumentType_DocumentTypeName] ON [dbo].[DocumentType] ([DocumentTypeName]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261106054221_InitialSchemaV11'
)
BEGIN
    CREATE UNIQUE INDEX [IX_ExtraService_ServiceName] ON [dbo].[ExtraService] ([ServiceName]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261106054221_InitialSchemaV11'
)
BEGIN
    CREATE INDEX [IX_ImportBatch_OTAID] ON [dbo].[ImportBatch] ([OTAID]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261106054221_InitialSchemaV11'
)
BEGIN
    CREATE INDEX [IX_ImportRow_ReservationID] ON [dbo].[ImportRow] ([ReservationID]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261106054221_InitialSchemaV11'
)
BEGIN
    CREATE UNIQUE INDEX [IX_OTA_OTAName] ON [dbo].[OTA] ([OTAName]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261106054221_InitialSchemaV11'
)
BEGIN
    CREATE INDEX [IX_OTARoomMapping_NormalizedName] ON [dbo].[OTARoomMapping] ([NormalizedName]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261106054221_InitialSchemaV11'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [UX_OTARoomMapping_Code] ON [dbo].[OTARoomMapping] ([OTAID], [ExternalRoomCode]) WHERE [OTAID] IS NOT NULL AND [ExternalRoomCode] IS NOT NULL AND [IsActive] = 1');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261106054221_InitialSchemaV11'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [UX_OTARoomMapping_GenericRoomName] ON [dbo].[OTARoomMapping] ([RoomID], [NormalizedName]) WHERE [OTAID] IS NULL AND [IsActive] = 1');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261106054221_InitialSchemaV11'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [UX_OTARoomMapping_OTAName] ON [dbo].[OTARoomMapping] ([OTAID], [NormalizedName]) WHERE [OTAID] IS NOT NULL AND [IsActive] = 1');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261106054221_InitialSchemaV11'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [UX_OTAStatementItem_SourceLine] ON [dbo].[OTAStatementItem] ([ReservationID], [SourceLineKey]) WHERE [SourceLineKey] IS NOT NULL');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261106054221_InitialSchemaV11'
)
BEGIN
    CREATE INDEX [IX_PaymentAllocation_ChargeID] ON [dbo].[PaymentAllocation] ([ChargeID]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261106054221_InitialSchemaV11'
)
BEGIN
    CREATE UNIQUE INDEX [IX_PaymentMethod_MethodName] ON [dbo].[PaymentMethod] ([MethodName]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261106054221_InitialSchemaV11'
)
BEGIN
    CREATE INDEX [IX_Reservation_BookingStatusID] ON [dbo].[Reservation] ([BookingStatusID]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261106054221_InitialSchemaV11'
)
BEGIN
    CREATE INDEX [IX_Reservation_CheckInDate] ON [dbo].[Reservation] ([CheckInDate]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261106054221_InitialSchemaV11'
)
BEGIN
    CREATE INDEX [IX_Reservation_CheckOutDate] ON [dbo].[Reservation] ([CheckOutDate]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261106054221_InitialSchemaV11'
)
BEGIN
    CREATE INDEX [IX_Reservation_GroupID] ON [dbo].[Reservation] ([GroupID]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261106054221_InitialSchemaV11'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [UX_Reservation_OTA_ExternalNumber] ON [dbo].[Reservation] ([OTAID], [ExternalReservationNumber]) WHERE [ExternalReservationNumber] IS NOT NULL');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261106054221_InitialSchemaV11'
)
BEGIN
    CREATE INDEX [IX_ReservationAccommodation_BedID_RoomID] ON [dbo].[ReservationAccommodation] ([BedID], [RoomID]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261106054221_InitialSchemaV11'
)
BEGIN
    CREATE INDEX [IX_ReservationAccommodation_Inventory] ON [dbo].[ReservationAccommodation] ([RoomID], [BedID], [StartDate], [EndDate]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261106054221_InitialSchemaV11'
)
BEGIN
    CREATE INDEX [IX_ReservationAccommodation_ReservationID] ON [dbo].[ReservationAccommodation] ([ReservationID]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261106054221_InitialSchemaV11'
)
BEGIN
    CREATE INDEX [IX_ReservationCharge_ExtraServiceID] ON [dbo].[ReservationCharge] ([ExtraServiceID]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261106054221_InitialSchemaV11'
)
BEGIN
    CREATE INDEX [IX_ReservationCharge_Reservation] ON [dbo].[ReservationCharge] ([ReservationID]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261106054221_InitialSchemaV11'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [UX_ReservationCharge_SourceLine] ON [dbo].[ReservationCharge] ([ReservationID], [SourceLineKey]) WHERE [SourceLineKey] IS NOT NULL');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261106054221_InitialSchemaV11'
)
BEGIN
    CREATE INDEX [IX_ReservationCustomer_CustomerID] ON [dbo].[ReservationCustomer] ([CustomerID]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261106054221_InitialSchemaV11'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [UX_ReservationCustomer_OneLead] ON [dbo].[ReservationCustomer] ([ReservationID]) WHERE [IsLeadGuest] = 1');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261106054221_InitialSchemaV11'
)
BEGIN
    CREATE INDEX [IX_ReservationMemo_ReservationID] ON [dbo].[ReservationMemo] ([ReservationID]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261106054221_InitialSchemaV11'
)
BEGIN
    CREATE INDEX [IX_ReservationPayment_PaymentMethodID] ON [dbo].[ReservationPayment] ([PaymentMethodID]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261106054221_InitialSchemaV11'
)
BEGIN
    CREATE INDEX [IX_ReservationPayment_Reservation] ON [dbo].[ReservationPayment] ([ReservationID]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261106054221_InitialSchemaV11'
)
BEGIN
    CREATE INDEX [IX_ReservationTax_AppliedTaxPolicyID] ON [dbo].[ReservationTax] ([AppliedTaxPolicyID]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261106054221_InitialSchemaV11'
)
BEGIN
    CREATE INDEX [IX_ReservationTax_LegalTaxPolicyID] ON [dbo].[ReservationTax] ([LegalTaxPolicyID]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261106054221_InitialSchemaV11'
)
BEGIN
    CREATE INDEX [IX_ReservationTax_ReservationID_ServiceDate] ON [dbo].[ReservationTax] ([ReservationID], [ServiceDate]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261106054221_InitialSchemaV11'
)
BEGIN
    CREATE INDEX [IX_ReservationTax_RoomID] ON [dbo].[ReservationTax] ([RoomID]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261106054221_InitialSchemaV11'
)
BEGIN
    CREATE INDEX [IX_Room_DefaultTaxPolicyID] ON [dbo].[Room] ([DefaultTaxPolicyID]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261106054221_InitialSchemaV11'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Room_RoomName] ON [dbo].[Room] ([RoomName]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261106054221_InitialSchemaV11'
)
BEGIN
    CREATE INDEX [IX_Stay_CustomerID] ON [dbo].[Stay] ([CustomerID]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261106054221_InitialSchemaV11'
)
BEGIN
    CREATE INDEX [IX_StayReservation_ReservationID] ON [dbo].[StayReservation] ([ReservationID]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261106054221_InitialSchemaV11'
)
BEGIN
    CREATE UNIQUE INDEX [IX_TaxBracket_TaxPolicyID_MinPriceInclusive] ON [dbo].[TaxBracket] ([TaxPolicyID], [MinPriceInclusive]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261106054221_InitialSchemaV11'
)
BEGIN
    CREATE UNIQUE INDEX [IX_TaxPolicy_PolicyName] ON [dbo].[TaxPolicy] ([PolicyName]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261106054221_InitialSchemaV11'
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
    WHERE [MigrationId] = N'20261106054221_InitialSchemaV11'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261106054221_InitialSchemaV11', N'10.0.14');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261106065008_ImportSourceAudit'
)
BEGIN
    DROP INDEX [IX_ImportRow_ReservationID] ON [dbo].[ImportRow];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261106065008_ImportSourceAudit'
)
BEGIN
    ALTER TABLE [dbo].[ImportRow] ADD [SourceRowHash] varchar(64) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261106065008_ImportSourceAudit'
)
BEGIN
    ALTER TABLE [dbo].[ImportBatch] ADD [ParserVersion] nvarchar(40) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261106065008_ImportSourceAudit'
)
BEGIN
    ALTER TABLE [dbo].[ImportBatch] ADD [SourceEncoding] nvarchar(100) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261106065008_ImportSourceAudit'
)
BEGIN
    ALTER TABLE [dbo].[ImportBatch] ADD [SourceFileContent] varbinary(max) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261106065008_ImportSourceAudit'
)
BEGIN
    ALTER TABLE [dbo].[ImportBatch] ADD [SourceFileHash] varchar(64) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261106065008_ImportSourceAudit'
)
BEGIN
    CREATE INDEX [IX_ImportRow_ReservationID_SourceRowHash] ON [dbo].[ImportRow] ([ReservationID], [SourceRowHash]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261106065008_ImportSourceAudit'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261106065008_ImportSourceAudit', N'10.0.14');
END;

COMMIT;
GO

