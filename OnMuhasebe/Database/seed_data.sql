-- ============================================================
-- Mini Ön Muhasebe - Örnek/Test Verisi
-- Veritabanı: OnMuhasebe
--
-- Çalıştırma (Türkçe karakterlerin bozulmaması için UTF-8 codepage şart):
--   sqlcmd -S "localhost\MSSQLSERVER01" -E -f i:65001 -i seed_data.sql
-- (Bu dosya UTF-8 BOM ile kaydedildi; -f i:65001 yine de önerilir.)
--
-- Not: Tablolar EF Core migration'ı ile zaten oluşturulmuş olmalı
--      (dotnet ef database update)
-- ============================================================

USE OnMuhasebe;
GO

-- ------------------------------------------------------------
-- Kullanicilar
-- Not: Şifreler örnek/test amaçlıdır; her iki kullanıcının şifresi "123456".
-- Biçim: iterasyon.salt(Base64).hash(Base64) — PBKDF2-SHA256, OnMuhasebe.Utility/SifreYardimcisi.
-- Salt her kullanıcı için rastgele üretildiğinden aynı şifre farklı hash verir.
-- ------------------------------------------------------------
SET IDENTITY_INSERT Kullanicilar ON;
INSERT INTO Kullanicilar (Id, KullaniciAdi, SifreHash, AdSoyad, Rol, Aktif) VALUES
(1, N'admin',       N'600000.Y3kSZaRgetUjROpcqMWOCA==.JfqchpyJXoV3yzqv7aq38r4LGaZhVMTVUDXUw3N1ydA=', N'Admin Kullanıcı', N'Yönetici', 1),
(2, N'ayse.yilmaz',  N'600000.+BbnhzAi+jdxyEh8d2NtjQ==.ItcYaPC8rzjvUSiwpFWVLoHskAsBUh09Du2iryjJywA=', N'Ayşe Yılmaz',      N'Standart', 1);
SET IDENTITY_INSERT Kullanicilar OFF;

-- ------------------------------------------------------------
-- SatisElemanlari
-- ------------------------------------------------------------
SET IDENTITY_INSERT SatisElemanlari ON;
INSERT INTO SatisElemanlari (Id, AdSoyad, Telefon, Aktif) VALUES
(1, N'Mehmet Demir', N'0555 111 22 33', 1),
(2, N'Zeynep Kaya',  N'0555 222 33 44', 1),
(3, N'Ali Çelik',    N'0555 333 44 55', 0);
SET IDENTITY_INSERT SatisElemanlari OFF;

-- ------------------------------------------------------------
-- Cariler (CariTipi: 1=Müşteri, 2=Tedarikçi, 3=Her ikisi)
-- ------------------------------------------------------------
SET IDENTITY_INSERT Cariler ON;
INSERT INTO Cariler (Id, CariKodu, Unvan, CariTipi, VergiDairesi, VergiNo, Telefon, Adres, Aktif) VALUES
(1, N'M0001',  N'ABC Ticaret Ltd. Şti.',  1, N'Kadıköy',  N'1234567890', N'0216 111 22 33', N'İstanbul', 1),
(2, N'M0002',  N'Yılmaz Gıda A.Ş.',       1, N'Çankaya',  N'2234567891', N'0312 222 33 44', N'Ankara',   1),
(3, N'T0001',  N'Özkan Tedarik San.',     2, N'Bornova',  N'3234567892', N'0232 333 44 55', N'İzmir',    1),
(4, N'MT0001', N'Deniz Toptan Ltd.',      3, N'Nilüfer',  N'4234567893', N'0224 444 55 66', N'Bursa',    1);
SET IDENTITY_INSERT Cariler OFF;

