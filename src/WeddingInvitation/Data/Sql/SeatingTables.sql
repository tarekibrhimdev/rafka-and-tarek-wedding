/*
  Reception seating — applies GuestTableAssignments + ReceptionTables.
  Run on SQL Server (production and any environment using this database).

  Notes:
  - Idempotent: safe to run once; skips objects that already exist.
  - Does not insert into __EFMigrationsHistory (schema managed outside EF migrations).
  - Matches WeddingInvitation.Data.WeddingDbContext (ReceptionTable, GuestTableAssignment).
*/

SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

IF OBJECT_ID(N'[dbo].[ReceptionTables]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[ReceptionTables] (
        [Id] UNIQUEIDENTIFIER NOT NULL CONSTRAINT [PK_ReceptionTables] PRIMARY KEY,
        [Name] NVARCHAR(120) NOT NULL,
        [Capacity] INT NOT NULL,
        [SortOrder] INT NOT NULL,
        [CreatedAtUtc] DATETIME2 NOT NULL,
        [UpdatedAtUtc] DATETIME2 NULL,
        [IsRemoved] BIT NOT NULL
    );
END
GO

IF OBJECT_ID(N'[dbo].[GuestTableAssignments]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[GuestTableAssignments] (
        [Id] UNIQUEIDENTIFIER NOT NULL CONSTRAINT [PK_GuestTableAssignments] PRIMARY KEY,
        [GuestId] UNIQUEIDENTIFIER NOT NULL,
        [ReceptionTableId] UNIQUEIDENTIFIER NOT NULL,
        [CreatedAtUtc] DATETIME2 NOT NULL,
        [UpdatedAtUtc] DATETIME2 NULL,
        [IsRemoved] BIT NOT NULL,
        CONSTRAINT [FK_GuestTableAssignments_Guests_GuestId]
            FOREIGN KEY ([GuestId]) REFERENCES [dbo].[Guests] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_GuestTableAssignments_ReceptionTables_ReceptionTableId]
            FOREIGN KEY ([ReceptionTableId]) REFERENCES [dbo].[ReceptionTables] ([Id]) ON DELETE CASCADE
    );
END
GO

IF NOT EXISTS (
    SELECT 1
    FROM sys.indexes
    WHERE name = N'IX_GuestTableAssignments_GuestId'
      AND object_id = OBJECT_ID(N'dbo.GuestTableAssignments')
)
BEGIN
    CREATE UNIQUE NONCLUSTERED INDEX [IX_GuestTableAssignments_GuestId]
    ON [dbo].[GuestTableAssignments] ([GuestId])
    WHERE [IsRemoved] = 0;
END
GO
