# Ekran Sözleşmesi (Frontend ↔ Backend)

View'lerin hepsi yazıldı. Bu dosya, her ekranın çalışması için **controller tarafında** ne yazman
gerektiğini listeler. Her view dosyasının en üstünde aynı bilgi yorum olarak da duruyor.

## Genel kurallar

- **View klasör adı = controller adı.** `Views/Cari/Index.cshtml` → `CariController.Index()`.
- **Ondalık sayılar virgülle gelir** (`2,50`). Sunucu kültürü `tr-TR` olduğu için model binding bunu doğru okur.
  Kültürü değiştirirsen (ör. `UseRequestLocalization`) formlar da otomatik uyum sağlar; view'ler ayırıcıyı sunucudan okuyor.
- **Navigation property'ler formdan gelmez.** `Cari`, `Kullanici`, `StokKarti` gibi alanlar `null!` tanımlı olduğu için
  POST'ta `ModelState` hata verebilir → `ModelState.Remove("Cari")` vb. ya da modelde `?` yap.
- **Hesaplanan değerler formdan gelmez:** `SatirTutari`, `AraToplam`, `KdvToplam`, `GenelToplam`, `FaturaNo`, `BelgeNo`,
  `KullaniciId`, `OlusturmaTarihi` sunucuda atanır.
- Tüm POST formları tag helper ile yazıldı → **antiforgery token otomatik** eklenir (`[ValidateAntiForgeryToken]` kullanabilirsin).
- Başarı mesajı göstermek için `TempData["Mesaj"] = "..."` yeterli (Index/Detay/Edit ekranları gösterir).
- Liste ekranlarındaki **arama kutuları ve açılır filtreler tarayıcıda çalışır**, backend gerekmez.
  Sadece **tarih aralığı** (`baslangic`, `bitis`) sunucuya gider.

## Ekranlar