-- ------------------------------------------------------------
-- StokKartlari
-- ------------------------------------------------------------
SET IDENTITY_INSERT StokKartlari ON;
INSERT INTO StokKartlari (Id, StokKodu, StokAdi, Birim, KdvOrani, AlisFiyati, SatisFiyati, KritikStok, Aktif) VALUES
(1, N'STK001', N'A4 Fotokopi Kağıdı', N'Paket', 20, 150.00, 200.00, 10, 1),
(2, N'STK002', N'Kurşun Kalem',       N'Adet',  20,   2.50,   5.00, 50, 1),
(3, N'STK003', N'Klasör',             N'Adet',  20,  25.00,  40.00, 20, 1),
(4, N'STK004', N'Yazıcı Toner',       N'Adet',  20, 450.00, 600.00,  5, 1),
(5, N'STK005', N'Zımba Teli',         N'Kutu',  20,  15.00,  25.00, 30, 1);
SET IDENTITY_INSERT StokKartlari OFF;

-- ------------------------------------------------------------
-- Parametreler (Bölüm 9)
-- ------------------------------------------------------------
SET IDENTITY_INSERT Parametreler ON;
INSERT INTO Parametreler (Id, ParametreKodu, ParametreDegeri, Aciklama) VALUES
(1,  N'FirmaUnvani',              N'ESBİ Bilişim Ltd. Şti.', N'Rapor/fatura başlığı'),
(2,  N'VergiDairesiNo',           N'Sakarya / 1234567890',   N'Firma vergi bilgileri'),
(3,  N'VarsayilanKdvOrani',       N'20',                     N'Yeni satırda öntanımlı KDV oranı'),
(4,  N'ParaBirimi',               N'TL',                     N'Görüntüleme birimi'),
(5,  N'OndalikBasamak',           N'2',                      N'Tutar hassasiyeti'),
(6,  N'NegatifStokKontrolu',      N'Kapali',                 N'Stok yetersizse davranış'),
(7,  N'SatisFaturaNoFormati',     N'SAT-{yyyy}-{0000}',      N'Otomatik numara şablonu'),
(8,  N'AlisFaturaNoFormati',      N'ALS-{yyyy}-{0000}',      N'Otomatik numara şablonu'),
(9,  N'SayimFazlasiFisNoFormati', N'SF-{yyyy}-{0000}',       N'Otomatik numara şablonu'),
(10, N'SayimEksigiFisNoFormati',  N'SE-{yyyy}-{0000}',       N'Otomatik numara şablonu'),
(11, N'KritikStokUyarisi',        N'Acik',                   N'Uyarı göster/gösterme');
SET IDENTITY_INSERT Parametreler OFF;

-- ------------------------------------------------------------
-- AlisFaturalari (CariId: Tedarikçi)
-- ------------------------------------------------------------
SET IDENTITY_INSERT AlisFaturalari ON;
INSERT INTO AlisFaturalari (Id, FaturaNo, Tarih, CariId, AraToplam, KdvToplam, GenelToplam, Aciklama, KullaniciId, OlusturmaTarihi) VALUES
(1, N'ALS-2026-0001', '2026-08-28', 3, 1500.00, 300.00, 1800.00, N'İlk alış faturası', 1, SYSDATETIME()),
(2, N'ALS-2026-0002', '2026-09-03', 4,  750.00, 150.00,  900.00, NULL,                  1, SYSDATETIME());
SET IDENTITY_INSERT AlisFaturalari OFF;

-- ------------------------------------------------------------
-- SatisFaturalari
-- ------------------------------------------------------------
SET IDENTITY_INSERT SatisFaturalari ON;
INSERT INTO SatisFaturalari (Id, FaturaNo, Tarih, CariId, SatisElemaniId, AraToplam, KdvToplam, GenelToplam, Aciklama, KullaniciId, OlusturmaTarihi) VALUES
(1, N'SAT-2026-0001', '2026-09-01', 1, 1,  400.00,  80.00,  480.00, N'İlk satış faturası', 1, SYSDATETIME()),
(2, N'SAT-2026-0002', '2026-09-05', 2, 2, 1200.00, 240.00, 1440.00, NULL,                  2, SYSDATETIME());
SET IDENTITY_INSERT SatisFaturalari OFF;

