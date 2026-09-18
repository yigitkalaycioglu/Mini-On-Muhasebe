using Microsoft.EntityFrameworkCore;
using OnMuhasebe.Business.Services.IServices;
using OnMuhasebe.DataAccess;
using OnMuhasebe.Models;

namespace OnMuhasebe.Business.Services
{
    public class SatisElemaniService : ISatisElemaniService
    {
        private readonly ApplicationDbContext _context;
        public SatisElemaniService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<SatisElemani>> GetAllSatisElemanlariAsync()
        {
            return await _context.SatisElemanlari.ToListAsync();
        }

        public async Task<SatisElemani?> GetSatisElemaniByIdAsync(int id)
        {
            return await _context.SatisElemanlari.FindAsync(id);
        }

        public async Task<bool> IsSatisElemaniNameUniqueAsync(string adSoyad, int? excludeId = null)
        {
            var normalizedName = adSoyad.ToLower();
            return !await _context.SatisElemanlari.AnyAsync(e => e.AdSoyad.ToLower() == normalizedName && (!excludeId.HasValue || e.Id != excludeId.Value));
        }

        public async Task<SatisElemani> CreateSatisElemaniAsync(SatisElemani satisElemani)
        {
            if (!await IsSatisElemaniNameUniqueAsync(satisElemani.AdSoyad))
            {
                throw new InvalidOperationException("Bu ad ve soyad zaten kayıtlı.");
            }

            if (await _context.SatisElemanlari.AnyAsync(e => e.Telefon == satisElemani.Telefon))
            {
                throw new InvalidOperationException("Bu telefon numarası zaten kayıtlı.");
            }

            _context.SatisElemanlari.Add(satisElemani);
            await _context.SaveChangesAsync();
            return satisElemani;
        }

        public async Task DeleteSatisElemaniAsync(int id)
        {
            var satisElemani = await _context.SatisElemanlari.FindAsync(id);
            if (satisElemani == null)
            {
                throw new KeyNotFoundException("Satış elemanı bulunamadı.");
            }

            if (await _context.SatisFaturalari.AnyAsync(f => f.SatisElemaniId == id))
            {
                satisElemani.Aktif = false;
            }
            else
            {
                _context.SatisElemanlari.Remove(satisElemani);
            }

            await _context.SaveChangesAsync();
        }

        public async Task UpdateSatisElemaniAsync(SatisElemani satisElemani)
        {
            var existingSatisElemani = await _context.SatisElemanlari.FindAsync(satisElemani.Id);
            if (existingSatisElemani == null)
            {
                throw new KeyNotFoundException("Satış elemanı bulunamadı.");
            }

            if (!await IsSatisElemaniNameUniqueAsync(satisElemani.AdSoyad, satisElemani.Id))
            {
                throw new InvalidOperationException("Bu ad ve soyad zaten kayıtlı.");
            }

            if (await _context.SatisElemanlari.AnyAsync(e => e.Telefon == satisElemani.Telefon && e.Id != satisElemani.Id))
            {
                throw new InvalidOperationException("Bu telefon numarası zaten kayıtlı.");
            }

            existingSatisElemani.AdSoyad = satisElemani.AdSoyad;
            existingSatisElemani.Telefon = satisElemani.Telefon;
            existingSatisElemani.Aktif = satisElemani.Aktif;

            await _context.SaveChangesAsync();
        }
    }
}
