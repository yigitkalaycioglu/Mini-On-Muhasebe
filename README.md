# Mini Ön Muhasebe

ASP.NET Core MVC ile yazılmış, stok ve cari takibi yapan mini ön muhasebe uygulaması.
Staj projesi olarak, proje dökümanındaki haftalık plana göre geliştirildi.

## Teknolojiler

- .NET 10 / ASP.NET Core MVC (Areas: `Customer`)
- Entity Framework Core 10 — Code First, 4 migration
- SQL Server (`localhost\MSSQLSERVER01`, veritabanı: `OnMuhasebe`)
- Cookie Authentication + rol bazlı yetkilendirme (PBKDF2 parola özeti)
- Bootstrap 5

## Katman yapısı

```
OnMuhasebe.Models      → POCO entity sınıfları (11 tablo)
OnMuhasebe.DataAccess  → DbContext, migration'lar
OnMuhasebe.Business    → Servisler + interface'ler, Sabitler.cs
OnMuhasebe.Utility     → SifreYardimcisi (PBKDF2)
OnMuhasebeWeb          → Controller, View, ViewModel
```

Bağımlılık yönü tek taraflı: `Web → Business → DataAccess → Models`.
Controller doğrudan `DbContext` kullanmaz, her zaman servis interface'i üzerinden çalışır (DI).

## Kurulum

```bash
# 1. Veritabanını oluştur
sqlcmd -S "localhost\MSSQLSERVER01" -i OnMuhasebe/Database/create_database.sql

# 2. Örnek veriyi yükle (opsiyonel)
sqlcmd -S "localhost\MSSQLSERVER01" -d OnMuhasebe -i OnMuhasebe/Database/seed_data.sql

# 3. Çalıştır
cd OnMuhasebe/OnMuhasebeWeb
dotnet run
```

Örnek kullanıcılar (parola: `123456`) `seed_data.sql` içinde tanımlı, yalnızca test amaçlıdır.

---

# Geliştirme sürecinde karşılaşılan zorluklar

Bu bölüm, proje boyunca takılınan noktaları ve nasıl çözüldüğünü anlatır.

## 1. Faturalar hiçbir şekilde kaydedilemiyordu

**Sorun:** Fatura formu gönderildiğinde `ModelState.IsValid` hep `false` dönüyordu, ama
ekranda görünür bir hata mesajı yoktu.

**Sebep:** Projede `<Nullable>enable</Nullable>` açık. Bu durumda
`public string FaturaNo { get; set; }` gibi nullable olmayan referans tipler MVC tarafından
**örtük olarak `[Required]`** sayılıyor. `FaturaNo` sunucuda üretiliyor;
`Cari`, `Kullanici`, `SatisElemani` ise navigation property — hiçbiri formdan gelmiyor,
dolayısıyla hepsi "boş" kabul edilip doğrulamayı düşürüyordu.

**Çözüm:** Formdan gelmeyen alanlar POST action'ında doğrulamadan çıkarıldı:

```csharp
ModelState.Remove(nameof(SatisFaturasi.FaturaNo));
ModelState.Remove(nameof(SatisFaturasi.Cari));
for (var i = 0; i < satisFaturasi.SatisFaturaSatirlari.Count; i++)
{
    ModelState.Remove($"SatisFaturaSatirlari[{i}].StokKarti");
}
```

Sorunun kaynağını bulmak için ayrı bir deneme projesinde gerçek model sınıfları kullanılarak
model binding test edildi. `Remove` sonrası hatalı verinin (boş cari, negatif miktar) hâlâ
reddedildiği ayrıca doğrulandı.

## 2. Stok ve cari listeleri hep 0 gösteriyordu

**Sorun:** Veritabanında hareket olmasına rağmen stok miktarı ve cari bakiye sütunları
0 çıkıyordu. Stok kartı düzenleme ekranı ayrıca yersiz "Kritik seviyede" uyarısı veriyordu.

**Sebep:** Miktar ve bakiye tabloda saklanmıyor; `StokHareketler` / `CariHareketler`
üzerinden hesaplanıyor. Sorgularda `Include` eksik olduğu için koleksiyonlar boş geliyor,
boş listenin toplamı da 0 oluyordu.

**Çözüm:** İlgili tüm sorgulara `Include` eklendi:

```csharp
.Include(s => s.StokHareketleri)
```

