# Mini Ön Muhasebe — Kısa Kullanım Kılavuzu

Bu kılavuz uygulamayı kullanacak kişiler içindir: hangi ekranın ne işe yaradığını ve günlük
işlemlerin hangi sırayla yapılacağını anlatır. Kurulum adımları için `README.md` dosyasına bakın.

## 1. Giriş ve roller

Uygulama tarayıcıdan açılır (`dotnet run` ile çalıştırıldığında `http://localhost:5029`). Giriş ekranında
kullanıcı adı ve şifre girilir. Pasife alınmış kullanıcı giriş yapamaz; açık bir oturumu varsa
bir sonraki tıklamada oturumu kapanır.

| Rol | Görebildiği ekranlar |
|---|---|
| **Yönetici** | Tüm ekranlar + Kullanıcılar + Parametreler |
| **Standart** | Cari, stok, satış/alış faturası, stok işlemleri, tahsilat/ödeme, raporlar |

Menü role göre oluşur; Standart kullanıcı yönetim ekranlarını görmez. Yaptığınız her fatura,
fiş ve tahsilat kaydına adınız işlenir.

## 2. İlk kurulum sırası

1. **Parametreler** (Yönetici): firma unvanı, vergi bilgisi, varsayılan KDV, belge numarası
   formatları ve negatif stok kontrolü ayarlanır.
2. **Satış elemanları**: satış faturasında seçilecek kişiler tanımlanır.
3. **Cari hesaplar**: müşteri ve tedarikçi kartları açılır. Cari tipi önemlidir:
   satış faturası ve tahsilat yalnızca *müşteri*, alış faturası ve ödeme yalnızca *tedarikçi*
   carilerine yapılabilir. "Müşteri + Tedarikçi" her ikisine de açıktır.
4. **Stok kartları**: ürün kodu, adı, birimi, KDV oranı, alış/satış fiyatı ve kritik stok
   seviyesi girilir. Stok miktarı elle girilmez; hareketlerden hesaplanır.

## 3. Günlük işlemler

**Alış faturası** — *Alış Faturası → Yeni*: tedarikçi ve tarih seçilir, kalemler eklenir
(ürün seçilince alış fiyatı ve KDV kendiliğinden gelir, değiştirilebilir). Kayıtta ürünlerin
stoğu artar, tedarikçiye borç oluşur. Fatura numarası otomatik verilir.

**Satış faturası** — *Satış Faturası → Yeni*: müşteri, satış elemanı ve tarih seçilir,
kalemler eklenir. Kayıtta stok azalır, müşteri borçlanır. Negatif stok kontrolü açıksa
stoğu yetmeyen ürün için fatura kaydedilmez ve ekranda mevcut miktar gösterilir.

**Tahsilat / ödeme** — *Tahsilat / Ödeme → + Tahsilat* ya da *+ Ödeme*: cari, tutar ve ödeme
türü (Nakit, Havale, Çek) seçilir. Tahsilat müşterinin borcunu, ödeme tedarikçiye olan
borcumuzu azaltır.

**Sayım fişi** — *Sayım Fişi*: fiziki sayımda fark çıkan ürün için kullanılır.
*Sayım fazlası* stoğu artırır, *sayım eksiği* azaltır. Ekran seçilen ürünün mevcut miktarını
ve fişten sonraki miktarı gösterir. Sayım fişleri cari hesaba işlemez.

Tutarlar ve miktarlar **virgülle** yazılır: `12,50`. Liste ekranlarındaki arama kutusu ve
açılır filtreler sayfayı yenilemeden süzer; tarih filtreleri için *Filtrele* düğmesine basılır.

## 4. Silme ve düzeltme

- **Fatura** detay ekranından silinir; faturanın stok ve cari hareketleri de geri alınır.
  Bir alış faturası silinince satılmış ürünün stoğu eksiye düşecekse (negatif stok kontrolü
  açıkken) silme engellenir ve nedeni gösterilir.
- **Sayım fişi** ve **tahsilat/ödeme** kendi listelerinden silinir. Fatura kaynaklı hareketler
  bu listelerden silinemez; ilgili fatura silinmelidir.
- **Cari, stok kartı, satış elemanı, kullanıcı**: hiç işlemi yoksa silinir; işlemi varsa geçmiş
  kayıtlar bozulmasın diye **pasife alınır**. Pasif kayıt yeni belgelerde seçilemez, eski
  belgelerde görünmeye devam eder.
- Yönetici kendi hesabını silemez, kendini pasife alamaz ve sistemde en az bir aktif yönetici
  kalmalıdır.

## 5. Raporlar

*Raporlar* menüsünden açılır; hepsi hareket kayıtlarından anlık hesaplanır ve **Yazdır**
düğmesiyle firma başlığı eklenmiş olarak yazdırılabilir.

| Rapor | İçerik |
|---|---|
| Cari bakiye listesi | Her carinin toplam borç, alacak ve bakiyesi. **(B)** cari bize borçlu, **(A)** biz cariye borçluyuz |
| Cari ekstre | Tek cari için tarih aralığındaki hareketler, devir ve yürüyen bakiye |
| Tahsilat / ödeme listesi | Dönemin tahsilat ve ödemeleri, ödeme türüne göre filtre |
| Stok durum listesi | Ürün bazında giriş, çıkış, mevcut miktar, seviye ve stok değeri |
| Kritik stok listesi | Kritik seviyeye düşmüş ürünler, eksik miktar ve tamamlama maliyeti |
| Stok hareketleri listesi | Satış, alış ve sayım fişlerinden oluşan tüm giriş/çıkışlar |
| Satış / alış faturası listesi | Dönemin faturaları ve toplamları |
| Satış elemanına göre satış | Eleman bazında fatura sayısı, ciro ve pay |

## 6. Parametreler (Yönetici)

| Parametre | Etkisi |
|---|---|
| Firma unvanı, vergi dairesi / no | Menü altında ve yazdırılan sayfaların başlığında görünür |
| Varsayılan KDV oranı | Yeni stok kartında ve yeni fatura satırında öntanımlı gelir |
| Para birimi | Tutarların yanında gösterilen birim |
| Ondalık basamak | Tutarların kaç basamakla hesaplanıp gösterileceği (0–2). Miktarlar ve birim fiyatlar etkilenmez |
| Negatif stok kontrolü | Açıkken stok yetersizse satış faturası ve sayım eksiği kaydedilmez |
| Kritik stok uyarısı | Açıkken panelde ve stok listesinde kritik seviye uyarısı gösterilir |
| Belge no formatları | `SAT-{yyyy}-{0000}` gibi: `{yyyy}` yıl, `{0000}` sıra numarası. Her belge türünün formatı farklı olmalıdır |
