-- ============================================
-- Enhanced Idempotent Migration Script
-- Handles both fresh installations and updates
-- ============================================

-- Create Migration History Table if it doesn't exist
IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

-- ============================================
-- Migration: 20250521135359_InitialCreate
-- ============================================
BEGIN TRANSACTION;
GO

-- Create AppNeoForm Schema
IF SCHEMA_ID(N'AppNeoForm') IS NULL 
BEGIN
    EXEC(N'CREATE SCHEMA [AppNeoForm];');
    PRINT 'Created schema: AppNeoForm';
END;
GO

-- Create Clients Table
IF OBJECT_ID(N'[AppNeoForm].[Clients]', 'U') IS NULL
BEGIN
    CREATE TABLE [AppNeoForm].[Clients] (
        [Id] int NOT NULL IDENTITY,
        [ClientId] nvarchar(100) NOT NULL,
        [BaseUrl] nvarchar(300) NOT NULL,
        CONSTRAINT [PK_Clients] PRIMARY KEY ([Id])
    );
    PRINT 'Created table: AppNeoForm.Clients';
END
ELSE
BEGIN
    PRINT 'Table AppNeoForm.Clients already exists - skipping creation';
END;
GO

-- Create Object Table
IF OBJECT_ID(N'[AppNeoForm].[Object]', 'U') IS NULL
BEGIN
    CREATE TABLE [AppNeoForm].[Object] (
        [_id] int NOT NULL IDENTITY,
        [guid] AS (left(json_value([ObjectJson],'$.guid'),(100))),
        [application] AS (left(json_value([ObjectJson],'$.application'),(100))),
        [objectName] AS (left(json_value([ObjectJson],'$.objectName'),(100))),
        [objectType] AS (left(json_value([ObjectJson],'$.objectType'),(100))),
        [isEncrypted] AS CASE WHEN JSON_VALUE([ObjectJson], '$.isEncrypted') = 'true' THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT) END,
        [ObjectJson] nvarchar(max) NOT NULL,
        CONSTRAINT [PK_Object] PRIMARY KEY ([_id])
    );
    PRINT 'Created table: AppNeoForm.Object';
END
ELSE
BEGIN
    PRINT 'Table AppNeoForm.Object already exists - skipping creation';
END;
GO

