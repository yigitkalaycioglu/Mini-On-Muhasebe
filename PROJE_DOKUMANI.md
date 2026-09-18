Mini Ön Muhasebe Uygulaması — Staj Proje Dokümanı
Proje Adı: Mini Ön Muhasebe Uygulaması
Belge Türü: Staj Proje Tanımı

1. Proje Özeti
Küçük bir işletmenin satış/alış faturaları, cari hesap, stok ve tahsilat/ödeme işlemlerini takip edebileceği basit bir ön muhasebe uygulaması geliştirilecektir. Uygulama fatura keser (alış ve satış), cari hesap açar, satış elemanı tanımlar, stok hareketlerini gösterir ve sayım fişleri ile stok düzeltmesi yapar.
Uygulama bilinçli olarak sade tutulmuştur. e-Fatura/e-Arşiv, GİB entegrasyonu ve resmi muhasebe kapsam dışıdır.
2. Amaç ve Kazanımlar
Stajyerin bu proje sonunda pratik yapacağı konular:
- İlişkisel veri tabanı tasarımı (tablo, ilişki, foreign key)
- CRUD (Ekle/Listele/Güncelle/Sil) ekranları
- Kullanıcı girişi ve rol bazlı yetkilendirme
- Temel iş kuralları (stok giriş/çıkış, cari bakiye)
- Parametre yönetimi ve basit raporlama
- Kaynak kod yönetimi (Git)
3. Kapsam
3.1 Kapsam İçi
- Kullanıcı girişi ve yetkilendirme
- Satış elemanı tanımlama
- Cari hesap yönetimi (müşteri / tedarikçi)
- Stok kartı yönetimi
- Stok işlemlerini görüntüleme + Sayım Fazlası ve Sayım Eksiği fişleri
- Satış faturası
- Alış faturası
- Tahsilat / ödeme
- Parametreler
- Basit raporlar
3.2 Kapsam Dışı
- e-Fatura / e-Arşiv ve GİB entegrasyonu
- Resmi muhasebe (muhasebe fişi, mizan, defter, beyanname)
- Çoklu firma / dönem, çoklu para birimi, çoklu depo
4. Teknoloji ve Ortam

| Katman | Önerilen | Alternatif |
| Dil / Platform | C# (.NET) | Tercihe göre serbest |
| Arayüz | WinForms (masaüstü) | ASP.NET Core (web) |
| Veri Tabanı | SQL Server | PostgreSQL / MySQL |
| Veri Erişimi | ADO.NET / Dapper | Entity Framework |
| Kaynak Kontrol | Git | — |

5. Kullanıcı Rolleri ve Yetkilendirme
Basit tutmak için iki rol yeterlidir:

| Rol | Yetki |
| Yönetici | Tüm ekranlar + kullanıcı yönetimi + parametreler |
| Standart | Cari, stok, satış/alış faturası, tahsilat/ödeme (kullanıcı yönetimi ve parametreler hariç) |

Kurallar: - Giriş ekranı: kullanıcı adı + şifre. - Şifreler hash’lenerek saklanır (düz metin şifre saklanmaz). - Pasif kullanıcı giriş yapamaz. - Menüler role göre görünür. - İşlem yapan kullanıcı, belgelerde (fatura, fiş, tahsilat) kaydedilir.
6. İşlevsel Gereksinimler (Modüller)
6.1 Kullanıcı Yönetimi (Yönetici)
Kullanıcı listeleme, ekleme, düzenleme, pasife alma, şifre sıfırlama.
6.2 Satış Elemanları
Satış elemanı ekleme, listeleme, pasife alma (ad soyad, telefon). Satış faturasında seçilir.
6.3 Cari Hesaplar
Müşteri/tedarikçi kartı açma ve düzenleme, bakiye görüntüleme, cari ekstre (hareket dökümü).
6.4 Stok Kartları
Stok kartı açma ve düzenleme (birim, KDV, alış/satış fiyatı, kritik stok), mevcut miktar görüntüleme.
6.5 Stok İşlemleri
Tüm stok hareketlerini (giriş/çıkış) listeleme. Ayrıca iki tür sayım fişi girilir: - Sayım Fazlası Fişi: sayımda fazla çıkan ürün için stoğu artırır (giriş). - Sayım Eksiği Fişi: sayımda eksik çıkan ürün için stoğu azaltır (çıkış).
Sayım fişleri yalnızca stoğu etkiler; cari hesaba işlemez.
6.6 Satış Faturası
Cari (müşteri) + satış elemanı + tarih + kalemler (stok, miktar, fiyat, KDV) girilir. Kayıtta stok çıkışı yapılır ve müşteri borçlandırılır.
6.7 Alış Faturası
Cari (tedarikçi) + tarih + kalemler girilir. Kayıtta stok girişi yapılır ve tedarikçiye borç oluşur (cari alacak).
6.8 Tahsilat / Ödeme
Müşteriden tahsilat veya tedarikçiye ödeme girilir (Nakit / Havale / Çek). Cari bakiye güncellenir.
6.9 Parametreler (Yönetici)
Firma bilgileri ve uygulama davranışını belirleyen parametreler (Bölüm 9).
6.10 Raporlar
- Cari bakiye listesi
- Cari ekstre (tek cari, tarih aralığı)
- Stok durum listesi
- Kritik stok listesi
- Stok hareketleri listesi
- Satış / alış faturası listesi
- Satış elemanına göre satış (opsiyonel)
- Tahsilat / ödeme listesi
7. Veri Modeli (Önerilen Tablo Yapısı)
11 tablo ile sade bir yapı önerilmektedir.
Kullanicilar

