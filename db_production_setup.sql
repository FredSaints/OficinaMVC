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
CREATE TABLE [AspNetRoles] (
    [Id] nvarchar(450) NOT NULL,
    [Name] nvarchar(256) NULL,
    [NormalizedName] nvarchar(256) NULL,
    [ConcurrencyStamp] nvarchar(max) NULL,
    CONSTRAINT [PK_AspNetRoles] PRIMARY KEY ([Id])
);

CREATE TABLE [AspNetUsers] (
    [Id] nvarchar(450) NOT NULL,
    [FirstName] nvarchar(50) NOT NULL,
    [LastName] nvarchar(50) NOT NULL,
    [NIF] nvarchar(9) NOT NULL,
    [ProfileImageUrl] nvarchar(max) NULL,
    [UserName] nvarchar(256) NULL,
    [NormalizedUserName] nvarchar(256) NULL,
    [Email] nvarchar(256) NULL,
    [NormalizedEmail] nvarchar(256) NULL,
    [EmailConfirmed] bit NOT NULL,
    [PasswordHash] nvarchar(max) NULL,
    [SecurityStamp] nvarchar(max) NULL,
    [ConcurrencyStamp] nvarchar(max) NULL,
    [PhoneNumber] nvarchar(max) NULL,
    [PhoneNumberConfirmed] bit NOT NULL,
    [TwoFactorEnabled] bit NOT NULL,
    [LockoutEnd] datetimeoffset NULL,
    [LockoutEnabled] bit NOT NULL,
    [AccessFailedCount] int NOT NULL,
    CONSTRAINT [PK_AspNetUsers] PRIMARY KEY ([Id])
);