Sadece liste metotlarına değil, `GetStokKartiByIdAsync` / `GetCariByIdAsync` gibi tekil
getirme metotlarına da eklenmesi gerekti — düzenleme ekranındaki yanlış kritik uyarısının
sebebi buydu.

## 3. Aynı formülün altı ayrı yerde tekrar etmesi

**Sorun:** Stok miktarı formülü (giriş toplamı − çıkış toplamı) altı farklı view'de,
cari bakiye formülü (borç − alacak) yine altı ayrı yerde elle yazılmıştı. `"Giris"` gibi
sabit metinler her yerde tekrar ediyordu. Kritik stok eşiğini `<` yerine `<=` yapmak
istendiğinde altı dosyayı bulmak gerekiyordu; biri atlanırsa iki ekran farklı sayı
gösteriyordu.

**Çözüm — üç aşamada:**

1. Formüller servis katmanına taşındı, sabit metinler `Sabitler.cs` içinde toplandı.
2. Ancak hesaplar `static` yapılmıştı (view'den çağırmak kolay olsun diye), oysa sınıfın
   geri kalanı DI ile çalışıyordu — aynı sınıfta iki farklı çağırma stili oluştu.
3. Hesaplar `IStokKartiService` / `ICariService` üzerinde **instance metot** haline
   getirildi. Böylece view onlara erişemez oldu; kural yazıyla değil derleyiciyle zorlanır
   hale geldi.

Bir istisna korundu: `Etki` ifadesi `static readonly Expression<Func<StokHareket, decimal>>`
olarak kaldı, çünkü `SumAsync` içinde kullanılıp **SQL'e çevriliyor** — toplama bellekte
değil veritabanında yapılıyor.

## 4. View'lerin iş mantığı içermesi (ViewModel'e geçiş)

**Sorun:** View'ler entity alıyordu (`@model List<StokKarti>`) ve hesabı kendisi yapıyordu:

```cshtml
decimal Mevcut(StokKarti s) => s.StokHareketleri.Sum(h => h.Yon == "Giris" ? h.Miktar : -h.Miktar);
var kritikSayisi = Model.Count(s => s.Aktif && Mevcut(s) <= s.KritikStok);
```

**Çözüm:** Ekrana özel ViewModel sınıfları oluşturuldu (`StokViewModels.cs`,
`CariViewModels.cs`). Controller servisi çağırıp hazır veriyi view'e veriyor; view hiçbir
hesap yapmıyor:

```csharp
Satirlar = stokKartlari.Select(s => new StokSatiriViewModel {
    Stok   = s,
    Mevcut = _stokKartiService.MevcutMiktar(s),
    Kritik = _stokKartiService.KritikSeviyede(s)
}).ToList()
```

`_ViewImports.cshtml` içinden `OnMuhasebe.Business.Services` import'u kaldırıldı; böylece
view artık servis katmanını göremiyor.

Form ekranları (`Edit`, `Create`, `SayimFisi`) bilerek entity almaya devam ediyor —
`asp-for` model binding'inin entity'ye bağlanması gerekiyor. Hesaplanmış özet bilgiler
onlara `ViewData["Ozet"]` ile gidiyor.

## 5. Giriş sistemi: Identity yerine kendi çözümümüz

**Sorun:** Bir eğitim videosundan alınan ASP.NET Core Identity kurulumu
(`AddIdentity<IdentityUser, IdentityRole>`, `IdentityDbContext`) projeye eklenmişti.

**Sebep:** Identity veritabanına yedi ek tablo ekliyor ve kendi `IdentityUser` sınıfını
dayatıyor. Proje dökümanı ise 11 tablolu bir şema ve kendi `Kullanici` modelimizi
istiyordu — ikisi çakışıyordu.

**Çözüm:** Identity kaldırıldı; mevcut `Kullanici` tablosu üzerinde cookie authentication
kuruldu. Parolalar için `SifreYardimcisi` yazıldı:

- PBKDF2-SHA256, 16 byte rastgele salt, 600.000 iterasyon, 32 byte çıktı
- Saklama formatı: `iterasyon.saltBase64.hashBase64`
- Karşılaştırma `CryptographicOperations.FixedTimeEquals` ile (timing attack'a karşı)

SHA256 yerine PBKDF2 seçildi: SHA256 hızlı olduğu için kaba kuvvet saldırısına açık,
PBKDF2 ise iterasyon sayısıyla kasıtlı olarak yavaşlatılabiliyor.

**Bu değişikliğin yan etkisi:** `seed_data.sql` içindeki parola özetleri hâlâ SHA256
formatındaydı, dolayısıyla PBKDF2'ye geçince kimse giriş yapamaz oldu. Özetler gerçek
`SifreYardimcisi` çalıştırılarak yeniden üretildi; hem seed dosyası hem canlı veritabanı
güncellendi.

## 6. Yanlış rol adı yüzünden kilitlenen ekranlar

**Sorun:** Fatura ekranlarında `[Authorize(Roles = "Yönetici, SatisElemani")]` yazıyordu.
`SatisElemani` bir **rol değil**, ayrı bir tablo (satış temsilcisi kaydı). Sistemde yalnızca
iki rol var: `Yönetici` ve `Standart`. Sonuç olarak Standart kullanıcılar fatura kesemiyordu.

**Çözüm:** Attribute sade `[Authorize]` yapıldı. Rol adları artık elle yazılmıyor,
`Sabitler.RolYonetici` / `Sabitler.RolStandart` sabitlerinden okunuyor.

## 7. Katman ihlalleri

Servis dosyalarına zaman zaman controller kodu (`ModelState`, `TempData`,
`RedirectToAction`) yazıldı. Kural netleştirildi:

- **Servis** iş kuralını doğrular, ihlalde `InvalidOperationException` fırlatır — HTTP'yi bilmez.
- **Controller** bu exception'ı yakalar, `ModelState.AddModelError` ile ekrana çevirir.

Benzer şekilde senkron `Find` kullanımları `FindAsync` ile değiştirilip tüm servis metotları
async hale getirildi.

## 8. Örnek veride negatif stok (−2)

**Sorun:** `seed_data.sql` yüklendiğinde bir ürünün stoğu −2 çıkıyordu.

**Sebep:** Örnek satış faturası, o ürüne ait alış hareketinden daha fazla çıkış yazıyordu.

**Çözüm:** Eksik alış hareketi (`SF-2026-0002`, 6 adet) seed dosyasına eklendi. Ayrıca
servis katmanındaki stok yeterlilik kontrolü, **aynı üründen birden fazla satır** içeren
faturaları da doğru hesaplaması için `GroupBy` ile yeniden yazıldı — aksi halde iki satırın
her biri tek tek yeterli görünüp toplamda stok eksiye düşebiliyordu.

## 9. Fatura silindiğinde stok ve cari geri alınmıyordu

**Çözüm:** Fatura silinirken `BelgeNo` üzerinden ilgili `StokHareket` ve `CariHareket`
kayıtları bulunup siliniyor. Fatura ve hareketler tek bir `SaveChangesAsync()` çağrısında
kaydediliyor; EF Core bunu **tek transaction** olarak çalıştırdığı için ya hepsi yazılıyor
ya hiçbiri. Yarım kalmış fatura oluşamıyor.

## 10. Rapor kısayollarının yanlış sayfaya gitmesi

Raporlar sayfasındaki bazı kısayollar, henüz yazılmamış rapor action'ları yerine fatura
oluşturma ekranına yönlendiriyordu. Yanıltıcı olmaması için bu kısayollar "yakında"
etiketiyle devre dışı bırakıldı.

## 11. Tutarsız isimlendirme

`GetAllKullanicarAsync` gibi yazım hataları ve metot adı tutarsızlıkları vardı. Proje
genelinde tarandı; tüm servis metotları `Get/Create/Update/Delete + Varlık + Async`
kalıbına oturtuldu.

---

## Doğrulama yöntemi

Her önemli değişiklikten sonra uygulama ayağa kaldırılıp gerçek HTTP istekleriyle sürüldü
(giriş → antiforgery token → form POST); ekranda görünen değerler doğrudan SQL sorgularıyla
karşılaştırıldı. Test verisi her seferinde temizlenip veritabanı eski sayımlarına döndürüldü.

Son doğrulama: 12 ekran HTTP 200; stok değerleri 28 / 650 / 55 / 4 / 50 ve cari bakiyeler
280 / 1.440 / 800 / 7.950 veritabanıyla birebir eşleşti; fatura oluştur → sil akışı stoğu
50 → 47 → 50, cari bakiyeyi 280 → 370 → 280 yaptı; yetersiz stokta fatura reddedildi.

## Bilinen eksikler

- Haftalık planın 4–5. hafta maddeleri (bazı rapor action'ları, kullanım kılavuzu)
- Form ekranları ViewModel yerine entity kullanıyor (bilinçli tercih, yukarıda açıklandı)
