using OnMuhasebe.Domain.Entities;

namespace OnMuhasebe.Application.Models
{
    // Liste ve rapor ekranları için veritabanında hesaplanan özetler.
    // Hareketler belleğe alınmaz; toplamlar SQL'de SUM ile bulunur ve kartla birlikte gelir.

    public class StokBakiyesi
    {
        public StokKarti Stok { get; init; } = null!;
        public decimal Giris { get; init; }
        public decimal Cikis { get; init; }
        public decimal Mevcut { get; init; }
    }

    public class CariBakiyesi
    {
        public Cari Cari { get; init; } = null!;
        public decimal Borc { get; init; }
        public decimal Alacak { get; init; }
        public decimal Bakiye { get; init; }
    }

    public class SatisElemaniCirosu
    {
        public SatisElemani Eleman { get; init; } = null!;
        public int FaturaSayisi { get; init; }
        public decimal AraToplam { get; init; }
        public decimal KdvToplam { get; init; }
        public decimal GenelToplam { get; init; }
    }
}