| # | Ekran | View | Controller / Action | Controller'ın hazırlaması gerekenler |
|---|---|---|---|---|
| 1 | Giriş | `Account/Login` | `AccountController` GET/POST `Login` | POST: `(string kullaniciAdi, string sifre, string? returnUrl)`; hata → `ModelState.AddModelError("", "...")`. Layout: `_AuthLayout` |
| – | Yetkisiz erişim | `Account/AccessDenied` | GET `AccessDenied` | — (cookie auth'un varsayılan yolu) |
| – | Çıkış | (layout'taki buton) | POST `Logout` | Oturumu kapat → `Login`'e yönlendir |
| 2 | Ana menü | `Home/Index` + `_Layout` | `HomeController.Index` | İsteğe bağlı `ViewData["KritikStokSayisi"]` (int). Menü: `User.IsInRole("Yönetici")` |
| 3 | Kullanıcı listesi | `Kullanici/Index` | GET `Index` | `List<Kullanici>` · **[Authorize(Roles = "Yönetici")]** |
| 3 | Kullanıcı kartı | `Kullanici/Create`, `Edit` | GET/POST `Create`, `Edit`; POST `SifreSifirla` | Create POST: `(Kullanici, string sifre, string sifreTekrar)` → hash'le. `SifreSifirla(int id, string yeniSifre, string yeniSifreTekrar)` |
| 4 | Satış elemanı listesi | `SatisElemani/Index` | GET `Index` | `List<SatisElemani>` |
| 4 | Satış elemanı kartı | `SatisElemani/Create`, `Edit` | GET/POST `Create`, `Edit` | — |
| 5 | Cari listesi | `Cari/Index` | GET `Index` | `List<Cari>` + `.Include(c => c.CariHareketleri)` (bakiye için) |
| 5 | Cari kartı | `Cari/Create`, `Edit` | GET/POST `Create`, `Edit` | Edit GET: `.Include(c => c.CariHareketleri)` (hesap özeti) |
| 5 | Cari ekstre | `Cari/Ekstre` | GET `Ekstre(int? id, DateTime? baslangic, DateTime? bitis)` | `Cari` + **tarih filtreli** `CariHareketleri`; `ViewData["Cariler"]`; isteğe bağlı `ViewData["DevirBakiye"]` (decimal). id yoksa `View(null)` |
| 6 | Stok listesi | `StokKarti/Index` | GET `Index` | `List<StokKarti>` + `.Include(s => s.StokHareketleri)` |
| 6 | Stok kartı | `StokKarti/Create`, `Edit` | GET/POST `Create`, `Edit` | Create GET: isteğe bağlı `ViewData["VarsayilanKdv"]`. Edit GET: `.Include(s => s.StokHareketleri)` |
| 7 | Stok hareketleri | `StokHareket/Index` | GET `Index(int? stokId, DateTime? baslangic, DateTime? bitis)` | `List<StokHareket>` + `.Include(StokKarti).Include(Kullanici)` |
| 8 | Sayım fazlası / eksiği fişi | `StokHareket/SayimFisi` | GET `SayimFisi(string? tip)`; POST `SayimFisi(StokHareket)` | `ViewData["Stoklar"]` (+ `Include(StokHareketleri)` mevcut miktar için). POST: `HareketTipi`'ne göre `Yon` ata, `BelgeNo` üret. **Cariye işlemez** |
| 9 | Satış faturası listesi | `SatisFaturasi/Index` | GET `Index(DateTime? baslangic, DateTime? bitis)` | `.Include(Cari).Include(SatisElemani).Include(Kullanici)` |
| 9 | Satış faturası (başlık + kalem) | `SatisFaturasi/Create` | GET/POST `Create` | `ViewData["Cariler"]` (tip 1/3), `ViewData["SatisElemanlari"]`, `ViewData["Stoklar"]`, isteğe bağlı `ViewData["YeniFaturaNo"]`. POST: kalemler `SatisFaturaSatirlari`'nda; **tek transaction**: fatura + StokHareket(Cikis) + CariHareket(Borc) |
| 9 | Satış faturası detay | `SatisFaturasi/Detay` | GET `Detay(int id)`; POST `Delete(int id)` | `.Include(...SatisFaturaSatirlari).ThenInclude(StokKarti)` + Cari, SatisElemani, Kullanici. Delete: hareketleri de geri al |
| 10 | Alış faturası listesi | `AlisFaturasi/Index` | GET `Index(DateTime? baslangic, DateTime? bitis)` | `.Include(Cari).Include(Kullanici)` |
| 10 | Alış faturası (başlık + kalem) | `AlisFaturasi/Create` | GET/POST `Create` | `ViewData["Cariler"]` (tip 2/3), `ViewData["Stoklar"]`. POST: StokHareket(Giris) + CariHareket(Alacak) |
| 10 | Alış faturası detay | `AlisFaturasi/Detay` | GET `Detay(int id)`; POST `Delete(int id)` | `.Include(...AlisFaturaSatirlari).ThenInclude(StokKarti)` + Cari, Kullanici |
| 11 | Tahsilat / ödeme listesi | `TahsilatOdeme/Index` | GET `Index(DateTime? baslangic, DateTime? bitis)`; POST `Delete(int id)` | `List<CariHareket>` (yalnızca Tahsilat/Odeme) + `.Include(Cari).Include(Kullanici)` |
| 11 | Tahsilat / ödeme girişi | `TahsilatOdeme/Create` | GET `Create(string? tip)`; POST `Create(CariHareket, decimal tutar)` | `ViewData["Cariler"]` (**tüm** aktif cariler; ekran tipe göre süzer). Tahsilat → `Alacak = tutar`, Ödeme → `Borc = tutar` |
| 12 | Parametreler | `Parametre/Index` | GET/POST `Index` | `List<Parametre>`. POST: `(List<Parametre> parametreler)` → Id'ye göre yalnızca `ParametreDegeri` güncelle · **[Authorize(Roles = "Yönetici")]** |
| 13 | Raporlar ana sayfa | `Rapor/Index` | GET `Index` | — |
| 13 | Cari bakiye listesi | `Rapor/CariBakiye` | GET `CariBakiye` | `List<Cari>` + `.Include(CariHareketleri)` |
| 13 | Stok durum listesi | `Rapor/StokDurum` | GET `StokDurum` | `List<StokKarti>` + `.Include(StokHareketleri)` |
| 13 | Kritik stok listesi | `Rapor/KritikStok` | GET `KritikStok` | Aktif stoklar + `.Include(StokHareketleri)` (süzme view'de) |
| 13 | Satış elemanına göre satış | `Rapor/SatisElemaniSatis` | GET `SatisElemaniSatis(DateTime? baslangic, DateTime? bitis)` | `List<SatisElemani>` + tarih filtreli `.Include(SatisFaturalari)` |

Cari ekstre, stok hareketleri, fatura listeleri ve tahsilat/ödeme listesi raporlar sayfasından da açılır
(ayrı rapor view'i yok, aynı ekranlar kullanılıyor).

## Paylaşılan dosyalar

| Dosya | Görevi |
|---|---|
| `Views/Shared/_Layout.cshtml` | Kenar menü (rol bazlı "Yönetim" bölümü), üst çubuk, kullanıcı / çıkış |
| `Views/Shared/_AuthLayout.cshtml` | Menüsüz giriş sayfası düzeni |
| `Views/Shared/_FaturaKalemleri.cshtml` | Satış + alış faturası ortak kalem tablosu |
| `Views/Shared/_ValidationScriptsPartial.cshtml` | İstemci doğrulaması + virgüllü sayı desteği |
| `Views/*/_Form.cshtml` | Her modülün Create/Edit ortak form alanları |
| `wwwroot/js/site.js` | Menü, tablo arama/filtre, silme onayı, yazdır, açık/kapalı anahtarı |
| `wwwroot/js/fatura-form.js` | Dinamik kalem ekle/sil, yeniden numaralandırma, anlık toplam |
| `wwwroot/css/site.css` | Tüm tasarım sistemi |
