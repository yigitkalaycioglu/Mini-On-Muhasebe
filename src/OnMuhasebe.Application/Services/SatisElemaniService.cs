using Microsoft.EntityFrameworkCore;
using OnMuhasebe.Application.Abstractions;
using OnMuhasebe.Application.Dtos;
using OnMuhasebe.Application.Exceptions;
using OnMuhasebe.Domain.Entities;

namespace OnMuhasebe.Application.Services
{
    public class SatisElemaniService : ISatisElemaniService
    {
        private readonly IApplicationDbContext _context;
        public SatisElemaniService(IApplicationDbContext context)
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

        public async Task<SatisElemani> CreateSatisElemaniAsync(SatisElemaniDto satisElemani)
        {
            if (!await IsSatisElemaniNameUniqueAsync(satisElemani.AdSoyad))
            {
                throw new IsKuraliException(nameof(SatisElemaniDto.AdSoyad), "Bu ad ve soyad zaten kayıtlı.");
            }

            if (await _context.SatisElemanlari.AnyAsync(e => e.Telefon == satisElemani.Telefon))
            {
                throw new IsKuraliException(nameof(SatisElemaniDto.Telefon), "Bu telefon numarası zaten kayıtlı.");
            }

            var yeni = new SatisElemani();
            satisElemani.ApplyTo(yeni);
            _context.SatisElemanlari.Add(yeni);
            await _context.SaveChangesAsync();
            return yeni;
        }

        public async Task<bool> DeleteSatisElemaniAsync(int id)
        {
            var satisElemani = await _context.SatisElemanlari.FindAsync(id);
            if (satisElemani == null)
            {
                throw new KayitBulunamadiException("Satış elemanı bulunamadı.");
            }

            var faturasiVar = await _context.SatisFaturalari.AnyAsync(f => f.SatisElemaniId == id);
            if (faturasiVar)
            {
                satisElemani.Aktif = false;
            }
            else
            {
                _context.SatisElemanlari.Remove(satisElemani);
            }

            await _context.SaveChangesAsync();
            return !faturasiVar;
        }

        public async Task UpdateSatisElemaniAsync(int id, SatisElemaniDto satisElemani)
        {
            var existingSatisElemani = await _context.SatisElemanlari.FindAsync(id);
            if (existingSatisElemani == null)
            {
                throw new KayitBulunamadiException("Satış elemanı bulunamadı.");
            }

            if (!await IsSatisElemaniNameUniqueAsync(satisElemani.AdSoyad, id))
            {
                throw new IsKuraliException(nameof(SatisElemaniDto.AdSoyad), "Bu ad ve soyad zaten kayıtlı.");
            }

            if (await _context.SatisElemanlari.AnyAsync(e => e.Telefon == satisElemani.Telefon && e.Id != id))
            {
                throw new IsKuraliException(nameof(SatisElemaniDto.Telefon), "Bu telefon numarası zaten kayıtlı.");
            }

            satisElemani.ApplyTo(existingSatisElemani);

            await _context.SaveChangesAsync();
        }
    }
}
