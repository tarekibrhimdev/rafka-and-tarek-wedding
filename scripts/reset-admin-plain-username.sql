/*
  Reset admin login: username + plaintext password in PasswordHash
  ================================================================
  App must use PlainTextPasswordHasher (see Program.cs). No table ALTER required:
  AspNetUsers.Email / NormalizedEmail may be NULL.

  Login: Username = admin, Password = T@rek92$$

  WARNING: Passwords in plaintext are insecure. Use only if you accept that risk.

  Run against your wedding database (e.g. wedding-management-system).
*/

SET NOCOUNT ON;

/* Clear Identity users (adjust if you use roles/claims you need to keep) */
DELETE FROM [AspNetUserTokens];
DELETE FROM [AspNetUserLogins];
DELETE FROM [AspNetUserClaims];
DELETE FROM [AspNetUserRoles];
DELETE FROM [AspNetUsers];
GO

DECLARE @id nvarchar(450) = N'c0a8f100-0000-4000-8000-000000000001';
DECLARE @stamp1 nvarchar(max) = N'7c2e4b1a9f8d4c3e2b1a0987654321fe';
DECLARE @stamp2 nvarchar(max) = N'6d3f5c2b8e9a4d1f0c2b3e4a5d6f7089';

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
    @id,
    0,
    SYSUTCDATETIME(),
    N'admin',
    N'ADMIN',
    NULL,
    NULL,
    1,
    N'T@rek92$$',
    @stamp1,
    @stamp2,
    NULL,
    0,
    0,
    NULL,
    1,
    0
);
GO
