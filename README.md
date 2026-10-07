# Mini Ön Muhasebe

ASP.NET Core MVC ile yazılmış, stok ve cari takibi yapan mini ön muhasebe uygulaması.
Staj projesi olarak, proje dökümanındaki haftalık plana göre geliştirildi.
Ekranların kullanımı için: [Kısa Kullanım Kılavuzu](docs/KULLANIM_KILAVUZU.md)

## Modüller

| Hafta | Modül |
|---|---|
| 1 | Giriş + rol bazlı yetkilendirme, kullanıcı yönetimi, satış elemanları |
| 2 | Cari hesaplar (bakiye, ekstre), stok kartları |
| 3 | Satış ve alış faturaları (stok ve cari hareketleri tek transaction'da) |
| 4 | Stok hareketleri, sayım fazlası/eksiği fişleri, tahsilat/ödeme |
| 5 | Parametreler, raporlar (cari bakiye, stok durum, kritik stok, satış elemanına göre satış) |

## Teknolojiler

- .NET 10 / ASP.NET Core MVC (Areas: `Customer`)
- Entity Framework Core 10, Code First, 5 migration (kolon tipleri dökümanın 7. bölümüyle birebir)
- SQL Server (`localhost\MSSQLSERVER01`, veritabanı: `OnMuhasebe`)
- Cookie Authentication + rol bazlı yetkilendirme (PBKDF2 parola özeti)
- Bootstrap 5

## Katman yapısı

```
OnMuhasebe.Models      → POCO entity sınıfları (11 tablo)
OnMuhasebe.DataAccess  → DbContext, migration'lar
OnMuhasebe.Business    → Servisler + interface'ler, Sabitler.cs, rapor özetleri
OnMuhasebe.Utility     → SifreYardimcisi (PBKDF2)
OnMuhasebeWeb          → Controller, View, ViewModel, filtre ve oturum doğrulama
```

Bağımlılık yönü tek taraflı: `Web → Business → DataAccess → Models`.
Controller doğrudan `DbContext` kullanmaz, her zaman servis interface'i üzerinden çalışır (DI).

## Kurulum

Gereksinimler: .NET 10 SDK ve SQL Server. Aşağıdaki komutlar `localhost\MSSQLSERVER01`
adlı örneği kullanır; SQL Server'ınız farklı bir adla kuruluysa (ör. `localhost` ya da
`.\SQLEXPRESS`) komutlardaki sunucu adını ve `src/OnMuhasebe.Web/appsettings.json`
içindeki `DefaultConnection` bağlantı cümlesini ona göre değiştirin.

```bash
# 1. Veritabanını oluştur
# (-f i:65001: Türkçe karakterlerin bozulmaması için dosyayı UTF-8 okur)
sqlcmd -S "localhost\MSSQLSERVER01" -E -f i:65001 -i database/create_database.sql

# 2. Örnek veriyi yükle (opsiyonel)
sqlcmd -S "localhost\MSSQLSERVER01" -E -f i:65001 -i database/seed_data.sql

# 3. Çalıştır → http://localhost:5029
dotnet run --project src/OnMuhasebe.Web
```

Örnek veriyle gelen kullanıcılar (yalnızca test amaçlı, ikisinin de parolası `123456`):

| Kullanıcı adı | Rol |
|---|---|
| `admin` | Yönetici |
| `ayse.yilmaz` | Standart |

---

# Geliştirme sürecinde karşılaşılan zorluklar

Bu bölüm, proje boyunca takılınan noktaları ve nasıl çözüldüğünü anlatır.

## 1. Faturalar hiçbir şekilde kaydedilemiyordu

Sorun: Fatura formu gönderildiğinde `ModelState.IsValid` hep `false` dönüyordu, ama
ekranda görünür bir hata mesajı yoktu.

Sebep: Projede `<Nullable>enable</Nullable>` açık. Bu durumda
`public string FaturaNo { get; set; }` gibi nullable olmayan referans tipler MVC tarafından
örtük olarak `[Required]` sayılıyor. `FaturaNo` sunucuda üretiliyor;
`Cari`, `Kullanici`, `SatisElemani` ise navigation property; hiçbiri formdan gelmiyor,
dolayısıyla hepsi "boş" kabul edilip doğrulamayı düşürüyordu.