| Alan | Tip | Açıklama |
| Id | int, PK |  |
| KullaniciAdi | nvarchar(50) | Benzersiz |
| SifreHash | nvarchar(256) | Hash’lenmiş şifre |
| AdSoyad | nvarchar(100) |  |
| Rol | nvarchar(20) | Yönetici / Standart |
| Aktif | bit |  |

SatisElemanlari

| Alan | Tip | Açıklama |
| Id | int, PK |  |
| AdSoyad | nvarchar(100) |  |
| Telefon | nvarchar(20) |  |
| Aktif | bit |  |

Cariler

| Alan | Tip | Açıklama |
| Id | int, PK |  |
| CariKodu | nvarchar(20) | Benzersiz |
| Unvan | nvarchar(150) |  |
| CariTipi | tinyint | 1=Müşteri, 2=Tedarikçi, 3=Her ikisi |
| VergiDairesi | nvarchar(50) |  |
| VergiNo | nvarchar(20) |  |
| Telefon | nvarchar(20) |  |
| Adres | nvarchar(250) |  |
| Aktif | bit |  |

StokKartlari

| Alan | Tip | Açıklama |
| Id | int, PK |  |
| StokKodu | nvarchar(20) | Benzersiz |
| StokAdi | nvarchar(150) |  |
| Birim | nvarchar(10) | Adet, Kg, Lt… |
| KdvOrani | decimal(5,2) | Varsayılan parametreden gelebilir |
| AlisFiyati | decimal(18,2) |  |
| SatisFiyati | decimal(18,2) |  |
| KritikStok | decimal(18,2) | Uyarı seviyesi |
| Aktif | bit |  |

SatisFaturalari (Fatura başlığı)

| Alan | Tip | Açıklama |
| Id | int, PK |  |
| FaturaNo | nvarchar(20) | Benzersiz, otomatik |
| Tarih | date |  |
| CariId | int, FK → Cariler | Müşteri |
| SatisElemaniId | int, FK → SatisElemanlari |  |
| AraToplam | decimal(18,2) | KDV hariç |
| KdvToplam | decimal(18,2) |  |
| GenelToplam | decimal(18,2) |  |
| Aciklama | nvarchar(250) |  |
| KullaniciId | int, FK → Kullanicilar |  |
| OlusturmaTarihi | datetime |  |

SatisFaturaSatirlari (Fatura kalemleri)

| Alan | Tip | Açıklama |
| Id | int, PK |  |
| SatisFaturaId | int, FK → SatisFaturalari |  |
| StokId | int, FK → StokKartlari |  |
| Miktar | decimal(18,2) |  |
| BirimFiyat | decimal(18,2) |  |
| KdvOrani | decimal(5,2) |  |
| SatirTutari | decimal(18,2) | Miktar × BirimFiyat (KDV hariç) |

AlisFaturalari / AlisFaturaSatirlari
SatisFaturalari ve SatisFaturaSatirlari ile aynı yapıdadır (satış elemanı alanı yoktur; CariId burada tedarikçidir).
StokHareketler (Stok bakiyesinin tek kaynağı)

| Alan | Tip | Açıklama |
| Id | int, PK |  |
| StokId | int, FK → StokKartlari |  |
| Tarih | date |  |
| HareketTipi | nvarchar(20) | Satis / Alis / SayimFazlasi / SayimEksigi |
| Yon | nvarchar(10) | Giris / Cikis |
| Miktar | decimal(18,2) |  |
| BelgeNo | nvarchar(20) | İlgili fatura/fiş no |
| Aciklama | nvarchar(250) |  |
| KullaniciId | int, FK |  |

Satış, alış ve sayım fişleri hep bu tabloya yazar. “Stok işlemleri” ekranı bu tabloyu listeler.
CariHareketler (Cari bakiyenin tek kaynağı)

| Alan | Tip | Açıklama |
| Id | int, PK |  |
| CariId | int, FK → Cariler |  |
| Tarih | date |  |
| IslemTipi | nvarchar(20) | Satis / Alis / Tahsilat / Odeme |
| Borc | decimal(18,2) |  |
| Alacak | decimal(18,2) |  |
| OdemeTuru | nvarchar(20) | Nakit / Havale / Çek (yalnızca tahsilat/ödemede) |
| BelgeNo | nvarchar(20) |  |
| Aciklama | nvarchar(250) |  |
| KullaniciId | int, FK |  |

