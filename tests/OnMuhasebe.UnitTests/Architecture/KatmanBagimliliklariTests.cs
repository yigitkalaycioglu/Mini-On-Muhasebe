using System.ComponentModel.DataAnnotations;
using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnMuhasebe.Application.Abstractions;
using OnMuhasebe.Application.Dtos;
using OnMuhasebe.Domain.Entities;
using OnMuhasebe.Infrastructure.Persistence;
using OnMuhasebe.Web.Controllers;

namespace OnMuhasebe.UnitTests.Architecture
{
    /// <summary>
    /// Clean Architecture kuralları derlenmiş kod üzerinden denetlenir: içteki katman dıştakini tanımaz,
    /// controller'lar veritabanına doğrudan erişmez, entity'ler form doğrulaması taşımaz, formlar adresteki kaydı değiştiremez.
    /// </summary>
    public class KatmanBagimliliklariTests
    {
        private static readonly Assembly Domain = typeof(Cari).Assembly;
        private static readonly Assembly Application = typeof(IApplicationDbContext).Assembly;
        private static readonly Assembly Infrastructure = typeof(ApplicationDbContext).Assembly;
        private static readonly Assembly Web = typeof(HomeController).Assembly;

        private static List<string> Referanslar(Assembly assembly) =>
            assembly.GetReferencedAssemblies().Select(r => r.Name!).ToList();

        [Fact]
        public void Domain_HicbirKatmanaVeCerceveyeBagliDegil()
        {
            var referanslar = Referanslar(Domain);

            Assert.DoesNotContain(referanslar, r => r.StartsWith("OnMuhasebe."));
            Assert.DoesNotContain(referanslar, r => r.StartsWith("Microsoft.EntityFrameworkCore") || r.StartsWith("Microsoft.AspNetCore"));
        }

        [Fact]
        public void Application_YalnizcaDomaineBagli()
        {
            var referanslar = Referanslar(Application);

            Assert.Equal(new[] { "OnMuhasebe.Domain" }, referanslar.Where(r => r.StartsWith("OnMuhasebe.")));
            Assert.DoesNotContain(referanslar, r => r.StartsWith("Microsoft.EntityFrameworkCore.SqlServer") || r.StartsWith("Microsoft.AspNetCore"));
        }

        [Fact]
        public void Infrastructure_WebKatmaninaBagliDegil()
        {
            Assert.DoesNotContain("OnMuhasebe.Web", Referanslar(Infrastructure));
        }

        [Fact]
        public void Controllerlar_VeritabaninaDogrudanErismez()
        {
            var ihlaller = Web.GetTypes()
                .Where(t => t.Namespace == typeof(HomeController).Namespace)
                .SelectMany(t => t.GetConstructors().SelectMany(c => c.GetParameters()).Select(p => (Tip: t, Parametre: p.ParameterType)))
                .Where(x => x.Parametre.Assembly == Infrastructure
                    || typeof(DbContext).IsAssignableFrom(x.Parametre)
                    || x.Parametre == typeof(IApplicationDbContext))
                .Select(x => $"{x.Tip.Name}({x.Parametre.Name})")
                .ToList();

            Assert.Empty(ihlaller);
        }

        [Fact]
        public void Entityler_DogrulamaAttributeTasimaz()
        {
            var ihlaller = Domain.GetTypes()
                .Where(t => t.Namespace == typeof(Cari).Namespace)
                .SelectMany(t => t.GetProperties())
                .Where(p => p.GetCustomAttributes<ValidationAttribute>().Any())
                .Select(p => $"{p.DeclaringType!.Name}.{p.Name}")
                .ToList();

            Assert.Empty(ihlaller);
        }

        [Fact]
        public void FormDtolari_IdVeEntityTasimaz()
        {
            // Düzenlenecek kaydın Id'si adresten gelir; formdan gelen Id ya da entity ile başka kayıt değiştirilemez.
            // Parametreler ekranı istisna: tek formda birden çok kayıt düzenlendiği için satırlar Id taşır.
            var ihlaller = Application.GetTypes()
                .Where(t => t.Namespace == typeof(CariDto).Namespace && t != typeof(ParametreDegeriDto))
                .SelectMany(t => t.GetProperties())
                .Where(p => p.Name == "Id" || p.PropertyType.Assembly == Domain)
                .Select(p => $"{p.DeclaringType!.Name}.{p.Name}")
                .ToList();

            Assert.Empty(ihlaller);
        }

        [Fact]
        public void PostActionlari_KayitNumarasiniYalnizcaAdrestenAlir()
        {
            // Model bağlama form alanlarına rotadan önce bakar: [FromRoute] olmasa forma eklenen bir "Id" alanıyla
            // /Cari/Edit/1 adresine gönderilen form 2 numaralı kaydı değiştirirdi.
            var idParametreleri = Web.GetTypes()
                .Where(t => t.Namespace == typeof(HomeController).Namespace)
                .SelectMany(t => t.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly))
                .Where(m => m.GetCustomAttribute<HttpPostAttribute>() != null)
                .SelectMany(m => m.GetParameters().Where(p => p.Name == "id").Select(p => (Metot: m, Parametre: p)))
                .ToList();

            Assert.NotEmpty(idParametreleri);
            Assert.Empty(idParametreleri
                .Where(x => x.Parametre.GetCustomAttribute<FromRouteAttribute>() == null)
                .Select(x => $"{x.Metot.DeclaringType!.Name}.{x.Metot.Name}"));
        }
    }
}
