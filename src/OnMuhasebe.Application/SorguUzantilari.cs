using System.Linq.Expressions;

namespace OnMuhasebe.Business
{
    internal static class SorguUzantilari
    {
        /// <summary>
        /// Tarih aralığı filtresi: başlangıç günü ve bitiş günü dahildir.
        /// Bitiş için "ertesi günün başlangıcından küçük" koşulu kullanılır; böylece saat bilgisi olan kayıtlar da kaçmaz.
        /// Örnek: query.TarihAraliginda(f => f.Tarih, baslangic, bitis)
        /// </summary>
        public static IQueryable<T> TarihAraliginda<T>(this IQueryable<T> query, Expression<Func<T, DateTime>> tarih, DateTime? baslangic, DateTime? bitis)
        {
            if (baslangic.HasValue)
            {
                query = query.Where(Kosul(tarih, Expression.GreaterThanOrEqual, baslangic.Value.Date));
            }
            if (bitis.HasValue)
            {
                query = query.Where(Kosul(tarih, Expression.LessThan, bitis.Value.Date.AddDays(1)));
            }
            return query;
        }

        // "kayit.Tarih >= deger" gibi bir koşul kurar. Değer closure üzerinden verildiği için
        // EF Core onu SQL'e sabit olarak değil parametre olarak geçirir.
        private static Expression<Func<T, bool>> Kosul<T>(
            Expression<Func<T, DateTime>> tarih,
            Func<Expression, Expression, BinaryExpression> karsilastirma,
            DateTime deger)
        {
            Expression<Func<DateTime>> degerIfadesi = () => deger;
            return Expression.Lambda<Func<T, bool>>(karsilastirma(tarih.Body, degerIfadesi.Body), tarih.Parameters);
        }
    }
}