Parametreler (Anahtar–Değer)

| Alan | Tip | Açıklama |
| Id | int, PK |  |
| ParametreKodu | nvarchar(50) | Benzersiz |
| ParametreDegeri | nvarchar(250) |  |
| Aciklama | nvarchar(250) |  |

8. İş Kuralları

| İşlem | Stok | Cari |
| Satış Faturası | Çıkış (azalır) | Müşteri Borç |
| Alış Faturası | Giriş (artar) | Tedarikçi Alacak |
| Sayım Fazlası Fişi | Giriş (artar) | — |
| Sayım Eksiği Fişi | Çıkış (azalır) | — |
| Tahsilat | — | Müşteri Alacak (borcu azalır) |
| Ödeme | — | Tedarikçi Borç (borcumuz azalır) |

Diğer kurallar: - Cari bakiye = Σ Borç − Σ Alacak. Pozitif bakiye müşterinin bize borçlu olduğunu gösterir. - Satır tutarı = Miktar × Birim Fiyat (KDV hariç). Genel toplam = Ara toplam + KDV. - Sayım fişleri sadece stoğu etkiler, cari veya kasa hareketi oluşturmaz. - Negatif stok kontrolü parametreye bağlıdır (Bölüm 9). - Fatura ve sayım fişi numaraları otomatik ve sıralı üretilir (format parametreden alınır). - Bir belge silinir/iptal edilirse ilgili stok ve cari hareketleri geri alınmalıdır. - Zorunlu alan ve tip kontrolleri yapılır (boş cari, negatif miktar vb. engellenir).
9. Parametreler

| Parametre | Açıklama | Örnek Değer |
| Firma Unvanı | Rapor/fatura başlığı | ESBİ Bilişim Ltd. Şti. |
| Vergi Dairesi / No | Firma vergi bilgileri | Sakarya / 1234567890 |
| Varsayılan KDV Oranı | Yeni satırda öntanımlı | %20 |
| Para Birimi | Görüntüleme birimi | TL |
| Ondalık Basamak | Tutar hassasiyeti | 2 |
| Negatif Stok Kontrolü | Stok yetersizse davranış | Kapalı |
| Satış Fatura No Formatı | Otomatik numara şablonu | SAT-2025-0001 |
| Alış Fatura No Formatı | Otomatik numara şablonu | ALS-2025-0001 |
| Sayım Fazlası Fiş No Formatı | Otomatik numara şablonu | SF-2025-0001 |
| Sayım Eksiği Fiş No Formatı | Otomatik numara şablonu | SE-2025-0001 |
| Kritik Stok Uyarısı | Uyarı göster/gösterme | Açık |

10. Ekranlar
- Giriş (Login) ekranı
- Ana menü (role göre görünen menüler)
- Kullanıcı listesi / kullanıcı kartı
- Satış elemanı listesi / kartı
- Cari listesi / cari kartı / cari ekstre
- Stok listesi / stok kartı
- Stok hareketleri listesi
- Sayım Fazlası Fişi / Sayım Eksiği Fişi ekranı
- Satış faturası (başlık + kalem tablosu)
- Alış faturası (başlık + kalem tablosu)
- Tahsilat / ödeme ekranı
- Parametreler ekranı
- Rapor ekranları
11. Proje Aşamaları

| Hafta | İçerik |
| 1 | Veri tabanı tasarımı + tablolar, proje iskeleti, giriş + kullanıcı yönetimi + satış elemanı |
| 2 | Cari kartlar + stok kartları (CRUD) |
| 3 | Satış ve alış faturaları (stok & cari güncelleme) |
| 4 | Stok işlemleri + sayım fişleri + tahsilat/ödeme |
| 5 | Parametreler + raporlar + test ve teslim |

12. Teslim ve Kabul Kriterleri
Teslim edilecekler: - Kaynak kod (Git deposu) - Veri tabanı oluşturma script’i (.sql) - Örnek/test verisi - Kısa kullanım kılavuzu (1–2 sayfa)
Kabul kriterleri: - Tüm modüller çalışır durumda. - Kullanıcı girişi ve rol bazlı yetkilendirme çalışıyor. - İş kuralları doğru (stok ve cari bakiye tutarlı, sayım fişleri sadece stoğu etkiliyor). - Raporlar hareketlerle uyumlu sonuç veriyor. - Düz metin şifre saklanmıyor. - Temel hata ve zorunlu alan kontrolleri mevcut.
13. Geliştirme Fikirleri (Bonus, opsiyonel)
- Excel’e / PDF’e rapor aktarma
- Arama, filtreleme ve sayfalama
- Basit grafik (aylık satış, en çok satan ürünler)
- İşlem geçmişi (log) kaydı
- Basit yedekleme