-- ------------------------------------------------------------
-- AlisFaturaSatirlari
-- ------------------------------------------------------------
SET IDENTITY_INSERT AlisFaturaSatirlari ON;
INSERT INTO AlisFaturaSatirlari (Id, AlisFaturaId, StokId, Miktar, BirimFiyat, KdvOrani, SatirTutari) VALUES
(1, 1, 1, 10, 150.00, 20, 1500.00),
(2, 2, 5, 50,  15.00, 20,  750.00);
SET IDENTITY_INSERT AlisFaturaSatirlari OFF;

-- ------------------------------------------------------------
-- SatisFaturaSatirlari
-- ------------------------------------------------------------
SET IDENTITY_INSERT SatisFaturaSatirlari ON;
INSERT INTO SatisFaturaSatirlari (Id, SatisFaturaId, StokId, Miktar, BirimFiyat, KdvOrani, SatirTutari) VALUES
(1, 1, 1, 2, 200.00, 20,  400.00),
(2, 2, 4, 2, 600.00, 20, 1200.00);
SET IDENTITY_INSERT SatisFaturaSatirlari OFF;

-- ------------------------------------------------------------
-- StokHareketler (fatura + sayım fişi hareketleri)
-- ------------------------------------------------------------
SET IDENTITY_INSERT StokHareketler ON;
INSERT INTO StokHareketler (Id, StokId, Tarih, HareketTipi, Yon, Miktar, BelgeNo, Aciklama, KullaniciId) VALUES
(1, 1, '2026-08-28', N'Alis',         N'Giris', 10, N'ALS-2026-0001', N'Alış faturası girişi', 1),
(2, 1, '2026-09-01', N'Satis',        N'Cikis',  2, N'SAT-2026-0001', N'Satış faturası çıkışı', 1),
(3, 4, '2026-09-05', N'Satis',        N'Cikis',  2, N'SAT-2026-0002', N'Satış faturası çıkışı', 2),
(4, 5, '2026-09-03', N'Alis',         N'Giris', 50, N'ALS-2026-0002', N'Alış faturası girişi', 1),
(5, 3, '2026-09-06', N'SayimFazlasi', N'Giris',  5, N'SF-2026-0001',  N'Sayımda fazla çıktı',  1),
(6, 4, '2026-09-04', N'SayimFazlasi', N'Giris',  6, N'SF-2026-0002',  N'Sayımda fazla çıktı',  1);
SET IDENTITY_INSERT StokHareketler OFF;

-- ------------------------------------------------------------
-- CariHareketler
-- ------------------------------------------------------------
SET IDENTITY_INSERT CariHareketler ON;
INSERT INTO CariHareketler (Id, CariId, Tarih, IslemTipi, Borc, Alacak, OdemeTuru, BelgeNo, Aciklama, KullaniciId) VALUES
(1, 1, '2026-09-01', N'Satis',    480.00,    0.00, NULL,     N'SAT-2026-0001', N'Satış faturası',    1),
(2, 2, '2026-09-05', N'Satis',   1440.00,    0.00, NULL,     N'SAT-2026-0002', N'Satış faturası',    2),
(3, 3, '2026-08-28', N'Alis',       0.00, 1800.00, NULL,     N'ALS-2026-0001', N'Alış faturası',     1),
(4, 4, '2026-09-03', N'Alis',       0.00,  900.00, NULL,     N'ALS-2026-0002', N'Alış faturası',     1),
(5, 1, '2026-09-07', N'Tahsilat',   0.00,  200.00, N'Nakit',  N'TAH-2026-0001', N'Kısmi tahsilat',   1),
(6, 3, '2026-09-08', N'Odeme',   1000.00,    0.00, N'Havale', N'ODE-2026-0001', N'Kısmi ödeme',      1);
SET IDENTITY_INSERT CariHareketler OFF;
GO

PRINT N'Örnek veri başarıyla eklendi.';
