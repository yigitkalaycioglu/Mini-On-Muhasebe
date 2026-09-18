// ============================================================
// Mini Ön Muhasebe — genel arayüz davranışları
// ============================================================

// ----- Küçük ekranlarda kenar menüyü aç/kapat -----
(function () {
    const sidebar = document.getElementById('sidebar');
    const toggle = document.getElementById('sidebarToggle');
    const backdrop = document.getElementById('sidebarBackdrop');

    if (!sidebar || !toggle || !backdrop) return;

    function menuyuAc() {
        sidebar.classList.add('is-open');
        backdrop.classList.add('is-visible');
    }

    function menuyuKapat() {
        sidebar.classList.remove('is-open');
        backdrop.classList.remove('is-visible');
    }

    toggle.addEventListener('click', function () {
        if (sidebar.classList.contains('is-open')) {
            menuyuKapat();
        } else {
            menuyuAc();
        }
    });

    backdrop.addEventListener('click', menuyuKapat);

    document.addEventListener('keydown', function (e) {
        if (e.key === 'Escape') menuyuKapat();
    });
})();

// ----- Tablo içinde anlık arama / filtre -----
// Kullanım:
//   <input data-tablo-arama="#tabloId">                     → satır metninde arar
//   <select data-tablo-secim="#tabloId" data-alan="tip">     → tr[data-tip] değerine göre süzer
//   <span data-tablo-sayac="#tabloId"></span>                → görünen satır sayısı
(function () {
    const trKucuk = (s) => (s || '').toLocaleLowerCase('tr-TR');

    function tabloyuSuz(tabloSecici) {
        const tablo = document.querySelector(tabloSecici);
        if (!tablo) return;

        const aramaKutusu = document.querySelector('[data-tablo-arama="' + tabloSecici + '"]');
        const secimler = document.querySelectorAll('[data-tablo-secim="' + tabloSecici + '"]');
        const aranan = aramaKutusu ? trKucuk(aramaKutusu.value.trim()) : '';

        let gorunen = 0;
        tablo.querySelectorAll('tbody tr').forEach(function (tr) {
            let uygun = !aranan || trKucuk(tr.textContent).includes(aranan);

            secimler.forEach(function (sel) {
                if (sel.value && tr.dataset[sel.dataset.alan] !== sel.value) uygun = false;
            });

            tr.hidden = !uygun;
            if (uygun) gorunen++;
        });

        const sayac = document.querySelector('[data-tablo-sayac="' + tabloSecici + '"]');
        if (sayac) sayac.textContent = gorunen;
    }

    document.querySelectorAll('[data-tablo-arama]').forEach(function (el) {
        el.addEventListener('input', () => tabloyuSuz(el.dataset.tabloArama));
    });

    document.querySelectorAll('[data-tablo-secim]').forEach(function (el) {
        el.addEventListener('change', () => tabloyuSuz(el.dataset.tabloSecim));
    });
})();

// ----- Onay isteyen formlar -----
// Kullanım: <form data-onay="Bu kayıt silinsin mi?">
document.addEventListener('submit', function (e) {
    const mesaj = e.target.dataset && e.target.dataset.onay;
    if (mesaj && !window.confirm(mesaj)) {
        e.preventDefault();
    }
});

// ----- Yazdır butonu -----
// Kullanım: <button type="button" data-yazdir>
document.addEventListener('click', function (e) {
    if (e.target.closest('[data-yazdir]')) {
        window.print();
    }
});

// ----- Açık/Kapalı anahtarını gizli alana yansıt -----
// Kullanım: <input type="checkbox" data-anahtar-hedef="#gizliAlanId"
//                  data-acik="Acik" data-kapali="Kapali">
document.querySelectorAll('[data-anahtar-hedef]').forEach(function (kutu) {
    const hedef = document.querySelector(kutu.dataset.anahtarHedef);
    if (!hedef) return;

    kutu.addEventListener('change', function () {
        hedef.value = kutu.checked ? kutu.dataset.acik : kutu.dataset.kapali;
    });
});

// ----- TempData toast bildirimi -----
(function () {
    function toastGoster() {
        const alan = document.querySelector('[data-toast-message]');
        if (!alan || !window.toastr) return;

        const mesaj = alan.dataset.toastMessage;
        const tip = (alan.dataset.toastType || 'success').toLowerCase();

        toastr.options = {
            closeButton: true,
            progressBar: true,
            positionClass: 'toast-top-right',
            timeOut: 3500,
            extendedTimeOut: 1000,
            newestOnTop: true
        };

        const bildirim = toastr[tip] || toastr.success;
        bildirim(mesaj);
        alan.remove();
    }

    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', toastGoster);
    } else {
        toastGoster();
    }
})();
