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
    WHERE [MigrationId] = N'20250218131423_FixCompositeKey'
)
BEGIN
    CREATE TABLE [Izdelki] (
        [ID] int NOT NULL IDENTITY,
        [Naziv] nvarchar(max) NOT NULL,
        CONSTRAINT [PK_Izdelki] PRIMARY KEY ([ID])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250218131423_FixCompositeKey'
)
BEGIN
    CREATE TABLE [Skladisca] (
        [ID] int NOT NULL IDENTITY,
        [Naziv] nvarchar(max) NOT NULL,
        CONSTRAINT [PK_Skladisca] PRIMARY KEY ([ID])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250218131423_FixCompositeKey'
)
BEGIN
    CREATE TABLE [SkladisceHasIzdelki] (
        [IDIz] int NOT NULL,
        [IDSk] int NOT NULL,
        [Zaloga] int NOT NULL,
        CONSTRAINT [PK_SkladisceHasIzdelki] PRIMARY KEY ([IDIz], [IDSk]),
        CONSTRAINT [FK_SkladisceHasIzdelki_Izdelki_IDIz] FOREIGN KEY ([IDIz]) REFERENCES [Izdelki] ([ID]) ON DELETE CASCADE,
        CONSTRAINT [FK_SkladisceHasIzdelki_Skladisca_IDSk] FOREIGN KEY ([IDSk]) REFERENCES [Skladisca] ([ID]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250218131423_FixCompositeKey'
)
BEGIN
    CREATE INDEX [IX_SkladisceHasIzdelki_IDSk] ON [SkladisceHasIzdelki] ([IDSk]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250218131423_FixCompositeKey'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20250218131423_FixCompositeKey', N'9.0.2');
END;

COMMIT;
GO

