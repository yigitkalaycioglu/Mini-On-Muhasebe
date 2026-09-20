-- ============================================================
-- Mini Ön Muhasebe - Veri Tabanı Oluşturma Script'i
-- Veritabanı: OnMuhasebe (SQL Server)
--
-- Bu dosya, EF Core migration'larından üretilmiştir ve idempotent'tir:
-- daha önce oluşturulmuş nesneler atlanır, script birden fazla kez
-- çalıştırılabilir.
--
-- Çalıştırma (Türkçe karakterlerin bozulmaması için UTF-8 codepage şart):
--   sqlcmd -S "localhost\MSSQLSERVER01" -E -f i:65001 -i create_database.sql
--
-- Ardından örnek/test verisi için:
--   sqlcmd -S "localhost\MSSQLSERVER01" -E -f i:65001 -i seed_data.sql
--
-- Alternatif (geliştirme ortamında, proje kökünden):
--   dotnet ef database update --project OnMuhasebe.DataAccess --startup-project OnMuhasebeWeb
--
-- Oluşturulan tablolar (11 adet):
--   Kullanicilar, SatisElemanlari, Cariler, StokKartlari,
--   SatisFaturalari, SatisFaturaSatirlari,
--   AlisFaturalari, AlisFaturaSatirlari,
--   StokHareketler, CariHareketler, Parametreler
--   (+ EF Core'un migration takibi için __EFMigrationsHistory)
-- ============================================================

IF DB_ID(N'OnMuhasebe') IS NULL
BEGIN
    CREATE DATABASE [OnMuhasebe];
END;
GO

USE [OnMuhasebe];
GO

-- ============================================================
-- Tablolar, ilişkiler ve indeksler
-- ============================================================

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
    WHERE [MigrationId] = N'20260909132858_AddModelsToDb'
)
BEGIN
    CREATE TABLE [Cariler] (
        [Id] int NOT NULL IDENTITY,
        [CariKodu] nvarchar(max) NOT NULL,
        [Unvan] nvarchar(max) NOT NULL,
        [CariTipi] tinyint NOT NULL,
        [VergiDairesi] nvarchar(max) NULL,
        [VergiNo] nvarchar(max) NULL,
        [Telefon] nvarchar(max) NULL,
        [Adres] nvarchar(max) NULL,
        [Aktif] bit NOT NULL,
        CONSTRAINT [PK_Cariler] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260909132858_AddModelsToDb'
)
BEGIN
    CREATE TABLE [Kullanicilar] (
        [Id] int NOT NULL IDENTITY,
        [KullaniciAdi] nvarchar(max) NOT NULL,
        [SifreHash] nvarchar(max) NOT NULL,
        [AdSoyad] nvarchar(max) NOT NULL,
        [Rol] nvarchar(max) NOT NULL,
        [Aktif] bit NOT NULL,
        CONSTRAINT [PK_Kullanicilar] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260909132858_AddModelsToDb'
)
BEGIN
    CREATE TABLE [Parametreler] (
        [Id] int NOT NULL IDENTITY,
        [ParametreKodu] nvarchar(max) NOT NULL,
        [ParametreDegeri] nvarchar(max) NOT NULL,
        [Aciklama] nvarchar(max) NULL,
        CONSTRAINT [PK_Parametreler] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260909132858_AddModelsToDb'
)
BEGIN
    CREATE TABLE [SatisElemanlari] (
        [Id] int NOT NULL IDENTITY,
        [AdSoyad] nvarchar(max) NOT NULL,
        [Telefon] nvarchar(max) NOT NULL,
        [Aktif] bit NOT NULL,
        CONSTRAINT [PK_SatisElemanlari] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260909132858_AddModelsToDb'
)
BEGIN
    CREATE TABLE [StokKartlari] (
        [Id] int NOT NULL IDENTITY,
        [StokKodu] nvarchar(max) NOT NULL,
        [StokAdi] nvarchar(max) NOT NULL,
        [Birim] nvarchar(max) NOT NULL,
        [KdvOrani] decimal(18,2) NOT NULL,
        [AlisFiyati] decimal(18,2) NOT NULL,
        [SatisFiyati] decimal(18,2) NOT NULL,
        [KritikStok] decimal(18,2) NOT NULL,
        [Aktif] bit NOT NULL,
        CONSTRAINT [PK_StokKartlari] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260909132858_AddModelsToDb'
)
BEGIN
    CREATE TABLE [AlisFaturalari] (
        [Id] int NOT NULL IDENTITY,
        [FaturaNo] nvarchar(max) NOT NULL,
        [Tarih] datetime2 NOT NULL,
        [CariId] int NOT NULL,
        [AraToplam] decimal(18,2) NOT NULL,
        [KdvToplam] decimal(18,2) NOT NULL,
        [GenelToplam] decimal(18,2) NOT NULL,
        [Aciklama] nvarchar(max) NULL,
        [KullaniciId] int NOT NULL,
        [OlusturmaTarihi] datetime2 NOT NULL,
        CONSTRAINT [PK_AlisFaturalari] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_AlisFaturalari_Cariler_CariId] FOREIGN KEY ([CariId]) REFERENCES [Cariler] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_AlisFaturalari_Kullanicilar_KullaniciId] FOREIGN KEY ([KullaniciId]) REFERENCES [Kullanicilar] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260909132858_AddModelsToDb'
)
BEGIN
    CREATE TABLE [CariHareketler] (
        [Id] int NOT NULL IDENTITY,
        [CariId] int NOT NULL,
        [Tarih] datetime2 NOT NULL,
        [IslemTipi] nvarchar(max) NOT NULL,
        [Borc] decimal(18,2) NOT NULL,
        [Alacak] decimal(18,2) NOT NULL,
        [OdemeTuru] nvarchar(max) NULL,
        [BelgeNo] nvarchar(max) NULL,
        [Aciklama] nvarchar(max) NULL,
        [KullaniciId] int NOT NULL,
        CONSTRAINT [PK_CariHareketler] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_CariHareketler_Cariler_CariId] FOREIGN KEY ([CariId]) REFERENCES [Cariler] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_CariHareketler_Kullanicilar_KullaniciId] FOREIGN KEY ([KullaniciId]) REFERENCES [Kullanicilar] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260909132858_AddModelsToDb'
)
BEGIN
    CREATE TABLE [SatisFaturalari] (
        [Id] int NOT NULL IDENTITY,
        [FaturaNo] nvarchar(max) NOT NULL,
        [Tarih] datetime2 NOT NULL,
        [CariId] int NOT NULL,
        [SatisElemaniId] int NOT NULL,
        [AraToplam] decimal(18,2) NOT NULL,
        [KdvToplam] decimal(18,2) NOT NULL,
        [GenelToplam] decimal(18,2) NOT NULL,
        [Aciklama] nvarchar(max) NULL,
        [KullaniciId] int NOT NULL,
        [OlusturmaTarihi] datetime2 NOT NULL,
        CONSTRAINT [PK_SatisFaturalari] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_SatisFaturalari_Cariler_CariId] FOREIGN KEY ([CariId]) REFERENCES [Cariler] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_SatisFaturalari_Kullanicilar_KullaniciId] FOREIGN KEY ([KullaniciId]) REFERENCES [Kullanicilar] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_SatisFaturalari_SatisElemanlari_SatisElemaniId] FOREIGN KEY ([SatisElemaniId]) REFERENCES [SatisElemanlari] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260909132858_AddModelsToDb'
)
BEGIN
    CREATE TABLE [StokHareketler] (
        [Id] int NOT NULL IDENTITY,
        [StokId] int NOT NULL,
        [Tarih] datetime2 NOT NULL,
        [HareketTipi] nvarchar(max) NOT NULL,
        [Yon] nvarchar(max) NOT NULL,
        [Miktar] decimal(18,2) NOT NULL,
        [BelgeNo] nvarchar(max) NULL,
        [Aciklama] nvarchar(max) NULL,
        [KullaniciId] int NOT NULL,
        [StokKartiId] int NOT NULL,
        CONSTRAINT [PK_StokHareketler] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_StokHareketler_Kullanicilar_KullaniciId] FOREIGN KEY ([KullaniciId]) REFERENCES [Kullanicilar] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_StokHareketler_StokKartlari_StokKartiId] FOREIGN KEY ([StokKartiId]) REFERENCES [StokKartlari] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260909132858_AddModelsToDb'
)
BEGIN
    CREATE TABLE [AlisFaturaSatirlari] (
        [Id] int NOT NULL IDENTITY,
        [AlisFaturaId] int NOT NULL,
        [StokId] int NOT NULL,
        [Miktar] decimal(18,2) NOT NULL,
        [BirimFiyat] decimal(18,2) NOT NULL,
        [KdvOrani] decimal(18,2) NOT NULL,
        [SatirTutari] decimal(18,2) NOT NULL,
        [AlisFaturasiId] int NOT NULL,
        [StokKartiId] int NOT NULL,
        CONSTRAINT [PK_AlisFaturaSatirlari] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_AlisFaturaSatirlari_AlisFaturalari_AlisFaturasiId] FOREIGN KEY ([AlisFaturasiId]) REFERENCES [AlisFaturalari] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_AlisFaturaSatirlari_StokKartlari_StokKartiId] FOREIGN KEY ([StokKartiId]) REFERENCES [StokKartlari] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260909132858_AddModelsToDb'
)
BEGIN
    CREATE TABLE [SatisFaturaSatirlari] (
        [Id] int NOT NULL IDENTITY,
        [SatisFaturaId] int NOT NULL,
        [StokId] int NOT NULL,
        [Miktar] decimal(18,2) NOT NULL,
        [BirimFiyat] decimal(18,2) NOT NULL,
        [KdvOrani] decimal(18,2) NOT NULL,
        [SatirTutari] decimal(18,2) NOT NULL,
        [SatisFaturasiId] int NOT NULL,
        [StokKartiId] int NOT NULL,
        CONSTRAINT [PK_SatisFaturaSatirlari] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_SatisFaturaSatirlari_SatisFaturalari_SatisFaturasiId] FOREIGN KEY ([SatisFaturasiId]) REFERENCES [SatisFaturalari] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_SatisFaturaSatirlari_StokKartlari_StokKartiId] FOREIGN KEY ([StokKartiId]) REFERENCES [StokKartlari] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260909132858_AddModelsToDb'
)
BEGIN
    CREATE INDEX [IX_AlisFaturalari_CariId] ON [AlisFaturalari] ([CariId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260909132858_AddModelsToDb'
)
BEGIN
    CREATE INDEX [IX_AlisFaturalari_KullaniciId] ON [AlisFaturalari] ([KullaniciId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260909132858_AddModelsToDb'
)
BEGIN
    CREATE INDEX [IX_AlisFaturaSatirlari_AlisFaturasiId] ON [AlisFaturaSatirlari] ([AlisFaturasiId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260909132858_AddModelsToDb'
)
BEGIN
    CREATE INDEX [IX_AlisFaturaSatirlari_StokKartiId] ON [AlisFaturaSatirlari] ([StokKartiId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260909132858_AddModelsToDb'
)
BEGIN
    CREATE INDEX [IX_CariHareketler_CariId] ON [CariHareketler] ([CariId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260909132858_AddModelsToDb'
)
BEGIN
    CREATE INDEX [IX_CariHareketler_KullaniciId] ON [CariHareketler] ([KullaniciId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260909132858_AddModelsToDb'
)
BEGIN
    CREATE INDEX [IX_SatisFaturalari_CariId] ON [SatisFaturalari] ([CariId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260909132858_AddModelsToDb'
)
BEGIN
    CREATE INDEX [IX_SatisFaturalari_KullaniciId] ON [SatisFaturalari] ([KullaniciId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260909132858_AddModelsToDb'
)
BEGIN
    CREATE INDEX [IX_SatisFaturalari_SatisElemaniId] ON [SatisFaturalari] ([SatisElemaniId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260909132858_AddModelsToDb'
)
BEGIN
    CREATE INDEX [IX_SatisFaturaSatirlari_SatisFaturasiId] ON [SatisFaturaSatirlari] ([SatisFaturasiId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260909132858_AddModelsToDb'
)
BEGIN
    CREATE INDEX [IX_SatisFaturaSatirlari_StokKartiId] ON [SatisFaturaSatirlari] ([StokKartiId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260909132858_AddModelsToDb'
)
BEGIN
    CREATE INDEX [IX_StokHareketler_KullaniciId] ON [StokHareketler] ([KullaniciId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260909132858_AddModelsToDb'
)
BEGIN
    CREATE INDEX [IX_StokHareketler_StokKartiId] ON [StokHareketler] ([StokKartiId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260909132858_AddModelsToDb'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260909132858_AddModelsToDb', N'10.0.12');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260909140056_FixForeignKeys'
)
BEGIN
    ALTER TABLE [AlisFaturaSatirlari] DROP CONSTRAINT [FK_AlisFaturaSatirlari_AlisFaturalari_AlisFaturasiId];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260909140056_FixForeignKeys'
)
BEGIN
    ALTER TABLE [AlisFaturaSatirlari] DROP CONSTRAINT [FK_AlisFaturaSatirlari_StokKartlari_StokKartiId];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260909140056_FixForeignKeys'
)
BEGIN
    ALTER TABLE [SatisFaturaSatirlari] DROP CONSTRAINT [FK_SatisFaturaSatirlari_SatisFaturalari_SatisFaturasiId];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260909140056_FixForeignKeys'
)
BEGIN
    ALTER TABLE [SatisFaturaSatirlari] DROP CONSTRAINT [FK_SatisFaturaSatirlari_StokKartlari_StokKartiId];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260909140056_FixForeignKeys'
)
BEGIN
    ALTER TABLE [StokHareketler] DROP CONSTRAINT [FK_StokHareketler_StokKartlari_StokKartiId];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260909140056_FixForeignKeys'
)
BEGIN
    DROP INDEX [IX_StokHareketler_StokKartiId] ON [StokHareketler];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260909140056_FixForeignKeys'
)
BEGIN
    DROP INDEX [IX_SatisFaturaSatirlari_SatisFaturasiId] ON [SatisFaturaSatirlari];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260909140056_FixForeignKeys'
)
BEGIN
    DROP INDEX [IX_SatisFaturaSatirlari_StokKartiId] ON [SatisFaturaSatirlari];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260909140056_FixForeignKeys'
)
BEGIN
    DROP INDEX [IX_AlisFaturaSatirlari_AlisFaturasiId] ON [AlisFaturaSatirlari];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260909140056_FixForeignKeys'
)
BEGIN
    DROP INDEX [IX_AlisFaturaSatirlari_StokKartiId] ON [AlisFaturaSatirlari];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260909140056_FixForeignKeys'
)
BEGIN
    DECLARE @var nvarchar(max);
    SELECT @var = QUOTENAME([d].[name])
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[StokHareketler]') AND [c].[name] = N'StokKartiId');
    IF @var IS NOT NULL EXEC(N'ALTER TABLE [StokHareketler] DROP CONSTRAINT ' + @var + ';');
    ALTER TABLE [StokHareketler] DROP COLUMN [StokKartiId];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260909140056_FixForeignKeys'
)
BEGIN
    DECLARE @var1 nvarchar(max);
    SELECT @var1 = QUOTENAME([d].[name])
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[SatisFaturaSatirlari]') AND [c].[name] = N'SatisFaturasiId');
    IF @var1 IS NOT NULL EXEC(N'ALTER TABLE [SatisFaturaSatirlari] DROP CONSTRAINT ' + @var1 + ';');
    ALTER TABLE [SatisFaturaSatirlari] DROP COLUMN [SatisFaturasiId];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260909140056_FixForeignKeys'
)
BEGIN
    DECLARE @var2 nvarchar(max);
    SELECT @var2 = QUOTENAME([d].[name])
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[SatisFaturaSatirlari]') AND [c].[name] = N'StokKartiId');
    IF @var2 IS NOT NULL EXEC(N'ALTER TABLE [SatisFaturaSatirlari] DROP CONSTRAINT ' + @var2 + ';');
    ALTER TABLE [SatisFaturaSatirlari] DROP COLUMN [StokKartiId];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260909140056_FixForeignKeys'
)
BEGIN
    DECLARE @var3 nvarchar(max);
    SELECT @var3 = QUOTENAME([d].[name])
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[AlisFaturaSatirlari]') AND [c].[name] = N'AlisFaturasiId');
    IF @var3 IS NOT NULL EXEC(N'ALTER TABLE [AlisFaturaSatirlari] DROP CONSTRAINT ' + @var3 + ';');
    ALTER TABLE [AlisFaturaSatirlari] DROP COLUMN [AlisFaturasiId];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260909140056_FixForeignKeys'
)
BEGIN
    DECLARE @var4 nvarchar(max);
    SELECT @var4 = QUOTENAME([d].[name])
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[AlisFaturaSatirlari]') AND [c].[name] = N'StokKartiId');
    IF @var4 IS NOT NULL EXEC(N'ALTER TABLE [AlisFaturaSatirlari] DROP CONSTRAINT ' + @var4 + ';');
    ALTER TABLE [AlisFaturaSatirlari] DROP COLUMN [StokKartiId];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260909140056_FixForeignKeys'
)
BEGIN
    CREATE INDEX [IX_StokHareketler_StokId] ON [StokHareketler] ([StokId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260909140056_FixForeignKeys'
)
BEGIN
    CREATE INDEX [IX_SatisFaturaSatirlari_SatisFaturaId] ON [SatisFaturaSatirlari] ([SatisFaturaId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260909140056_FixForeignKeys'
)
BEGIN
    CREATE INDEX [IX_SatisFaturaSatirlari_StokId] ON [SatisFaturaSatirlari] ([StokId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260909140056_FixForeignKeys'
)
BEGIN
    CREATE INDEX [IX_AlisFaturaSatirlari_AlisFaturaId] ON [AlisFaturaSatirlari] ([AlisFaturaId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260909140056_FixForeignKeys'
)
BEGIN
    CREATE INDEX [IX_AlisFaturaSatirlari_StokId] ON [AlisFaturaSatirlari] ([StokId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260909140056_FixForeignKeys'
)
BEGIN
    ALTER TABLE [AlisFaturaSatirlari] ADD CONSTRAINT [FK_AlisFaturaSatirlari_AlisFaturalari_AlisFaturaId] FOREIGN KEY ([AlisFaturaId]) REFERENCES [AlisFaturalari] ([Id]) ON DELETE CASCADE;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260909140056_FixForeignKeys'
)
BEGIN
    ALTER TABLE [AlisFaturaSatirlari] ADD CONSTRAINT [FK_AlisFaturaSatirlari_StokKartlari_StokId] FOREIGN KEY ([StokId]) REFERENCES [StokKartlari] ([Id]) ON DELETE CASCADE;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260909140056_FixForeignKeys'
)
BEGIN
    ALTER TABLE [SatisFaturaSatirlari] ADD CONSTRAINT [FK_SatisFaturaSatirlari_SatisFaturalari_SatisFaturaId] FOREIGN KEY ([SatisFaturaId]) REFERENCES [SatisFaturalari] ([Id]) ON DELETE CASCADE;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260909140056_FixForeignKeys'
)
BEGIN
    ALTER TABLE [SatisFaturaSatirlari] ADD CONSTRAINT [FK_SatisFaturaSatirlari_StokKartlari_StokId] FOREIGN KEY ([StokId]) REFERENCES [StokKartlari] ([Id]) ON DELETE CASCADE;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260909140056_FixForeignKeys'
)
BEGIN
    ALTER TABLE [StokHareketler] ADD CONSTRAINT [FK_StokHareketler_StokKartlari_StokId] FOREIGN KEY ([StokId]) REFERENCES [StokKartlari] ([Id]) ON DELETE CASCADE;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260909140056_FixForeignKeys'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260909140056_FixForeignKeys', N'10.0.12');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260914134142_AddValidationAttributes'
)
BEGIN
    DECLARE @var5 nvarchar(max);
    SELECT @var5 = QUOTENAME([d].[name])
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[StokKartlari]') AND [c].[name] = N'StokKodu');
    IF @var5 IS NOT NULL EXEC(N'ALTER TABLE [StokKartlari] DROP CONSTRAINT ' + @var5 + ';');
    ALTER TABLE [StokKartlari] ALTER COLUMN [StokKodu] nvarchar(20) NOT NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260914134142_AddValidationAttributes'
)
BEGIN
    DECLARE @var6 nvarchar(max);
    SELECT @var6 = QUOTENAME([d].[name])
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[StokKartlari]') AND [c].[name] = N'StokAdi');
    IF @var6 IS NOT NULL EXEC(N'ALTER TABLE [StokKartlari] DROP CONSTRAINT ' + @var6 + ';');
    ALTER TABLE [StokKartlari] ALTER COLUMN [StokAdi] nvarchar(150) NOT NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260914134142_AddValidationAttributes'
)
BEGIN
    DECLARE @var7 nvarchar(max);
    SELECT @var7 = QUOTENAME([d].[name])
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[StokKartlari]') AND [c].[name] = N'Birim');
    IF @var7 IS NOT NULL EXEC(N'ALTER TABLE [StokKartlari] DROP CONSTRAINT ' + @var7 + ';');
    ALTER TABLE [StokKartlari] ALTER COLUMN [Birim] nvarchar(10) NOT NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260914134142_AddValidationAttributes'
)
BEGIN
    DECLARE @var8 nvarchar(max);
    SELECT @var8 = QUOTENAME([d].[name])
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[StokHareketler]') AND [c].[name] = N'Aciklama');
    IF @var8 IS NOT NULL EXEC(N'ALTER TABLE [StokHareketler] DROP CONSTRAINT ' + @var8 + ';');
    ALTER TABLE [StokHareketler] ALTER COLUMN [Aciklama] nvarchar(250) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260914134142_AddValidationAttributes'
)
BEGIN
    DECLARE @var9 nvarchar(max);
    SELECT @var9 = QUOTENAME([d].[name])
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[SatisFaturalari]') AND [c].[name] = N'Aciklama');
    IF @var9 IS NOT NULL EXEC(N'ALTER TABLE [SatisFaturalari] DROP CONSTRAINT ' + @var9 + ';');
    ALTER TABLE [SatisFaturalari] ALTER COLUMN [Aciklama] nvarchar(250) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260914134142_AddValidationAttributes'
)
BEGIN
    DECLARE @var10 nvarchar(max);
    SELECT @var10 = QUOTENAME([d].[name])
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[SatisElemanlari]') AND [c].[name] = N'Telefon');
    IF @var10 IS NOT NULL EXEC(N'ALTER TABLE [SatisElemanlari] DROP CONSTRAINT ' + @var10 + ';');
    ALTER TABLE [SatisElemanlari] ALTER COLUMN [Telefon] nvarchar(20) NOT NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260914134142_AddValidationAttributes'
)
BEGIN
    DECLARE @var11 nvarchar(max);
    SELECT @var11 = QUOTENAME([d].[name])
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[SatisElemanlari]') AND [c].[name] = N'AdSoyad');
    IF @var11 IS NOT NULL EXEC(N'ALTER TABLE [SatisElemanlari] DROP CONSTRAINT ' + @var11 + ';');
    ALTER TABLE [SatisElemanlari] ALTER COLUMN [AdSoyad] nvarchar(100) NOT NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260914134142_AddValidationAttributes'
)
BEGIN
    DECLARE @var12 nvarchar(max);
    SELECT @var12 = QUOTENAME([d].[name])
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Parametreler]') AND [c].[name] = N'ParametreKodu');
    IF @var12 IS NOT NULL EXEC(N'ALTER TABLE [Parametreler] DROP CONSTRAINT ' + @var12 + ';');
    ALTER TABLE [Parametreler] ALTER COLUMN [ParametreKodu] nvarchar(50) NOT NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260914134142_AddValidationAttributes'
)
BEGIN
    DECLARE @var13 nvarchar(max);
    SELECT @var13 = QUOTENAME([d].[name])
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Parametreler]') AND [c].[name] = N'ParametreDegeri');
    IF @var13 IS NOT NULL EXEC(N'ALTER TABLE [Parametreler] DROP CONSTRAINT ' + @var13 + ';');
    ALTER TABLE [Parametreler] ALTER COLUMN [ParametreDegeri] nvarchar(250) NOT NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260914134142_AddValidationAttributes'
)
BEGIN
    DECLARE @var14 nvarchar(max);
    SELECT @var14 = QUOTENAME([d].[name])
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Parametreler]') AND [c].[name] = N'Aciklama');
    IF @var14 IS NOT NULL EXEC(N'ALTER TABLE [Parametreler] DROP CONSTRAINT ' + @var14 + ';');
    ALTER TABLE [Parametreler] ALTER COLUMN [Aciklama] nvarchar(250) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260914134142_AddValidationAttributes'
)
BEGIN
    DECLARE @var15 nvarchar(max);
    SELECT @var15 = QUOTENAME([d].[name])
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Kullanicilar]') AND [c].[name] = N'KullaniciAdi');
    IF @var15 IS NOT NULL EXEC(N'ALTER TABLE [Kullanicilar] DROP CONSTRAINT ' + @var15 + ';');
    ALTER TABLE [Kullanicilar] ALTER COLUMN [KullaniciAdi] nvarchar(50) NOT NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260914134142_AddValidationAttributes'
)
BEGIN
    DECLARE @var16 nvarchar(max);
    SELECT @var16 = QUOTENAME([d].[name])
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Kullanicilar]') AND [c].[name] = N'AdSoyad');
    IF @var16 IS NOT NULL EXEC(N'ALTER TABLE [Kullanicilar] DROP CONSTRAINT ' + @var16 + ';');
    ALTER TABLE [Kullanicilar] ALTER COLUMN [AdSoyad] nvarchar(100) NOT NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260914134142_AddValidationAttributes'
)
BEGIN
    DECLARE @var17 nvarchar(max);
    SELECT @var17 = QUOTENAME([d].[name])
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Cariler]') AND [c].[name] = N'VergiNo');
    IF @var17 IS NOT NULL EXEC(N'ALTER TABLE [Cariler] DROP CONSTRAINT ' + @var17 + ';');
    ALTER TABLE [Cariler] ALTER COLUMN [VergiNo] nvarchar(20) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260914134142_AddValidationAttributes'
)
BEGIN
    DECLARE @var18 nvarchar(max);
    SELECT @var18 = QUOTENAME([d].[name])
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Cariler]') AND [c].[name] = N'VergiDairesi');
    IF @var18 IS NOT NULL EXEC(N'ALTER TABLE [Cariler] DROP CONSTRAINT ' + @var18 + ';');
    ALTER TABLE [Cariler] ALTER COLUMN [VergiDairesi] nvarchar(50) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260914134142_AddValidationAttributes'
)
BEGIN
    DECLARE @var19 nvarchar(max);
    SELECT @var19 = QUOTENAME([d].[name])
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Cariler]') AND [c].[name] = N'Unvan');
    IF @var19 IS NOT NULL EXEC(N'ALTER TABLE [Cariler] DROP CONSTRAINT ' + @var19 + ';');
    ALTER TABLE [Cariler] ALTER COLUMN [Unvan] nvarchar(150) NOT NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260914134142_AddValidationAttributes'
)
BEGIN
    DECLARE @var20 nvarchar(max);
    SELECT @var20 = QUOTENAME([d].[name])
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Cariler]') AND [c].[name] = N'Telefon');
    IF @var20 IS NOT NULL EXEC(N'ALTER TABLE [Cariler] DROP CONSTRAINT ' + @var20 + ';');
    ALTER TABLE [Cariler] ALTER COLUMN [Telefon] nvarchar(20) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260914134142_AddValidationAttributes'
)
BEGIN
    DECLARE @var21 nvarchar(max);
    SELECT @var21 = QUOTENAME([d].[name])
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Cariler]') AND [c].[name] = N'CariKodu');
    IF @var21 IS NOT NULL EXEC(N'ALTER TABLE [Cariler] DROP CONSTRAINT ' + @var21 + ';');
    ALTER TABLE [Cariler] ALTER COLUMN [CariKodu] nvarchar(20) NOT NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260914134142_AddValidationAttributes'
)
BEGIN
    DECLARE @var22 nvarchar(max);
    SELECT @var22 = QUOTENAME([d].[name])
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Cariler]') AND [c].[name] = N'Adres');
    IF @var22 IS NOT NULL EXEC(N'ALTER TABLE [Cariler] DROP CONSTRAINT ' + @var22 + ';');
    ALTER TABLE [Cariler] ALTER COLUMN [Adres] nvarchar(250) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260914134142_AddValidationAttributes'
)
BEGIN
    DECLARE @var23 nvarchar(max);
    SELECT @var23 = QUOTENAME([d].[name])
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[CariHareketler]') AND [c].[name] = N'OdemeTuru');
    IF @var23 IS NOT NULL EXEC(N'ALTER TABLE [CariHareketler] DROP CONSTRAINT ' + @var23 + ';');
    ALTER TABLE [CariHareketler] ALTER COLUMN [OdemeTuru] nvarchar(20) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260914134142_AddValidationAttributes'
)
BEGIN
    DECLARE @var24 nvarchar(max);
    SELECT @var24 = QUOTENAME([d].[name])
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[CariHareketler]') AND [c].[name] = N'Aciklama');
    IF @var24 IS NOT NULL EXEC(N'ALTER TABLE [CariHareketler] DROP CONSTRAINT ' + @var24 + ';');
    ALTER TABLE [CariHareketler] ALTER COLUMN [Aciklama] nvarchar(250) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260914134142_AddValidationAttributes'
)
BEGIN
    DECLARE @var25 nvarchar(max);
    SELECT @var25 = QUOTENAME([d].[name])
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[AlisFaturalari]') AND [c].[name] = N'Aciklama');
    IF @var25 IS NOT NULL EXEC(N'ALTER TABLE [AlisFaturalari] DROP CONSTRAINT ' + @var25 + ';');
    ALTER TABLE [AlisFaturalari] ALTER COLUMN [Aciklama] nvarchar(250) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260914134142_AddValidationAttributes'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260914134142_AddValidationAttributes', N'10.0.12');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260918162558_AddUniqueIndexes'
)
BEGIN
    DECLARE @var26 nvarchar(max);
    SELECT @var26 = QUOTENAME([d].[name])
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[StokHareketler]') AND [c].[name] = N'BelgeNo');
    IF @var26 IS NOT NULL EXEC(N'ALTER TABLE [StokHareketler] DROP CONSTRAINT ' + @var26 + ';');
    ALTER TABLE [StokHareketler] ALTER COLUMN [BelgeNo] nvarchar(450) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260918162558_AddUniqueIndexes'
)
BEGIN
    DECLARE @var27 nvarchar(max);
    SELECT @var27 = QUOTENAME([d].[name])
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[SatisFaturalari]') AND [c].[name] = N'FaturaNo');
    IF @var27 IS NOT NULL EXEC(N'ALTER TABLE [SatisFaturalari] DROP CONSTRAINT ' + @var27 + ';');
    ALTER TABLE [SatisFaturalari] ALTER COLUMN [FaturaNo] nvarchar(450) NOT NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260918162558_AddUniqueIndexes'
)
BEGIN
    DECLARE @var28 nvarchar(max);
    SELECT @var28 = QUOTENAME([d].[name])
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[CariHareketler]') AND [c].[name] = N'BelgeNo');
    IF @var28 IS NOT NULL EXEC(N'ALTER TABLE [CariHareketler] DROP CONSTRAINT ' + @var28 + ';');
    ALTER TABLE [CariHareketler] ALTER COLUMN [BelgeNo] nvarchar(450) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260918162558_AddUniqueIndexes'
)
BEGIN
    DECLARE @var29 nvarchar(max);
    SELECT @var29 = QUOTENAME([d].[name])
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[AlisFaturalari]') AND [c].[name] = N'FaturaNo');
    IF @var29 IS NOT NULL EXEC(N'ALTER TABLE [AlisFaturalari] DROP CONSTRAINT ' + @var29 + ';');
    ALTER TABLE [AlisFaturalari] ALTER COLUMN [FaturaNo] nvarchar(450) NOT NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260918162558_AddUniqueIndexes'
)
BEGIN
    CREATE UNIQUE INDEX [IX_StokKartlari_StokKodu] ON [StokKartlari] ([StokKodu]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260918162558_AddUniqueIndexes'
)
BEGIN
    CREATE INDEX [IX_StokHareketler_BelgeNo] ON [StokHareketler] ([BelgeNo]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260918162558_AddUniqueIndexes'
)
BEGIN
    CREATE UNIQUE INDEX [IX_SatisFaturalari_FaturaNo] ON [SatisFaturalari] ([FaturaNo]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260918162558_AddUniqueIndexes'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Parametreler_ParametreKodu] ON [Parametreler] ([ParametreKodu]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260918162558_AddUniqueIndexes'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Kullanicilar_KullaniciAdi] ON [Kullanicilar] ([KullaniciAdi]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260918162558_AddUniqueIndexes'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Cariler_CariKodu] ON [Cariler] ([CariKodu]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260918162558_AddUniqueIndexes'
)
BEGIN
    CREATE INDEX [IX_CariHareketler_BelgeNo] ON [CariHareketler] ([BelgeNo]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260918162558_AddUniqueIndexes'
)
BEGIN
    CREATE UNIQUE INDEX [IX_AlisFaturalari_FaturaNo] ON [AlisFaturalari] ([FaturaNo]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260918162558_AddUniqueIndexes'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260918162558_AddUniqueIndexes', N'10.0.12');
END;

COMMIT;
GO

