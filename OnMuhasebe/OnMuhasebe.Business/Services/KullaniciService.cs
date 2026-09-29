using Microsoft.EntityFrameworkCore;
using OnMuhasebe.Business.Services.IServices;
using OnMuhasebe.DataAccess;
using OnMuhasebe.Models;
using OnMuhasebe.Utility;

namespace OnMuhasebe.Business.Services
{
    public class KullaniciService : IKullaniciService
    {
        private readonly ApplicationDbContext _context;
        public KullaniciService(ApplicationDbContext context)
        {
            _context = context;
        }

        // Kayıtlı olmayan kullanıcı adıyla girişte karşılaştırılan geçerli biçimli bir özet.
        private static readonly Lazy<string> SahteSifreHash = new(() => SifreYardimcisi.HashOlustur(Guid.NewGuid().ToString()));

        public async Task<List<Kullanici>> GetAllKullanicilarAsync()
        {
            return await _context.Kullanicilar.ToListAsync();
        }

        public async Task<Kullanici?> GetKullaniciByIdAsync(int id)
        {
            return await _context.Kullanicilar.FindAsync(id);
        }

        public async Task<Kullanici?> DogrulaAsync(string kullaniciAdi, string sifre)
        {
            var kullanici = await _context.Kullanicilar
                .FirstOrDefaultAsync(k => k.KullaniciAdi == kullaniciAdi);

            // Kullanıcı bulunamasa da şifre doğrulaması aynı sürede çalıştırılır; böylece yanıt
            // süresinden kullanıcı adının kayıtlı olup olmadığı anlaşılamaz.
            var sifreDogru = SifreYardimcisi.Dogrula(sifre, kullanici?.SifreHash ?? SahteSifreHash.Value);

            // Pasif kullanıcı giriş yapamaz.
            return kullanici != null && kullanici.Aktif && sifreDogru ? kullanici : null;
        }

        public async Task<bool> IsKullaniciNameUniqueAsync(string kullaniciAdi, int? excludeId = null)
        {
            var normalizedName = kullaniciAdi.ToLower();
            return !await _context.Kullanicilar.AnyAsync(k => k.KullaniciAdi.ToLower() == normalizedName && (!excludeId.HasValue || k.Id != excludeId.Value));
        }

        public async Task<Kullanici> CreateKullaniciAsync(Kullanici kullanici, string sifre)
        {
            if (!await IsKullaniciNameUniqueAsync(kullanici.KullaniciAdi))
            {
                throw new AlanHatasiException(nameof(Kullanici.KullaniciAdi), "Bu kullanıcı adı zaten kayıtlı.");
            }

            kullanici.Id = 0; // Id veritabanında üretilir
            kullanici.SifreHash = SifreYardimcisi.HashOlustur(sifre);
            _context.Kullanicilar.Add(kullanici);
            await _context.SaveChangesAsync();
            return kullanici;
        }

        public async Task UpdateKullaniciAsync(Kullanici kullanici, int islemYapanId)
        {
            var existingKullanici = await _context.Kullanicilar.FindAsync(kullanici.Id);
            if (existingKullanici == null)
            {
                throw new KeyNotFoundException("Kullanıcı bulunamadı.");
            }

            // Aktif bir yönetici pasife alınıyor ya da rolü düşürülüyorsa yönetimsiz kalınmamalı.
            var yoneticilikKalkiyor = AktifYonetici(existingKullanici)
                && (!kullanici.Aktif || kullanici.Rol != Sabitler.RolYonetici);
            if (yoneticilikKalkiyor)
            {
                if (existingKullanici.Id == islemYapanId)
                {
                    throw new InvalidOperationException("Kendi yönetici yetkinizi kaldıramaz, kendinizi pasife alamazsınız.");
                }
                await SonYoneticiDegilseDevamAsync(existingKullanici.Id);
            }

            if (!await IsKullaniciNameUniqueAsync(kullanici.KullaniciAdi, kullanici.Id))
            {
                throw new AlanHatasiException(nameof(Kullanici.KullaniciAdi), "Bu kullanıcı adı zaten kayıtlı.");
            }

            existingKullanici.KullaniciAdi = kullanici.KullaniciAdi;
            existingKullanici.AdSoyad = kullanici.AdSoyad;
            existingKullanici.Rol = kullanici.Rol;
            existingKullanici.Aktif = kullanici.Aktif;

            await _context.SaveChangesAsync();
        }

        public async Task<bool> DeleteKullaniciAsync(int id, int islemYapanId)
        {
            var kullanici = await _context.Kullanicilar.FindAsync(id);
            if (kullanici == null)
            {
                throw new KeyNotFoundException("Kullanıcı bulunamadı.");
            }

            if (id == islemYapanId)
            {
                throw new InvalidOperationException("Kendi hesabınızı silemez, pasife alamazsınız.");
            }
            if (AktifYonetici(kullanici))
            {
                await SonYoneticiDegilseDevamAsync(id);
            }

            var kayitliIslemVar = await _context.SatisFaturalari.AnyAsync(f => f.KullaniciId == id)
                || await _context.AlisFaturalari.AnyAsync(f => f.KullaniciId == id)
                || await _context.StokHareketler.AnyAsync(h => h.KullaniciId == id)
                || await _context.CariHareketler.AnyAsync(h => h.KullaniciId == id);

            if (kayitliIslemVar)
            {
                kullanici.Aktif = false;
            }
            else
            {
                _context.Kullanicilar.Remove(kullanici);
            }

            await _context.SaveChangesAsync();
            return !kayitliIslemVar;
        }

        private static bool AktifYonetici(Kullanici kullanici) =>
            kullanici.Aktif && kullanici.Rol == Sabitler.RolYonetici;

        // Kullanıcı ve parametre yönetimi yalnızca yöneticiye açık; son yönetici kaldırılırsa kimse yönetemez.
        private async Task SonYoneticiDegilseDevamAsync(int haricTutulanId)
        {
            var baskaYoneticiVar = await _context.Kullanicilar
                .AnyAsync(k => k.Id != haricTutulanId && k.Aktif && k.Rol == Sabitler.RolYonetici);
            if (!baskaYoneticiVar)
            {
                throw new InvalidOperationException("Sistemde en az bir aktif yönetici kalmalıdır.");
            }
        }

        public async Task SifreSifirlaAsync(int id, string yeniSifre)
        {
            var kullanici = await _context.Kullanicilar.FindAsync(id);
            if (kullanici == null)
            {
                throw new KeyNotFoundException("Kullanıcı bulunamadı.");
            }
            kullanici.SifreHash = SifreYardimcisi.HashOlustur(yeniSifre);
            await _context.SaveChangesAsync();
        }

    }
}
