/*
  DESTRUCTIVE — drops seating tables and all assignments.
  Run only if you need to remove the seating feature from the database.
*/

SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

IF OBJECT_ID(N'[dbo].[GuestTableAssignments]', N'U') IS NOT NULL
    DROP TABLE [dbo].[GuestTableAssignments];
GO

IF OBJECT_ID(N'[dbo].[ReceptionTables]', N'U') IS NOT NULL
    DROP TABLE [dbo].[ReceptionTables];
GO
