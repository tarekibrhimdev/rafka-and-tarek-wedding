/*
  Wedding invitation — SQL Server schema + admin seed
  =====================================================

  Prerequisites:
  1. Create an empty database (example name below).
  2. In SSMS / Azure Data Studio: select that database, then execute this entire script.

  Regenerate the schema section after model changes:
    dotnet ef migrations script ^
      --project src/WeddingInvitation/WeddingInvitation.csproj ^
      --configuration Release ^
      --output scripts/_schema-from-ef.sql
    (merge the transaction body into this file or replace lines between SCHEMA START / END)

  Default admin (if using normal Identity hasher — not plaintext):
    UserName: admin@local.test
    Password: ChangeMe!123

  Plaintext username login + DB reset: see scripts/reset-admin-plain-username.sql and PlainTextPasswordHasher.

  To use a different password with normal hashing, run locally:
    dotnet run --project tools/PwdHash/PwdHash.csproj -- "YourPassword!"
  Paste the printed hash into [PasswordHash] in the INSERT below.

  After deployment, the app will NOT apply EF migrations automatically (see DbInitializer).
*/

SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

/* Uncomment and set your database name */
-- CREATE DATABASE [WeddingInvitation];
-- GO
-- USE [WeddingInvitation];
-- GO

IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

/* ========== SCHEMA START (EF InitialCreate) ========== */

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
    [IsRemoved] bit NOT NULL,
    [CreatedAtUtc] datetime2 NOT NULL,
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

CREATE TABLE [Guests] (
    [Id] uniqueidentifier NOT NULL,
    [DisplayName] nvarchar(200) NOT NULL,
    [Email] nvarchar(256) NULL,
    [Phone] nvarchar(64) NULL,
    [Notes] nvarchar(2000) NULL,
    [CreatedAtUtc] datetime2 NOT NULL,
    [UpdatedAtUtc] datetime2 NULL,
    [IsRemoved] bit NOT NULL,
    CONSTRAINT [PK_Guests] PRIMARY KEY ([Id])
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

CREATE TABLE [GuestFamilyMembers] (
    [Id] uniqueidentifier NOT NULL,
    [GuestId] uniqueidentifier NOT NULL,
    [FullName] nvarchar(200) NOT NULL,
    [SortOrder] int NOT NULL,
    [CreatedAtUtc] datetime2 NOT NULL,
    [UpdatedAtUtc] datetime2 NULL,
    [IsRemoved] bit NOT NULL,
    CONSTRAINT [PK_GuestFamilyMembers] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_GuestFamilyMembers_Guests_GuestId] FOREIGN KEY ([GuestId]) REFERENCES [Guests] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [Invitations] (
    [Id] uniqueidentifier NOT NULL,
    [GuestId] uniqueidentifier NOT NULL,
    [Token] nvarchar(128) NOT NULL,
    [MaxPersons] int NOT NULL,
    [RsvpStatus] int NOT NULL,
    [ComingCount] int NULL,
    [RespondedAtUtc] datetime2 NULL,
    [CreatedAtUtc] datetime2 NOT NULL,
    [UpdatedAtUtc] datetime2 NULL,
    [IsRemoved] bit NOT NULL,
    CONSTRAINT [PK_Invitations] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Invitations_Guests_GuestId] FOREIGN KEY ([GuestId]) REFERENCES [Guests] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [InvitationAuditEntries] (
    [Id] uniqueidentifier NOT NULL,
    [InvitationId] uniqueidentifier NOT NULL,
    [EventType] int NOT NULL,
    [Details] nvarchar(4000) NULL,
    [CreatedAtUtc] datetime2 NOT NULL,
    [UpdatedAtUtc] datetime2 NULL,
    [IsRemoved] bit NOT NULL,
    CONSTRAINT [PK_InvitationAuditEntries] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_InvitationAuditEntries_Invitations_InvitationId] FOREIGN KEY ([InvitationId]) REFERENCES [Invitations] ([Id]) ON DELETE CASCADE
);

CREATE INDEX [IX_AspNetRoleClaims_RoleId] ON [AspNetRoleClaims] ([RoleId]);

CREATE UNIQUE INDEX [RoleNameIndex] ON [AspNetRoles] ([NormalizedName]) WHERE [NormalizedName] IS NOT NULL;

CREATE INDEX [IX_AspNetUserClaims_UserId] ON [AspNetUserClaims] ([UserId]);

CREATE INDEX [IX_AspNetUserLogins_UserId] ON [AspNetUserLogins] ([UserId]);

CREATE INDEX [IX_AspNetUserRoles_RoleId] ON [AspNetUserRoles] ([RoleId]);

CREATE INDEX [EmailIndex] ON [AspNetUsers] ([NormalizedEmail]);

CREATE UNIQUE INDEX [UserNameIndex] ON [AspNetUsers] ([NormalizedUserName]) WHERE [NormalizedUserName] IS NOT NULL;

CREATE INDEX [IX_GuestFamilyMembers_GuestId] ON [GuestFamilyMembers] ([GuestId]);

CREATE INDEX [IX_InvitationAuditEntries_InvitationId] ON [InvitationAuditEntries] ([InvitationId]);

CREATE UNIQUE INDEX [IX_Invitations_GuestId] ON [Invitations] ([GuestId]) WHERE IsRemoved = 0;

CREATE UNIQUE INDEX [IX_Invitations_Token] ON [Invitations] ([Token]);

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260508115158_InitialCreate', N'10.0.0');

COMMIT;
GO

/* ========== SCHEMA END ========== */

/* Seed: single admin (ASP.NET Core Identity password hash for ChangeMe!123) */
IF NOT EXISTS (SELECT 1 FROM [AspNetUsers] WHERE [NormalizedEmail] = N'ADMIN@LOCAL.TEST')
BEGIN
    INSERT INTO [AspNetUsers] (
        [Id],
        [IsRemoved],
        [CreatedAtUtc],
        [UserName],
        [NormalizedUserName],
        [Email],
        [NormalizedEmail],
        [EmailConfirmed],
        [PasswordHash],
        [SecurityStamp],
        [ConcurrencyStamp],
        [PhoneNumber],
        [PhoneNumberConfirmed],
        [TwoFactorEnabled],
        [LockoutEnd],
        [LockoutEnabled],
        [AccessFailedCount]
    )
    VALUES (
        N'a8f5f167-f446-409d-82bf-a4e5f64879ac',
        0,
        SYSUTCDATETIME(),
        N'admin@local.test',
        N'ADMIN@LOCAL.TEST',
        N'admin@local.test',
        N'ADMIN@LOCAL.TEST',
        1,
        N'AQAAAAIAAYagAAAAEE8EllJNTyGD6VH/Vl+qYZsCJzttBnsG2aJA06UtXMFFyr3uJov/zwRmSPF/Jx4ZJw==',
        N'8f4c9e2b7d6a41f2b3c5d8e9f0a1b2c3',
        N'9d5e0f3c8b7a4926c4d7e8f9a0b1c2d3',
        NULL,
        0,
        0,
        NULL,
        1,
        0
    );
END;
GO
