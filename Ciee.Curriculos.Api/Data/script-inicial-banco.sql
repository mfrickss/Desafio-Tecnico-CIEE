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
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260929194843_CriacaoTabelaCandidatos'
)
BEGIN
    CREATE TABLE [Candidatos] (
        [Id] uniqueidentifier NOT NULL,
        [NomeCompleto] nvarchar(150) NOT NULL,
        [Email] nvarchar(150) NOT NULL,
        [Telefone] nvarchar(30) NULL,
        [CargoInteresse] nvarchar(100) NULL,
        [ResumoProfissional] nvarchar(2000) NULL,
        [DataCadastro] datetime2 NOT NULL,
        [TeveOrigemPdf] bit NOT NULL,
        CONSTRAINT [PK_Candidatos] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260929194843_CriacaoTabelaCandidatos'
)
BEGIN
    CREATE INDEX [IX_Candidatos_Email] ON [Candidatos] ([Email]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260929194843_CriacaoTabelaCandidatos'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260929194843_CriacaoTabelaCandidatos', N'8.0.11');
END;
GO

COMMIT;
GO

