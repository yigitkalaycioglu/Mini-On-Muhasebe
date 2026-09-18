using Microsoft.EntityFrameworkCore;
using OnMuhasebe.Business.Services.IServices;
using OnMuhasebe.DataAccess;
using OnMuhasebe.Models;

namespace OnMuhasebe.Business.Services
{
    public class StokKartiService : IStokKartiService
    {
        private readonly ApplicationDbContext _context;
        public StokKartiService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<StokKarti>> GetAllStokKartlariAsync()
        {
            return await _context.StokKartlari
                .Include(s => s.StokHareketleri)
                .ToListAsync();
        }

        public async Task<StokKarti?> GetStokKartiByIdAsync(int id)
        {
            return await _context.StokKartlari.FindAsync(id);
        }

        public async Task<StokKarti> CreateStokKartiAsync(StokKarti stokKarti)
        {
            if (await _context.StokKartlari.AnyAsync(s => s.StokKodu == stokKarti.StokKodu))
            {
                throw new InvalidOperationException("Bu stok kodu zaten kayıtlı.");
            }

            _context.StokKartlari.Add(stokKarti);
            await _context.SaveChangesAsync();
            return stokKarti;
        }

        public async Task UpdateStokKartiAsync(StokKarti stokKarti)
        {
            var existingStokKarti = await _context.StokKartlari.FindAsync(stokKarti.Id);
            if (existingStokKarti == null)
            {
                throw new KeyNotFoundException("Stok kartı bulunamadı.");
            }

            if (await _context.StokKartlari.AnyAsync(s => s.StokKodu == stokKarti.StokKodu && s.Id != stokKarti.Id))
            {
                throw new InvalidOperationException("Bu stok kodu zaten kayıtlı.");
            }

            _context.Entry(existingStokKarti).CurrentValues.SetValues(stokKarti);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteStokKartiAsync(int id)
        {
            var stokKarti = await _context.StokKartlari.FindAsync(id);
            if (stokKarti == null)
            {
                throw new KeyNotFoundException("Stok kartı bulunamadı.");
            }

            var kayitliIslemVar = await _context.SatisFaturaSatirlari.AnyAsync(s => s.StokId == id)
                || await _context.AlisFaturaSatirlari.AnyAsync(s => s.StokId == id)
                || await _context.StokHareketler.AnyAsync(h => h.StokId == id);

            if (kayitliIslemVar)
            {
                stokKarti.Aktif = false;
            }
            else
            {
                _context.StokKartlari.Remove(stokKarti);
            }

            await _context.SaveChangesAsync();
        }
    }
}