CREATE TABLE [AspNetRoleClaims] (
    [Id] int NOT NULL IDENTITY,
    [RoleId] nvarchar(450) NOT NULL,
    [ClaimType] nvarchar(max) NULL,
    [ClaimValue] nvarchar(max) NULL,
    CONSTRAINT [PK_AspNetRoleClaims] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_AspNetRoleClaims_AspNetRoles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [AspNetRoles] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [AspNetUserClaims] (
    [Id] int NOT NULL IDENTITY,
    [UserId] nvarchar(450) NOT NULL,
    [ClaimType] nvarchar(max) NULL,
    [ClaimValue] nvarchar(max) NULL,
    CONSTRAINT [PK_AspNetUserClaims] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_AspNetUserClaims_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [AspNetUserLogins] (
    [LoginProvider] nvarchar(450) NOT NULL,
    [ProviderKey] nvarchar(450) NOT NULL,
    [ProviderDisplayName] nvarchar(max) NULL,
    [UserId] nvarchar(450) NOT NULL,
    CONSTRAINT [PK_AspNetUserLogins] PRIMARY KEY ([LoginProvider], [ProviderKey]),
    CONSTRAINT [FK_AspNetUserLogins_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [AspNetUserRoles] (
    [UserId] nvarchar(450) NOT NULL,
    [RoleId] nvarchar(450) NOT NULL,
    CONSTRAINT [PK_AspNetUserRoles] PRIMARY KEY ([UserId], [RoleId]),
    CONSTRAINT [FK_AspNetUserRoles_AspNetRoles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [AspNetRoles] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_AspNetUserRoles_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [AspNetUserTokens] (
    [UserId] nvarchar(450) NOT NULL,
    [LoginProvider] nvarchar(450) NOT NULL,
    [Name] nvarchar(450) NOT NULL,
    [Value] nvarchar(max) NULL,
    CONSTRAINT [PK_AspNetUserTokens] PRIMARY KEY ([UserId], [LoginProvider], [Name]),
    CONSTRAINT [FK_AspNetUserTokens_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [Vehicles] (
    [Id] int NOT NULL IDENTITY,
    [LicensePlate] nvarchar(10) NOT NULL,
    [Brand] nvarchar(30) NOT NULL,
    [Model] nvarchar(30) NOT NULL,
    [Year] int NOT NULL,
    [FuelType] nvarchar(20) NULL,
    [OwnerId] nvarchar(450) NOT NULL,
    CONSTRAINT [PK_Vehicles] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Vehicles_AspNetUsers_OwnerId] FOREIGN KEY ([OwnerId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [Appointments] (
    [Id] int NOT NULL IDENTITY,
    [Date] datetime2 NOT NULL,
    [ConclusionDate] datetime2 NULL,
    [ServiceType] nvarchar(50) NOT NULL,
    [Status] nvarchar(20) NOT NULL,
    [Notes] nvarchar(200) NULL,
    [ClientId] nvarchar(450) NOT NULL,
    [MechanicId] nvarchar(450) NOT NULL,
    [VehicleId] int NOT NULL,
    CONSTRAINT [PK_Appointments] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Appointments_AspNetUsers_ClientId] FOREIGN KEY ([ClientId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_Appointments_AspNetUsers_MechanicId] FOREIGN KEY ([MechanicId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_Appointments_Vehicles_VehicleId] FOREIGN KEY ([VehicleId]) REFERENCES [Vehicles] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [Repairs] (
    [Id] int NOT NULL IDENTITY,
    [StartDate] datetime2 NOT NULL,
    [EndDate] datetime2 NULL,
    [Description] nvarchar(100) NOT NULL,
    [TotalCost] decimal(10,2) NOT NULL,
    [Status] nvarchar(20) NOT NULL,
    [VehicleId] int NOT NULL,
    CONSTRAINT [PK_Repairs] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Repairs_Vehicles_VehicleId] FOREIGN KEY ([VehicleId]) REFERENCES [Vehicles] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [RepairUser] (
    [MechanicsId] nvarchar(450) NOT NULL,
    [RepairId] int NOT NULL,
    CONSTRAINT [PK_RepairUser] PRIMARY KEY ([MechanicsId], [RepairId]),
    CONSTRAINT [FK_RepairUser_AspNetUsers_MechanicsId] FOREIGN KEY ([MechanicsId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_RepairUser_Repairs_RepairId] FOREIGN KEY ([RepairId]) REFERENCES [Repairs] ([Id]) ON DELETE NO ACTION
);

CREATE INDEX [IX_Appointments_ClientId] ON [Appointments] ([ClientId]);

CREATE INDEX [IX_Appointments_MechanicId] ON [Appointments] ([MechanicId]);

CREATE INDEX [IX_Appointments_VehicleId] ON [Appointments] ([VehicleId]);

CREATE INDEX [IX_AspNetRoleClaims_RoleId] ON [AspNetRoleClaims] ([RoleId]);

CREATE UNIQUE INDEX [RoleNameIndex] ON [AspNetRoles] ([NormalizedName]) WHERE [NormalizedName] IS NOT NULL;

CREATE INDEX [IX_AspNetUserClaims_UserId] ON [AspNetUserClaims] ([UserId]);

CREATE INDEX [IX_AspNetUserLogins_UserId] ON [AspNetUserLogins] ([UserId]);

CREATE INDEX [IX_AspNetUserRoles_RoleId] ON [AspNetUserRoles] ([RoleId]);

CREATE INDEX [EmailIndex] ON [AspNetUsers] ([NormalizedEmail]);

CREATE UNIQUE INDEX [IX_AspNetUsers_NIF] ON [AspNetUsers] ([NIF]);

CREATE UNIQUE INDEX [UserNameIndex] ON [AspNetUsers] ([NormalizedUserName]) WHERE [NormalizedUserName] IS NOT NULL;

CREATE INDEX [IX_Repairs_VehicleId] ON [Repairs] ([VehicleId]);

CREATE INDEX [IX_RepairUser_RepairId] ON [RepairUser] ([RepairId]);

CREATE UNIQUE INDEX [IX_Vehicles_LicensePlate] ON [Vehicles] ([LicensePlate]);

CREATE INDEX [IX_Vehicles_OwnerId] ON [Vehicles] ([OwnerId]);

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250609130004_InitialCreate', N'9.0.5');

ALTER TABLE [Appointments] DROP CONSTRAINT [FK_Appointments_Vehicles_VehicleId];

ALTER TABLE [Repairs] DROP CONSTRAINT [FK_Repairs_Vehicles_VehicleId];

ALTER TABLE [Vehicles] DROP CONSTRAINT [FK_Vehicles_AspNetUsers_OwnerId];

ALTER TABLE [Appointments] ADD CONSTRAINT [FK_Appointments_Vehicles_VehicleId] FOREIGN KEY ([VehicleId]) REFERENCES [Vehicles] ([Id]) ON DELETE NO ACTION;

ALTER TABLE [Repairs] ADD CONSTRAINT [FK_Repairs_Vehicles_VehicleId] FOREIGN KEY ([VehicleId]) REFERENCES [Vehicles] ([Id]) ON DELETE NO ACTION;

ALTER TABLE [Vehicles] ADD CONSTRAINT [FK_Vehicles_AspNetUsers_OwnerId] FOREIGN KEY ([OwnerId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE NO ACTION;

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250611172929_Roles', N'9.0.5');

DECLARE @var sysname;
SELECT @var = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Vehicles]') AND [c].[name] = N'FuelType');
IF @var IS NOT NULL EXEC(N'ALTER TABLE [Vehicles] DROP CONSTRAINT [' + @var + '];');
UPDATE [Vehicles] SET [FuelType] = 0 WHERE [FuelType] IS NULL;
ALTER TABLE [Vehicles] ALTER COLUMN [FuelType] int NOT NULL;
ALTER TABLE [Vehicles] ADD DEFAULT 0 FOR [FuelType];

CREATE TABLE [Schedules] (
    [Id] int NOT NULL IDENTITY,
    [UserId] nvarchar(450) NOT NULL,
    [DayOfWeek] int NOT NULL,
    [StartTime] time NOT NULL,
    [EndTime] time NOT NULL,
    CONSTRAINT [PK_Schedules] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Schedules_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE NO ACTION
);

CREATE TABLE [Specialties] (
    [Id] int NOT NULL IDENTITY,
    [Name] nvarchar(50) NOT NULL,
    CONSTRAINT [PK_Specialties] PRIMARY KEY ([Id])
);

CREATE TABLE [UserSpecialties] (
    [UserId] nvarchar(450) NOT NULL,
    [SpecialtyId] int NOT NULL,
    CONSTRAINT [PK_UserSpecialties] PRIMARY KEY ([UserId], [SpecialtyId]),
    CONSTRAINT [FK_UserSpecialties_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_UserSpecialties_Specialties_SpecialtyId] FOREIGN KEY ([SpecialtyId]) REFERENCES [Specialties] ([Id]) ON DELETE NO ACTION
);

CREATE INDEX [IX_Schedules_UserId] ON [Schedules] ([UserId]);

CREATE INDEX [IX_UserSpecialties_SpecialtyId] ON [UserSpecialties] ([SpecialtyId]);

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250612182211_MechanicsSpecialtiesSchedules', N'9.0.5');

CREATE TABLE [RepairTypes] (
    [Id] int NOT NULL IDENTITY,
    [Name] nvarchar(100) NOT NULL,
    [Description] nvarchar(250) NOT NULL,
    CONSTRAINT [PK_RepairTypes] PRIMARY KEY ([Id])
);

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250613120945_AddRepairType', N'9.0.5');

EXEC sp_rename N'[Vehicles].[Model]', N'CarModel', 'COLUMN';

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250616111422_RenameModelToCarModel', N'9.0.5');

DECLARE @var1 sysname;
SELECT @var1 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Vehicles]') AND [c].[name] = N'Brand');
IF @var1 IS NOT NULL EXEC(N'ALTER TABLE [Vehicles] DROP CONSTRAINT [' + @var1 + '];');
ALTER TABLE [Vehicles] DROP COLUMN [Brand];

DECLARE @var2 sysname;
SELECT @var2 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Vehicles]') AND [c].[name] = N'CarModel');
IF @var2 IS NOT NULL EXEC(N'ALTER TABLE [Vehicles] DROP CONSTRAINT [' + @var2 + '];');
ALTER TABLE [Vehicles] DROP COLUMN [CarModel];

ALTER TABLE [Vehicles] ADD [BrandId] int NOT NULL DEFAULT 0;

ALTER TABLE [Vehicles] ADD [CarModelId] int NOT NULL DEFAULT 0;

CREATE TABLE [Brands] (
    [Id] int NOT NULL IDENTITY,
    [Name] nvarchar(50) NOT NULL,
    CONSTRAINT [PK_Brands] PRIMARY KEY ([Id])
);

CREATE TABLE [CarModels] (
    [Id] int NOT NULL IDENTITY,
    [Name] nvarchar(50) NOT NULL,
    [BrandId] int NOT NULL,
    CONSTRAINT [PK_CarModels] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_CarModels_Brands_BrandId] FOREIGN KEY ([BrandId]) REFERENCES [Brands] ([Id]) ON DELETE NO ACTION
);

CREATE INDEX [IX_Vehicles_BrandId] ON [Vehicles] ([BrandId]);

CREATE INDEX [IX_Vehicles_CarModelId] ON [Vehicles] ([CarModelId]);

CREATE INDEX [IX_CarModels_BrandId] ON [CarModels] ([BrandId]);

ALTER TABLE [Vehicles] ADD CONSTRAINT [FK_Vehicles_Brands_BrandId] FOREIGN KEY ([BrandId]) REFERENCES [Brands] ([Id]) ON DELETE NO ACTION;

ALTER TABLE [Vehicles] ADD CONSTRAINT [FK_Vehicles_CarModels_CarModelId] FOREIGN KEY ([CarModelId]) REFERENCES [CarModels] ([Id]) ON DELETE NO ACTION;

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250616191843_AddCarBrandAndModel', N'9.0.5');

ALTER TABLE [AspNetRoleClaims] DROP CONSTRAINT [FK_AspNetRoleClaims_AspNetRoles_RoleId];

ALTER TABLE [AspNetUserClaims] DROP CONSTRAINT [FK_AspNetUserClaims_AspNetUsers_UserId];

ALTER TABLE [AspNetUserLogins] DROP CONSTRAINT [FK_AspNetUserLogins_AspNetUsers_UserId];

ALTER TABLE [AspNetUserRoles] DROP CONSTRAINT [FK_AspNetUserRoles_AspNetRoles_RoleId];

ALTER TABLE [AspNetUserRoles] DROP CONSTRAINT [FK_AspNetUserRoles_AspNetUsers_UserId];

ALTER TABLE [AspNetUserTokens] DROP CONSTRAINT [FK_AspNetUserTokens_AspNetUsers_UserId];

ALTER TABLE [AspNetRoleClaims] ADD CONSTRAINT [FK_AspNetRoleClaims_AspNetRoles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [AspNetRoles] ([Id]) ON DELETE NO ACTION;

ALTER TABLE [AspNetUserClaims] ADD CONSTRAINT [FK_AspNetUserClaims_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE NO ACTION;

ALTER TABLE [AspNetUserLogins] ADD CONSTRAINT [FK_AspNetUserLogins_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE NO ACTION;

ALTER TABLE [AspNetUserRoles] ADD CONSTRAINT [FK_AspNetUserRoles_AspNetRoles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [AspNetRoles] ([Id]) ON DELETE NO ACTION;

ALTER TABLE [AspNetUserRoles] ADD CONSTRAINT [FK_AspNetUserRoles_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE NO ACTION;

ALTER TABLE [AspNetUserTokens] ADD CONSTRAINT [FK_AspNetUserTokens_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE NO ACTION;

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250617110447_ConfigureCascadeDeleteForMechanicRelations', N'9.0.5');

ALTER TABLE [Vehicles] DROP CONSTRAINT [FK_Vehicles_Brands_BrandId];

DROP INDEX [IX_Vehicles_BrandId] ON [Vehicles];

DECLARE @var3 sysname;
SELECT @var3 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Vehicles]') AND [c].[name] = N'BrandId');
IF @var3 IS NOT NULL EXEC(N'ALTER TABLE [Vehicles] DROP CONSTRAINT [' + @var3 + '];');
ALTER TABLE [Vehicles] DROP COLUMN [BrandId];

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250617122350_RefactorVehicleWithBrandAndModel', N'9.0.5');

CREATE TABLE [Parts] (
    [Id] int NOT NULL IDENTITY,
    [Name] nvarchar(100) NOT NULL,
    [Description] nvarchar(250) NOT NULL,
    [StockQuantity] int NOT NULL,
    [Price] decimal(10,2) NOT NULL,
    CONSTRAINT [PK_Parts] PRIMARY KEY ([Id])
);

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250618163512_AddPartsTable', N'9.0.5');

DECLARE @var4 sysname;
SELECT @var4 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Repairs]') AND [c].[name] = N'Description');
IF @var4 IS NOT NULL EXEC(N'ALTER TABLE [Repairs] DROP CONSTRAINT [' + @var4 + '];');
ALTER TABLE [Repairs] ALTER COLUMN [Description] nvarchar(500) NOT NULL;

ALTER TABLE [Repairs] ADD [AppointmentId] int NULL;

ALTER TABLE [Appointments] ADD [RepairId] int NULL;

CREATE TABLE [RepairParts] (
    [RepairId] int NOT NULL,
    [PartId] int NOT NULL,
    [Id] int NOT NULL,
    [Quantity] int NOT NULL,
    [UnitPrice] decimal(10,2) NOT NULL,
    CONSTRAINT [PK_RepairParts] PRIMARY KEY ([RepairId], [PartId]),
    CONSTRAINT [FK_RepairParts_Parts_PartId] FOREIGN KEY ([PartId]) REFERENCES [Parts] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_RepairParts_Repairs_RepairId] FOREIGN KEY ([RepairId]) REFERENCES [Repairs] ([Id]) ON DELETE NO ACTION
);

CREATE UNIQUE INDEX [IX_Repairs_AppointmentId] ON [Repairs] ([AppointmentId]) WHERE [AppointmentId] IS NOT NULL;

CREATE INDEX [IX_RepairParts_PartId] ON [RepairParts] ([PartId]);

ALTER TABLE [Repairs] ADD CONSTRAINT [FK_Repairs_Appointments_AppointmentId] FOREIGN KEY ([AppointmentId]) REFERENCES [Appointments] ([Id]) ON DELETE SET NULL;

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250618173703_AddRepairPartsAndRelations', N'9.0.5');

ALTER TABLE [Vehicles] ADD [Mileage] int NOT NULL DEFAULT 0;

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250623121517_AddMileageToVehicle', N'9.0.5');

CREATE TABLE [Invoices] (
    [Id] int NOT NULL IDENTITY,
    [RepairId] int NOT NULL,
    [InvoiceDate] datetime2 NOT NULL,
    [TotalAmount] decimal(10,2) NOT NULL,
    [Status] nvarchar(20) NOT NULL,
    CONSTRAINT [PK_Invoices] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Invoices_Repairs_RepairId] FOREIGN KEY ([RepairId]) REFERENCES [Repairs] ([Id]) ON DELETE NO ACTION
);

CREATE TABLE [InvoiceItems] (
    [Id] int NOT NULL IDENTITY,
    [InvoiceId] int NOT NULL,
    [Description] nvarchar(200) NOT NULL,
    [Quantity] int NOT NULL,
    [UnitPrice] decimal(10,2) NOT NULL,
    CONSTRAINT [PK_InvoiceItems] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_InvoiceItems_Invoices_InvoiceId] FOREIGN KEY ([InvoiceId]) REFERENCES [Invoices] ([Id]) ON DELETE NO ACTION
);

CREATE INDEX [IX_InvoiceItems_InvoiceId] ON [InvoiceItems] ([InvoiceId]);

CREATE INDEX [IX_Invoices_RepairId] ON [Invoices] ([RepairId]);

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250625094837_AddInvoicingModule', N'9.0.5');

ALTER TABLE [Invoices] ADD [Subtotal] decimal(10,2) NOT NULL DEFAULT 0.0;

ALTER TABLE [Invoices] ADD [TaxAmount] decimal(10,2) NOT NULL DEFAULT 0.0;

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250625100527_UpdateInvoiceSchemaWithTax', N'9.0.5');

COMMIT;
GO