Çözüm: Formdan gelmeyen alanlar POST action'ında doğrulamadan çıkarıldı:

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

Sorun: Veritabanında hareket olmasına rağmen stok miktarı ve cari bakiye sütunları
0 çıkıyordu. Stok kartı düzenleme ekranı ayrıca yersiz "Kritik seviyede" uyarısı veriyordu.

Sebep: Miktar ve bakiye tabloda saklanmıyor; `StokHareketler` / `CariHareketler`
üzerinden hesaplanıyor. Sorgularda `Include` eksik olduğu için koleksiyonlar boş geliyor,
boş listenin toplamı da 0 oluyordu.

Çözüm: İlgili tüm sorgulara `Include` eklendi:

```csharp
.Include(s => s.StokHareketleri)
```

Sadece liste metotlarına değil, `GetStokKartiByIdAsync` / `GetCariByIdAsync` gibi tekil
getirme metotlarına da eklenmesi gerekti. Düzenleme ekranındaki yanlış kritik uyarısının
sebebi buydu.

## 3. Aynı formülün altı ayrı yerde tekrar etmesi

Sorun: Stok miktarı formülü (giriş toplamı − çıkış toplamı) altı farklı view'de,
cari bakiye formülü (borç − alacak) yine altı ayrı yerde elle yazılmıştı. `"Giris"` gibi
sabit metinler her yerde tekrar ediyordu. Kritik stok eşiğini `<` yerine `<=` yapmak
istendiğinde altı dosyayı bulmak gerekiyordu; biri atlanırsa iki ekran farklı sayı
gösteriyordu.

Çözüm üç aşamada oldu:

1. Formüller servis katmanına taşındı, sabit metinler `Sabitler.cs` içinde toplandı.
2. Ancak hesaplar `static` yapılmıştı (view'den çağırmak kolay olsun diye), oysa sınıfın
   geri kalanı DI ile çalışıyordu. Aynı sınıfta iki farklı çağırma stili oluştu.
3. Hesaplar `IStokKartiService` / `ICariService` üzerinde instance metot haline
   getirildi. Böylece view onlara erişemez oldu; kural yazıyla değil derleyiciyle zorlanır
   hale geldi.

Bir istisna korundu: `Etki` ifadesi `static readonly Expression<Func<StokHareket, decimal>>`
olarak kaldı, çünkü `SumAsync` içinde kullanılıp SQL'e çevriliyor; toplama bellekte
değil veritabanında yapılıyor.

## 4. View'lerin iş mantığı içermesi (ViewModel'e geçiş)

Sorun: View'ler entity alıyordu (`@model List<StokKarti>`) ve hesabı kendisi yapıyordu:

```cshtml
decimal Mevcut(StokKarti s) => s.StokHareketleri.Sum(h => h.Yon == "Giris" ? h.Miktar : -h.Miktar);
var kritikSayisi = Model.Count(s => s.Aktif && Mevcut(s) <= s.KritikStok);
```

Çözüm: Ekrana özel ViewModel sınıfları oluşturuldu (`StokViewModels.cs`,
`CariViewModels.cs`). Controller servisi çağırıp hazır veriyi view'e veriyor; view hiçbir
hesap yapmıyor:

```csharp
Satirlar = bakiyeler.Select(b => new StokSatiriViewModel {
    Stok   = b.Stok,
    Mevcut = b.Mevcut,
    Kritik = uyariAcik && _stokKartiService.KritikSeviyede(b)
}).ToList()
```

`_ViewImports.cshtml` içinden `OnMuhasebe.Business.Services` import'u kaldırıldı; böylece
view artık servis katmanını göremiyor.

Form ekranları (`Edit`, `Create`, `SayimFisi`) bilerek entity almaya devam ediyor, çünkü
`asp-for` model binding'inin entity'ye bağlanması gerekiyor. Hesaplanmış özet bilgiler
onlara `ViewData["Ozet"]` ile gidiyor.

## 5. Giriş sistemi: Identity yerine kendi çözümümüz

Sorun: Bir eğitim videosundan alınan ASP.NET Core Identity kurulumu
(`AddIdentity<IdentityUser, IdentityRole>`, `IdentityDbContext`) projeye eklenmişti.

Sebep: Identity veritabanına yedi ek tablo ekliyor ve kendi `IdentityUser` sınıfını
dayatıyor. Proje dökümanı ise 11 tablolu bir şema ve kendi `Kullanici` modelimizi
istiyordu, ikisi çakışıyordu.

Çözüm: Identity kaldırıldı; mevcut `Kullanici` tablosu üzerinde cookie authentication
kuruldu. Parolalar için `SifreYardimcisi` yazıldı:

- PBKDF2-SHA256, 16 byte rastgele salt, 600.000 iterasyon, 32 byte çıktı
- Saklama formatı: `iterasyon.saltBase64.hashBase64`
- Karşılaştırma `CryptographicOperations.FixedTimeEquals` ile (timing attack'a karşı)

SHA256 yerine PBKDF2 seçildi: SHA256 hızlı olduğu için kaba kuvvet saldırısına açık,
PBKDF2 ise iterasyon sayısıyla kasıtlı olarak yavaşlatılabiliyor.

Bu değişikliğin yan etkisi: `seed_data.sql` içindeki parola özetleri hâlâ SHA256
formatındaydı, dolayısıyla PBKDF2'ye geçince kimse giriş yapamaz oldu. Özetler gerçek
`SifreYardimcisi` çalıştırılarak yeniden üretildi; hem seed dosyası hem canlı veritabanı
güncellendi.

## 6. Örnek veride negatif stok (−2)

Sorun: `seed_data.sql` yüklendiğinde bir ürünün stoğu −2 çıkıyordu.

Sebep: Örnek satış faturası, o ürüne ait alış hareketinden daha fazla çıkış yazıyordu.

Çözüm: Eksik alış hareketi (`SF-2026-0002`, 6 adet) seed dosyasına eklendi. Ayrıca
servis katmanındaki stok yeterlilik kontrolü, aynı üründen birden fazla satır içeren
faturaları da doğru hesaplaması için `GroupBy` ile yeniden yazıldı. Aksi halde iki satırın
her biri tek tek yeterli görünüp toplamda stok eksiye düşebiliyordu.

## 7. Fatura silindiğinde stok ve cari geri alınmıyordu

Çözüm: Fatura silinirken `BelgeNo` ve hareket türü üzerinden ilgili `StokHareket` ve
`CariHareket` kayıtları bulunup siliniyor. Fatura ve hareketler tek bir `SaveChangesAsync()`
çağrısında kaydediliyor; EF Core bunu tek transaction olarak çalıştırdığı için ya hepsi
yazılıyor ya hiçbiri. Yarım kalmış fatura oluşamıyor.

Sonradan fark edilen bir durum: alış faturası silinince girişler geri alındığı için, ürün bu
arada satıldıysa stok eksiye düşebiliyordu. Negatif stok kontrolü açıkken bu silme artık
engelleniyor ve kullanıcıya hangi ürünün eksiye düşeceği gösteriliyor.

## 8. Virgüllü tutarlar sunucunun diline bağlıydı

Sorun: Formlarda tutarlar `12,50` biçiminde giriliyor. Model binding bu metni
sunucunun kültürüne göre sayıya çevirir. Geliştirme bilgisayarı Türkçe olduğu için sorun
görünmüyordu; İngilizce bir sunucuda aynı değer `1250` olarak kaydedilirdi.

Çözüm: `Program.cs` içinde `UseRequestLocalization` ile uygulama kültürü `tr-TR` olarak
sabitlendi. Model binding'in İngilizce hata mesajları da (ör. *"The value 'abc' is not
valid."*) Türkçe mesajlarla değiştirildi.

## 9. Pasife alınan kullanıcının açık oturumu

Sorun: Kullanıcı pasife alındığında yeniden giriş yapamıyordu, ama zaten açık olan
oturumu 8 saatlik kayan süre boyunca eski yetkileriyle çalışmaya devam ediyordu. Rolü
değiştirilen kullanıcı için de aynısı geçerliydi.

Çözüm: Cookie authentication'ın `OnValidatePrincipal` olayında her istekte kullanıcı
veritabanından okunuyor; kullanıcı silinmiş, pasif ya da adı/rolü değişmişse oturum
kapatılıyor. Ayrıca yöneticinin kendini silmesi/pasife alması ve son aktif yöneticinin
kaldırılması engellendi. Aksi halde kullanıcı ve parametre yönetimi yapacak kimse kalmazdı.

## 10. Liste ekranları bütün hareketleri belleğe çekiyordu

Sorun: 2. maddedeki `Include` çözümü doğru sonuç veriyordu ama stok ve cari listeleri
her açılışta *tüm* hareket kayıtlarını belleğe alıp C# tarafında topluyordu. Hareket sayısı
arttıkça sayfa yavaşlar ve bellek kullanımı büyürdü.

Çözüm: Liste ve rapor ekranları için toplamlar veritabanında hesaplanıyor. Aynı `Etki`
ifadesi alt sorgu içinde kullanılarak formül tek yerde kaldı:

```csharp
.Select(s => new StokBakiyesi {
    Stok   = s,
    Mevcut = s.StokHareketleri.AsQueryable().Sum(Etki)   // SQL'de SUM
})
```

Tek kaydın düzenleme ekranında (hareket sayısı az) `Include` ile yükleme korundu.

## 11. Veritabanı kolon tipleri dökümandan farklıydı

Sorun: Proje dökümanı tekrar baştan sona kontrol edilince, tablo ve kolon adları doğru
olsa da 18 kolonun tipinin 7. bölümdeki tablodan farklı olduğu görüldü. Tarih alanları `date`
yerine `datetime2`, KDV oranları `decimal(5,2)` yerine `decimal(18,2)`, belge numarası ve
hareket tipi gibi alanlar `nvarchar(20)` yerine `nvarchar(max)`/`nvarchar(450)` olmuştu.

Sebep: EF Core, tipi belirtilmeyen alanlara kendi varsayılanını verir: `DateTime` →
`datetime2`, `decimal` → `decimal(18,2)`, uzunluğu verilmemiş `string` → `nvarchar(max)`
(indeksli olanlar `nvarchar(450)`). Formdan girilen alanlarda `[StringLength]` olduğu için
sorun yoktu; sunucunun atadığı alanlar (belge no, hareket tipi, şifre özeti) açıkta kalmıştı.

Çözüm: `ApplicationDbContext` içinde tipler dökümandaki tabloya göre tanımlandı
(`HasColumnType("date")`, `HasPrecision(5, 2)`, `HasMaxLength(20)`...) ve yeni bir migration
eklendi. Aynı migration'da cari, stok, satış elemanı ve kullanıcıdan belgelere giden ilişkiler
`Cascade` yerine `Restrict` yapıldı: artık bir ana kayıt veritabanından silinerek faturaları
ve hareketleri, yani muhasebe geçmişi, kaybolamaz (fatura → kalemler ilişkisi `Cascade` kaldı).
Şema, dökümandaki 11 tablo / 86 kolon / 13 ilişki ile alan alan karşılaştırılarak doğrulandı.

---

## Doğrulama yöntemi

Her önemli değişiklikten sonra uygulama ayağa kaldırılıp gerçek HTTP istekleriyle sürüldü
(giriş → antiforgery token → form POST); ekranda görünen değerler doğrudan SQL sorgularıyla
karşılaştırıldı. Test verisi her seferinde temizlenip veritabanı eski sayımlarına döndürüldü.

Son doğrulama (örnek veriyle): stok değerleri 8 / 0 / 5 / 4 / 50 ve cari bakiyeler
280 / 1.440 / −900 / −800 hem liste ekranlarında hem raporlarda veritabanıyla birebir eşleşti;
sayım fişi, tahsilat/ödeme ve fatura oluştur → sil akışları stoğu ve bakiyeyi doğru değiştirip
geri aldı; yetersiz stokta satış ve sayım eksiği reddedildi.

## Yapılmayanlar

- Dökümandaki bonus maddeler (Excel/PDF aktarma, grafik, işlem geçmişi, yedekleme) yapılmadı.
  Raporlar tarayıcıdan yazdırılabiliyor.
- Form ekranları ViewModel yerine entity kullanıyor (bilinçli tercih, 4. maddede açıklandı).
- Tahsilat/ödeme belge numaraları için dökümanda parametre tanımlı olmadığından sabit
  `TAH-{yyyy}-{0000}` / `ODE-{yyyy}-{0000}` formatı kullanılıyor.

## Lisans

MIT