IF NOT EXISTS (SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20250521135359_InitialCreate')
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20250521135359_InitialCreate', N'8.0.16');
    PRINT 'Recorded migration: 20250521135359_InitialCreate';
END;
GO

COMMIT;
GO

-- ============================================
-- Migration: 20250708092252_AddApiKeyToClients
-- ============================================
BEGIN TRANSACTION;
GO

-- Add ApiKey column to Clients table if it doesn't exist
IF OBJECT_ID(N'[AppNeoForm].[Clients]', 'U') IS NOT NULL 
   AND COL_LENGTH(N'[AppNeoForm].[Clients]', N'ApiKey') IS NULL
BEGIN
    ALTER TABLE [AppNeoForm].[Clients] ADD [ApiKey] nvarchar(max) NOT NULL DEFAULT N'';
    PRINT 'Added column: ApiKey to AppNeoForm.Clients';
END
ELSE IF COL_LENGTH(N'[AppNeoForm].[Clients]', N'ApiKey') IS NOT NULL
BEGIN
    PRINT 'Column ApiKey already exists in AppNeoForm.Clients - skipping';
END;
GO

IF NOT EXISTS (SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20250708092252_AddApiKeyToClients')
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20250708092252_AddApiKeyToClients', N'8.0.16');
    PRINT 'Recorded migration: 20250708092252_AddApiKeyToClients';
END;
GO

COMMIT;
GO

-- ============================================
-- Migration: 20250710102245_AddOidcAuthentication
-- ============================================
BEGIN TRANSACTION;
GO

-- Create UserAuthentications Table
IF OBJECT_ID(N'[UserAuthentications]', 'U') IS NULL
BEGIN
    CREATE TABLE [UserAuthentications] (
        [Id] nvarchar(450) NOT NULL,
        [Guid] nvarchar(450) NOT NULL,
        [UserId] nvarchar(max) NOT NULL,
        [UserEmail] nvarchar(max) NOT NULL,
        [AccessToken] nvarchar(max) NOT NULL,
        [IdToken] nvarchar(max) NOT NULL,
        [RefreshToken] nvarchar(max) NOT NULL,
        [ExpiresAt] datetime2 NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [IsActive] bit NOT NULL,
        CONSTRAINT [PK_UserAuthentications] PRIMARY KEY ([Id])
    );
    PRINT 'Created table: UserAuthentications';
    
    -- Create indexes
    CREATE INDEX [IX_UserAuthentications_Guid] ON [UserAuthentications] ([Guid]);
    CREATE INDEX [IX_UserAuthentications_Guid_IsActive] ON [UserAuthentications] ([Guid], [IsActive]);
    PRINT 'Created indexes on UserAuthentications';
END
ELSE
BEGIN
    PRINT 'Table UserAuthentications already exists - skipping creation';
    
    -- Ensure indexes exist
    IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_UserAuthentications_Guid' AND object_id = OBJECT_ID('UserAuthentications'))
    BEGIN
        CREATE INDEX [IX_UserAuthentications_Guid] ON [UserAuthentications] ([Guid]);
        PRINT 'Created missing index: IX_UserAuthentications_Guid';
    END;
    
    IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_UserAuthentications_Guid_IsActive' AND object_id = OBJECT_ID('UserAuthentications'))
    BEGIN
        CREATE INDEX [IX_UserAuthentications_Guid_IsActive] ON [UserAuthentications] ([Guid], [IsActive]);
        PRINT 'Created missing index: IX_UserAuthentications_Guid_IsActive';
    END;
END;
GO

IF NOT EXISTS (SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20250710102245_AddOidcAuthentication')
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20250710102245_AddOidcAuthentication', N'8.0.16');
    PRINT 'Recorded migration: 20250710102245_AddOidcAuthentication';
END;
GO

COMMIT;
GO

-- ============================================
-- Migration: 20250710125945_MakeRefreshTokenNullable
-- ============================================
BEGIN TRANSACTION;
GO

-- Make RefreshToken nullable
IF OBJECT_ID(N'[UserAuthentications]', 'U') IS NOT NULL
   AND COL_LENGTH(N'[UserAuthentications]', N'RefreshToken') IS NOT NULL
BEGIN
    DECLARE @var0 sysname;
    SELECT @var0 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[UserAuthentications]') AND [c].[name] = N'RefreshToken');
    IF @var0 IS NOT NULL EXEC(N'ALTER TABLE [UserAuthentications] DROP CONSTRAINT [' + @var0 + '];');
    
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('UserAuthentications') AND name = 'RefreshToken' AND is_nullable = 1)
    BEGIN
        ALTER TABLE [UserAuthentications] ALTER COLUMN [RefreshToken] nvarchar(max) NULL;
        PRINT 'Modified column: RefreshToken to nullable in UserAuthentications';
    END
    ELSE
    BEGIN
        PRINT 'Column RefreshToken is already nullable - skipping';
    END;
END;
GO

-- Make IdToken nullable
IF OBJECT_ID(N'[UserAuthentications]', 'U') IS NOT NULL
   AND COL_LENGTH(N'[UserAuthentications]', N'IdToken') IS NOT NULL
BEGIN
    DECLARE @var1 sysname;
    SELECT @var1 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[UserAuthentications]') AND [c].[name] = N'IdToken');
    IF @var1 IS NOT NULL EXEC(N'ALTER TABLE [UserAuthentications] DROP CONSTRAINT [' + @var1 + '];');
    
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('UserAuthentications') AND name = 'IdToken' AND is_nullable = 1)
    BEGIN
        ALTER TABLE [UserAuthentications] ALTER COLUMN [IdToken] nvarchar(max) NULL;
        PRINT 'Modified column: IdToken to nullable in UserAuthentications';
    END
    ELSE
    BEGIN
        PRINT 'Column IdToken is already nullable - skipping';
    END;
END;
GO

IF NOT EXISTS (SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20250710125945_MakeRefreshTokenNullable')
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20250710125945_MakeRefreshTokenNullable', N'8.0.16');
    PRINT 'Recorded migration: 20250710125945_MakeRefreshTokenNullable';
END;
GO

COMMIT;
GO

-- ============================================
-- Migration: 20250710130730_FixUserAuthenticationNullableFieldsAndDefaults
-- ============================================
BEGIN TRANSACTION;
GO

-- Make UserEmail nullable
IF OBJECT_ID(N'[UserAuthentications]', 'U') IS NOT NULL
   AND COL_LENGTH(N'[UserAuthentications]', N'UserEmail') IS NOT NULL
BEGIN
    DECLARE @var2 sysname;
    SELECT @var2 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[UserAuthentications]') AND [c].[name] = N'UserEmail');
    IF @var2 IS NOT NULL EXEC(N'ALTER TABLE [UserAuthentications] DROP CONSTRAINT [' + @var2 + '];');
    
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('UserAuthentications') AND name = 'UserEmail' AND is_nullable = 1)
    BEGIN
        ALTER TABLE [UserAuthentications] ALTER COLUMN [UserEmail] nvarchar(max) NULL;
        PRINT 'Modified column: UserEmail to nullable in UserAuthentications';
    END
    ELSE
    BEGIN
        PRINT 'Column UserEmail is already nullable - skipping';
    END;
END;
GO

IF NOT EXISTS (SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20250710130730_FixUserAuthenticationNullableFieldsAndDefaults')
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20250710130730_FixUserAuthenticationNullableFieldsAndDefaults', N'8.0.16');
    PRINT 'Recorded migration: 20250710130730_FixUserAuthenticationNullableFieldsAndDefaults';
END;
GO

COMMIT;
GO

-- ============================================
-- Migration: 20251112092543_AddIdentityTables
-- ============================================
BEGIN TRANSACTION;
GO

-- Update ApiKey column size in Clients table
IF OBJECT_ID(N'[AppNeoForm].[Clients]', 'U') IS NOT NULL
   AND COL_LENGTH(N'[AppNeoForm].[Clients]', N'ApiKey') IS NOT NULL
BEGIN
    DECLARE @currentType nvarchar(128);
    SELECT @currentType = t.name + '(' + CAST(c.max_length AS nvarchar) + ')'
    FROM sys.columns c
    INNER JOIN sys.types t ON c.user_type_id = t.user_type_id
    WHERE c.object_id = OBJECT_ID('[AppNeoForm].[Clients]') AND c.name = 'ApiKey';
    
    IF @currentType != 'nvarchar(1000)'  -- 500 * 2 bytes for nvarchar
    BEGIN
        DECLARE @var3 sysname;
        SELECT @var3 = [d].[name]
        FROM [sys].[default_constraints] [d]
        INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
        WHERE ([d].[parent_object_id] = OBJECT_ID(N'[AppNeoForm].[Clients]') AND [c].[name] = N'ApiKey');
        IF @var3 IS NOT NULL EXEC(N'ALTER TABLE [AppNeoForm].[Clients] DROP CONSTRAINT [' + @var3 + '];');
        
        ALTER TABLE [AppNeoForm].[Clients] ALTER COLUMN [ApiKey] nvarchar(500) NOT NULL;
        PRINT 'Modified column: ApiKey size to nvarchar(500) in AppNeoForm.Clients';
    END
    ELSE
    BEGIN
        PRINT 'Column ApiKey already has correct size - skipping';
    END;
END;
GO

-- Create AspNetRoles Table
IF OBJECT_ID(N'[AspNetRoles]', 'U') IS NULL
BEGIN
    CREATE TABLE [AspNetRoles] (
        [Id] nvarchar(450) NOT NULL,
        [Name] nvarchar(256) NULL,
        [NormalizedName] nvarchar(256) NULL,
        [ConcurrencyStamp] nvarchar(max) NULL,
        CONSTRAINT [PK_AspNetRoles] PRIMARY KEY ([Id])
    );
    PRINT 'Created table: AspNetRoles';
    
    EXEC(N'CREATE UNIQUE INDEX [RoleNameIndex] ON [AspNetRoles] ([NormalizedName]) WHERE [NormalizedName] IS NOT NULL');
    PRINT 'Created index: RoleNameIndex on AspNetRoles';
END
ELSE
BEGIN
    PRINT 'Table AspNetRoles already exists - skipping creation';
END;
GO

-- Create AspNetUsers Table
IF OBJECT_ID(N'[AspNetUsers]', 'U') IS NULL
BEGIN
    CREATE TABLE [AspNetUsers] (
        [Id] nvarchar(450) NOT NULL,
        [FullName] nvarchar(max) NULL,
        [CreatedAt] datetime2 NOT NULL,
        [CreatedBy] nvarchar(max) NULL,
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
    PRINT 'Created table: AspNetUsers';
    
    CREATE INDEX [EmailIndex] ON [AspNetUsers] ([NormalizedEmail]);
    EXEC(N'CREATE UNIQUE INDEX [UserNameIndex] ON [AspNetUsers] ([NormalizedUserName]) WHERE [NormalizedUserName] IS NOT NULL');
    PRINT 'Created indexes on AspNetUsers';
END
ELSE
BEGIN
    PRINT 'Table AspNetUsers already exists - skipping creation';
    
    -- Add missing columns if they don't exist
    IF COL_LENGTH(N'[AspNetUsers]', N'FullName') IS NULL
    BEGIN
        ALTER TABLE [AspNetUsers] ADD [FullName] nvarchar(max) NULL;
        PRINT 'Added column: FullName to AspNetUsers';
    END;
    
    IF COL_LENGTH(N'[AspNetUsers]', N'CreatedAt') IS NULL
    BEGIN
        ALTER TABLE [AspNetUsers] ADD [CreatedAt] datetime2 NOT NULL DEFAULT GETDATE();
        PRINT 'Added column: CreatedAt to AspNetUsers';
    END;
    
    IF COL_LENGTH(N'[AspNetUsers]', N'CreatedBy') IS NULL
    BEGIN
        ALTER TABLE [AspNetUsers] ADD [CreatedBy] nvarchar(max) NULL;
        PRINT 'Added column: CreatedBy to AspNetUsers';
    END;
END;
GO

-- Create AspNetRoleClaims Table
IF OBJECT_ID(N'[AspNetRoleClaims]', 'U') IS NULL
BEGIN
    CREATE TABLE [AspNetRoleClaims] (
        [Id] int NOT NULL IDENTITY,
        [RoleId] nvarchar(450) NOT NULL,
        [ClaimType] nvarchar(max) NULL,
        [ClaimValue] nvarchar(max) NULL,
        CONSTRAINT [PK_AspNetRoleClaims] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_AspNetRoleClaims_AspNetRoles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [AspNetRoles] ([Id]) ON DELETE CASCADE
    );
    CREATE INDEX [IX_AspNetRoleClaims_RoleId] ON [AspNetRoleClaims] ([RoleId]);
    PRINT 'Created table: AspNetRoleClaims';
END
ELSE
BEGIN
    PRINT 'Table AspNetRoleClaims already exists - skipping creation';
END;
GO

-- Create AspNetUserClaims Table
IF OBJECT_ID(N'[AspNetUserClaims]', 'U') IS NULL
BEGIN
    CREATE TABLE [AspNetUserClaims] (
        [Id] int NOT NULL IDENTITY,
        [UserId] nvarchar(450) NOT NULL,
        [ClaimType] nvarchar(max) NULL,
        [ClaimValue] nvarchar(max) NULL,
        CONSTRAINT [PK_AspNetUserClaims] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_AspNetUserClaims_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
    );
    CREATE INDEX [IX_AspNetUserClaims_UserId] ON [AspNetUserClaims] ([UserId]);
    PRINT 'Created table: AspNetUserClaims';
END
ELSE
BEGIN
    PRINT 'Table AspNetUserClaims already exists - skipping creation';
END;
GO

-- Create AspNetUserLogins Table
IF OBJECT_ID(N'[AspNetUserLogins]', 'U') IS NULL
BEGIN
    CREATE TABLE [AspNetUserLogins] (
        [LoginProvider] nvarchar(450) NOT NULL,
        [ProviderKey] nvarchar(450) NOT NULL,
        [ProviderDisplayName] nvarchar(max) NULL,
        [UserId] nvarchar(450) NOT NULL,
        CONSTRAINT [PK_AspNetUserLogins] PRIMARY KEY ([LoginProvider], [ProviderKey]),
        CONSTRAINT [FK_AspNetUserLogins_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
    );
    CREATE INDEX [IX_AspNetUserLogins_UserId] ON [AspNetUserLogins] ([UserId]);
    PRINT 'Created table: AspNetUserLogins';
END
ELSE
BEGIN
    PRINT 'Table AspNetUserLogins already exists - skipping creation';
END;
GO

-- Create AspNetUserRoles Table
IF OBJECT_ID(N'[AspNetUserRoles]', 'U') IS NULL
BEGIN
    CREATE TABLE [AspNetUserRoles] (
        [UserId] nvarchar(450) NOT NULL,
        [RoleId] nvarchar(450) NOT NULL,
        CONSTRAINT [PK_AspNetUserRoles] PRIMARY KEY ([UserId], [RoleId]),
        CONSTRAINT [FK_AspNetUserRoles_AspNetRoles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [AspNetRoles] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_AspNetUserRoles_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
    );
    CREATE INDEX [IX_AspNetUserRoles_RoleId] ON [AspNetUserRoles] ([RoleId]);
    PRINT 'Created table: AspNetUserRoles';
END
ELSE
BEGIN
    PRINT 'Table AspNetUserRoles already exists - skipping creation';
END;
GO

-- Create AspNetUserTokens Table
IF OBJECT_ID(N'[AspNetUserTokens]', 'U') IS NULL
BEGIN
    CREATE TABLE [AspNetUserTokens] (
        [UserId] nvarchar(450) NOT NULL,
        [LoginProvider] nvarchar(450) NOT NULL,
        [Name] nvarchar(450) NOT NULL,
        [Value] nvarchar(max) NULL,
        CONSTRAINT [PK_AspNetUserTokens] PRIMARY KEY ([UserId], [LoginProvider], [Name]),
        CONSTRAINT [FK_AspNetUserTokens_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
    );
    PRINT 'Created table: AspNetUserTokens';
END
ELSE
BEGIN
    PRINT 'Table AspNetUserTokens already exists - skipping creation';
END;
GO

IF NOT EXISTS (SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20251112092543_AddIdentityTables')
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20251112092543_AddIdentityTables', N'8.0.16');
    PRINT 'Recorded migration: 20251112092543_AddIdentityTables';
END;
GO

COMMIT;
GO

-- ============================================
-- Migration: 20251112093839_AddMustChangePasswordField
-- ============================================
BEGIN TRANSACTION;
GO

-- Add MustChangePassword column to AspNetUsers if it doesn't exist
IF OBJECT_ID(N'[AspNetUsers]', 'U') IS NOT NULL
   AND COL_LENGTH(N'[AspNetUsers]', N'MustChangePassword') IS NULL
BEGIN
    ALTER TABLE [AspNetUsers] ADD [MustChangePassword] bit NOT NULL DEFAULT CAST(0 AS bit);
    PRINT 'Added column: MustChangePassword to AspNetUsers';
END
ELSE IF COL_LENGTH(N'[AspNetUsers]', N'MustChangePassword') IS NOT NULL
BEGIN
    PRINT 'Column MustChangePassword already exists in AspNetUsers - skipping';
END;
GO

IF NOT EXISTS (SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20251112093839_AddMustChangePasswordField')
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20251112093839_AddMustChangePasswordField', N'8.0.16');
    PRINT 'Recorded migration: 20251112093839_AddMustChangePasswordField';
END;
GO

COMMIT;
GO

-- ============================================
-- Migration: 20251124112829_UpdateSchemaToAppNeoFormExt
-- ============================================
BEGIN TRANSACTION;
GO

-- Create AppNeoFormExt Schema
IF SCHEMA_ID(N'AppNeoFormExt') IS NULL 
BEGIN
    EXEC(N'CREATE SCHEMA [AppNeoFormExt];');
    PRINT 'Created schema: AppNeoFormExt';
END;
GO

-- Transfer UserAuthentications to AppNeoFormExt schema
IF OBJECT_ID(N'[UserAuthentications]', 'U') IS NOT NULL
   AND OBJECT_ID(N'[AppNeoFormExt].[UserAuthentications]', 'U') IS NULL
BEGIN
    ALTER SCHEMA [AppNeoFormExt] TRANSFER [UserAuthentications];
    PRINT 'Transferred UserAuthentications to AppNeoFormExt schema';
END
ELSE IF OBJECT_ID(N'[AppNeoFormExt].[UserAuthentications]', 'U') IS NOT NULL
BEGIN
    PRINT 'UserAuthentications already in AppNeoFormExt schema - skipping';
END;
GO

-- Transfer Object to AppNeoFormExt schema
IF OBJECT_ID(N'[AppNeoForm].[Object]', 'U') IS NOT NULL
   AND OBJECT_ID(N'[AppNeoFormExt].[Object]', 'U') IS NULL
BEGIN
    ALTER SCHEMA [AppNeoFormExt] TRANSFER [AppNeoForm].[Object];
    PRINT 'Transferred Object to AppNeoFormExt schema';
END
ELSE IF OBJECT_ID(N'[AppNeoFormExt].[Object]', 'U') IS NOT NULL
BEGIN
    PRINT 'Object already in AppNeoFormExt schema - skipping';
END;
GO

-- Transfer Clients to AppNeoFormExt schema
IF OBJECT_ID(N'[AppNeoForm].[Clients]', 'U') IS NOT NULL
   AND OBJECT_ID(N'[AppNeoFormExt].[Clients]', 'U') IS NULL
BEGIN
    ALTER SCHEMA [AppNeoFormExt] TRANSFER [AppNeoForm].[Clients];
    PRINT 'Transferred Clients to AppNeoFormExt schema';
END
ELSE IF OBJECT_ID(N'[AppNeoFormExt].[Clients]', 'U') IS NOT NULL
BEGIN
    PRINT 'Clients already in AppNeoFormExt schema - skipping';
END;
GO

-- Transfer AspNetUserTokens to AppNeoFormExt schema
IF OBJECT_ID(N'[AspNetUserTokens]', 'U') IS NOT NULL
   AND OBJECT_ID(N'[AppNeoFormExt].[AspNetUserTokens]', 'U') IS NULL
BEGIN
    ALTER SCHEMA [AppNeoFormExt] TRANSFER [AspNetUserTokens];
    PRINT 'Transferred AspNetUserTokens to AppNeoFormExt schema';
END
ELSE IF OBJECT_ID(N'[AppNeoFormExt].[AspNetUserTokens]', 'U') IS NOT NULL
BEGIN
    PRINT 'AspNetUserTokens already in AppNeoFormExt schema - skipping';
END;
GO

-- Transfer AspNetUsers to AppNeoFormExt schema
IF OBJECT_ID(N'[AspNetUsers]', 'U') IS NOT NULL
   AND OBJECT_ID(N'[AppNeoFormExt].[AspNetUsers]', 'U') IS NULL
BEGIN
    ALTER SCHEMA [AppNeoFormExt] TRANSFER [AspNetUsers];
    PRINT 'Transferred AspNetUsers to AppNeoFormExt schema';
END
ELSE IF OBJECT_ID(N'[AppNeoFormExt].[AspNetUsers]', 'U') IS NOT NULL
BEGIN
    PRINT 'AspNetUsers already in AppNeoFormExt schema - skipping';
END;
GO

-- Transfer AspNetUserRoles to AppNeoFormExt schema
IF OBJECT_ID(N'[AspNetUserRoles]', 'U') IS NOT NULL
   AND OBJECT_ID(N'[AppNeoFormExt].[AspNetUserRoles]', 'U') IS NULL
BEGIN
    ALTER SCHEMA [AppNeoFormExt] TRANSFER [AspNetUserRoles];
    PRINT 'Transferred AspNetUserRoles to AppNeoFormExt schema';
END
ELSE IF OBJECT_ID(N'[AppNeoFormExt].[AspNetUserRoles]', 'U') IS NOT NULL
BEGIN
    PRINT 'AspNetUserRoles already in AppNeoFormExt schema - skipping';
END;
GO

-- Transfer AspNetUserLogins to AppNeoFormExt schema
IF OBJECT_ID(N'[AspNetUserLogins]', 'U') IS NOT NULL
   AND OBJECT_ID(N'[AppNeoFormExt].[AspNetUserLogins]', 'U') IS NULL
BEGIN
    ALTER SCHEMA [AppNeoFormExt] TRANSFER [AspNetUserLogins];
    PRINT 'Transferred AspNetUserLogins to AppNeoFormExt schema';
END
ELSE IF OBJECT_ID(N'[AppNeoFormExt].[AspNetUserLogins]', 'U') IS NOT NULL
BEGIN
    PRINT 'AspNetUserLogins already in AppNeoFormExt schema - skipping';
END;
GO

-- Transfer AspNetUserClaims to AppNeoFormExt schema
IF OBJECT_ID(N'[AspNetUserClaims]', 'U') IS NOT NULL
   AND OBJECT_ID(N'[AppNeoFormExt].[AspNetUserClaims]', 'U') IS NULL
BEGIN
    ALTER SCHEMA [AppNeoFormExt] TRANSFER [AspNetUserClaims];
    PRINT 'Transferred AspNetUserClaims to AppNeoFormExt schema';
END
ELSE IF OBJECT_ID(N'[AppNeoFormExt].[AspNetUserClaims]', 'U') IS NOT NULL
BEGIN
    PRINT 'AspNetUserClaims already in AppNeoFormExt schema - skipping';
END;
GO

-- Transfer AspNetRoles to AppNeoFormExt schema
IF OBJECT_ID(N'[AspNetRoles]', 'U') IS NOT NULL
   AND OBJECT_ID(N'[AppNeoFormExt].[AspNetRoles]', 'U') IS NULL
BEGIN
    ALTER SCHEMA [AppNeoFormExt] TRANSFER [AspNetRoles];
    PRINT 'Transferred AspNetRoles to AppNeoFormExt schema';
END
ELSE IF OBJECT_ID(N'[AppNeoFormExt].[AspNetRoles]', 'U') IS NOT NULL
BEGIN
    PRINT 'AspNetRoles already in AppNeoFormExt schema - skipping';
END;
GO

-- Transfer AspNetRoleClaims to AppNeoFormExt schema
IF OBJECT_ID(N'[AspNetRoleClaims]', 'U') IS NOT NULL
   AND OBJECT_ID(N'[AppNeoFormExt].[AspNetRoleClaims]', 'U') IS NULL
BEGIN
    ALTER SCHEMA [AppNeoFormExt] TRANSFER [AspNetRoleClaims];
    PRINT 'Transferred AspNetRoleClaims to AppNeoFormExt schema';
END
ELSE IF OBJECT_ID(N'[AppNeoFormExt].[AspNetRoleClaims]', 'U') IS NOT NULL
BEGIN
    PRINT 'AspNetRoleClaims already in AppNeoFormExt schema - skipping';
END;
GO

IF NOT EXISTS (SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20251124112829_UpdateSchemaToAppNeoFormExt')
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20251124112829_UpdateSchemaToAppNeoFormExt', N'8.0.16');
    PRINT 'Recorded migration: 20251124112829_UpdateSchemaToAppNeoFormExt';
END;
GO

COMMIT;
GO

-- ============================================
-- Migration: 20251125141204_AddSessionAuditsTable
-- ============================================
BEGIN TRANSACTION;
GO

-- Create SessionAudits Table
IF OBJECT_ID(N'[AppNeoFormExt].[SessionAudits]', 'U') IS NULL
BEGIN
    CREATE TABLE [AppNeoFormExt].[SessionAudits] (
        [Id] int NOT NULL IDENTITY,
        [ClientId] nvarchar(100) NOT NULL,
        [Guid] uniqueidentifier NOT NULL,
        [PersonalCode] nvarchar(100) NOT NULL,
        [AuthType] nvarchar(50) NOT NULL,
        [Email] nvarchar(255) NULL,
        [OidcUserId] nvarchar(255) NULL,
        [TokenId] nvarchar(100) NULL,
        [CreatedAt] datetime2 NOT NULL,
        [ExpiresAt] datetime2 NULL,
        [IpAddress] nvarchar(50) NULL,
        [UserAgent] nvarchar(500) NULL,
        [Success] bit NOT NULL,
        [ErrorMessage] nvarchar(500) NULL,
        CONSTRAINT [PK_SessionAudits] PRIMARY KEY ([Id])
    );
    PRINT 'Created table: AppNeoFormExt.SessionAudits';
    
    -- Create indexes
    CREATE INDEX [IX_SessionAudits_ClientId] ON [AppNeoFormExt].[SessionAudits] ([ClientId]);
    CREATE INDEX [IX_SessionAudits_ClientId_CreatedAt] ON [AppNeoFormExt].[SessionAudits] ([ClientId], [CreatedAt]);
    CREATE INDEX [IX_SessionAudits_CreatedAt] ON [AppNeoFormExt].[SessionAudits] ([CreatedAt]);
    CREATE INDEX [IX_SessionAudits_Guid] ON [AppNeoFormExt].[SessionAudits] ([Guid]);
    PRINT 'Created indexes on SessionAudits';
END
ELSE
BEGIN
    PRINT 'Table AppNeoFormExt.SessionAudits already exists - skipping creation';
    
    -- Ensure indexes exist
    IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_SessionAudits_ClientId' AND object_id = OBJECT_ID('[AppNeoFormExt].[SessionAudits]'))
    BEGIN
        CREATE INDEX [IX_SessionAudits_ClientId] ON [AppNeoFormExt].[SessionAudits] ([ClientId]);
        PRINT 'Created missing index: IX_SessionAudits_ClientId';
    END;
    
    IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_SessionAudits_ClientId_CreatedAt' AND object_id = OBJECT_ID('[AppNeoFormExt].[SessionAudits]'))
    BEGIN
        CREATE INDEX [IX_SessionAudits_ClientId_CreatedAt] ON [AppNeoFormExt].[SessionAudits] ([ClientId], [CreatedAt]);
        PRINT 'Created missing index: IX_SessionAudits_ClientId_CreatedAt';
    END;
    
    IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_SessionAudits_CreatedAt' AND object_id = OBJECT_ID('[AppNeoFormExt].[SessionAudits]'))
    BEGIN
        CREATE INDEX [IX_SessionAudits_CreatedAt] ON [AppNeoFormExt].[SessionAudits] ([CreatedAt]);
        PRINT 'Created missing index: IX_SessionAudits_CreatedAt';
    END;
    
    IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_SessionAudits_Guid' AND object_id = OBJECT_ID('[AppNeoFormExt].[SessionAudits]'))
    BEGIN
        CREATE INDEX [IX_SessionAudits_Guid] ON [AppNeoFormExt].[SessionAudits] ([Guid]);
        PRINT 'Created missing index: IX_SessionAudits_Guid';
    END;
END;
GO

IF NOT EXISTS (SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20251125141204_AddSessionAuditsTable')
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20251125141204_AddSessionAuditsTable', N'8.0.16');
    PRINT 'Recorded migration: 20251125141204_AddSessionAuditsTable';
END;
GO

COMMIT;
GO

-- ============================================
-- Migration Complete
-- ============================================
PRINT '===========================================';
PRINT 'All migrations completed successfully!';
PRINT '===========================================';
GO

