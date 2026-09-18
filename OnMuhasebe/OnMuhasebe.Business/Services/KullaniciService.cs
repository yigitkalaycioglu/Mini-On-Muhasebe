using Microsoft.EntityFrameworkCore;
using OnMuhasebe.Business.Services.IServices;
using OnMuhasebe.DataAccess;
using OnMuhasebe.Models;

namespace OnMuhasebe.Business.Services
{
    public class KullaniciService : IKullaniciService
    {
        private readonly ApplicationDbContext _context;
        public KullaniciService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<Kullanici>> GetAllKullanicilarAsync()
        {
            return await _context.Kullanicilar.ToListAsync();
        }

        public async Task<Kullanici?> GetKullaniciByIdAsync(int id)
        {
            return await _context.Kullanicilar.FindAsync(id);
        }

        public async Task<bool> IsKullaniciNameUniqueAsync(string kullaniciAdi, int? excludeId = null)
        {
            var normalizedName = kullaniciAdi.ToLower();
            return !await _context.Kullanicilar.AnyAsync(k => k.KullaniciAdi.ToLower() == normalizedName && (!excludeId.HasValue || k.Id != excludeId.Value));
        }

        public async Task<Kullanici> CreateKullaniciAsync(Kullanici kullanici)
        {
            if (!await IsKullaniciNameUniqueAsync(kullanici.KullaniciAdi))
            {
                throw new InvalidOperationException("Bu kullanıcı adı zaten kayıtlı.");
            }

            _context.Kullanicilar.Add(kullanici);
            await _context.SaveChangesAsync();
            return kullanici;
        }

        public async Task UpdateKullaniciAsync(Kullanici kullanici)
        {
            var existingKullanici = await _context.Kullanicilar.FindAsync(kullanici.Id);
            if (existingKullanici == null)
            {
                throw new KeyNotFoundException("Kullanıcı bulunamadı.");
            }

            if (!await IsKullaniciNameUniqueAsync(kullanici.KullaniciAdi, kullanici.Id))
            {
                throw new InvalidOperationException("Bu kullanıcı adı zaten kayıtlı.");
            }

            existingKullanici.KullaniciAdi = kullanici.KullaniciAdi;
            existingKullanici.AdSoyad = kullanici.AdSoyad;
            existingKullanici.Rol = kullanici.Rol;
            existingKullanici.Aktif = kullanici.Aktif;

            // Şifre formdan gelmez; yalnızca controller yeni bir hash atadıysa güncellenir.
            if (!string.IsNullOrEmpty(kullanici.SifreHash))
            {
                existingKullanici.SifreHash = kullanici.SifreHash;
            }

            await _context.SaveChangesAsync();
        }

        public async Task DeleteKullaniciAsync(int id)
        {
            var kullanici = await _context.Kullanicilar.FindAsync(id);
            if (kullanici == null)
            {
                throw new KeyNotFoundException("Kullanıcı bulunamadı.");
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
        }
    }
}
