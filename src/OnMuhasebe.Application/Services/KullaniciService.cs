using Microsoft.EntityFrameworkCore;
using OnMuhasebe.Application.Abstractions;
using OnMuhasebe.Application.Dtos;
using OnMuhasebe.Application.Exceptions;
using OnMuhasebe.Domain;
using OnMuhasebe.Domain.Entities;

namespace OnMuhasebe.Application.Services
{
    public class KullaniciService : IKullaniciService
    {
        private readonly IApplicationDbContext _context;
        private readonly ISifreHashleyici _sifreHashleyici;
        public KullaniciService(IApplicationDbContext context, ISifreHashleyici sifreHashleyici)
        {
            _context = context;
            _sifreHashleyici = sifreHashleyici;
        }

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
            var sifreDogru = _sifreHashleyici.Dogrula(sifre, kullanici?.SifreHash);

            // Pasif kullanıcı giriş yapamaz.
            return kullanici != null && kullanici.Aktif && sifreDogru ? kullanici : null;
        }

        public async Task<bool> IsKullaniciNameUniqueAsync(string kullaniciAdi, int? excludeId = null)
        {
            var normalizedName = kullaniciAdi.ToLower();
            return !await _context.Kullanicilar.AnyAsync(k => k.KullaniciAdi.ToLower() == normalizedName && (!excludeId.HasValue || k.Id != excludeId.Value));
        }

        public async Task<Kullanici> CreateKullaniciAsync(KullaniciDto kullanici, string sifre)
        {
            if (!await IsKullaniciNameUniqueAsync(kullanici.KullaniciAdi))
            {
                throw new IsKuraliException(nameof(KullaniciDto.KullaniciAdi), "Bu kullanıcı adı zaten kayıtlı.");
            }

            var yeni = new Kullanici { SifreHash = _sifreHashleyici.HashOlustur(sifre) };
            kullanici.ApplyTo(yeni);
            _context.Kullanicilar.Add(yeni);
            await _context.SaveChangesAsync();
            return yeni;
        }

        public async Task UpdateKullaniciAsync(int id, KullaniciDto kullanici, int islemYapanId)
        {
            var existingKullanici = await _context.Kullanicilar.FindAsync(id);
            if (existingKullanici == null)
            {
                throw new KayitBulunamadiException("Kullanıcı bulunamadı.");
            }

            // Aktif bir yönetici pasife alınıyor ya da rolü düşürülüyorsa yönetimsiz kalınmamalı.
            var yoneticilikKalkiyor = AktifYonetici(existingKullanici)
                && (!kullanici.Aktif || kullanici.Rol != Sabitler.RolYonetici);
            if (yoneticilikKalkiyor)
            {
                if (existingKullanici.Id == islemYapanId)
                {
                    throw new IsKuraliException("Kendi yönetici yetkinizi kaldıramaz, kendinizi pasife alamazsınız.");
                }
                await SonYoneticiDegilseDevamAsync(existingKullanici.Id);
            }

            if (!await IsKullaniciNameUniqueAsync(kullanici.KullaniciAdi, id))
            {
                throw new IsKuraliException(nameof(KullaniciDto.KullaniciAdi), "Bu kullanıcı adı zaten kayıtlı.");
            }

            kullanici.ApplyTo(existingKullanici);

            await _context.SaveChangesAsync();
        }

        public async Task<bool> DeleteKullaniciAsync(int id, int islemYapanId)
        {
            var kullanici = await _context.Kullanicilar.FindAsync(id);
            if (kullanici == null)
            {
                throw new KayitBulunamadiException("Kullanıcı bulunamadı.");
            }

            if (id == islemYapanId)
            {
                throw new IsKuraliException("Kendi hesabınızı silemez, pasife alamazsınız.");
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
                throw new IsKuraliException("Sistemde en az bir aktif yönetici kalmalıdır.");
            }
        }

        public async Task SifreSifirlaAsync(int id, string yeniSifre)
        {
            var kullanici = await _context.Kullanicilar.FindAsync(id);
            if (kullanici == null)
            {
                throw new KayitBulunamadiException("Kullanıcı bulunamadı.");
            }
            kullanici.SifreHash = _sifreHashleyici.HashOlustur(yeniSifre);
            await _context.SaveChangesAsync();
        }

    }
}
