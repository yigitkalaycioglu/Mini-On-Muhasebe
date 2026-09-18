// ============================================================
// Fatura kalem tablosu (Satış ve Alış faturası ortak)
//
// Kapsayıcı:  <div data-fatura-kalemleri
//                  data-koleksiyon="SatisFaturaSatirlari"  → model binding koleksiyon adı
//                  data-fiyat="satis" | "alis"             → stok seçilince hangi fiyat gelsin
//                  data-ondalik=",">                       → sunucu kültürünün ondalık ayırıcısı
//
// Model binding indeksleri 0'dan başlayıp kesintisiz olmalıdır
// (SatisFaturaSatirlari[0].StokId, [1].StokId ...). Satır silinince
// bütün satırlar yeniden numaralandırılır; aksi halde boşluktan sonraki
// satırlar sunucuya hiç ulaşmaz.
// ============================================================
(function () {
    const kok = document.querySelector('[data-fatura-kalemleri]');
    if (!kok) return;

    const koleksiyon = kok.dataset.koleksiyon;
    const fiyatTipi = kok.dataset.fiyat;
    const ondalik = kok.dataset.ondalik || ',';

    const govde = kok.querySelector('[data-kalem-govde]');
    const sablon = kok.querySelector('template[data-kalem-sablon]');
    const uyari = kok.querySelector('[data-kalem-uyari]');
    const form = kok.closest('form');
    const paraBicimi = new Intl.NumberFormat('tr-TR', { minimumFractionDigits: 2, maximumFractionDigits: 2 });

    // ----- Sayı yardımcıları -----
    // Hem "1.250,50" hem "1250.50" hem "12,5" yazımını kabul eder
    function sayi(deger) {
        let s = String(deger ?? '').replace(/\s/g, '');
        if (!s) return NaN;
        const sonVirgul = s.lastIndexOf(',');
        const sonNokta = s.lastIndexOf('.');
        if (sonVirgul > -1 && sonNokta > -1) {
            const ayirac = sonVirgul > sonNokta ? ',' : '.';
            const binlik = ayirac === ',' ? '.' : ',';
            s = s.split(binlik).join('').replace(ayirac, '.');
        } else {
            s = s.replace(',', '.');
        }
        return /^-?\d*\.?\d+$/.test(s) ? parseFloat(s) : NaN;
    }

    const yuvarla = (n) => Math.round((n + Number.EPSILON) * 100) / 100;

    // Sunucunun beklediği biçim: "12,50" (tr-TR)
    function sunucuBicimi(n, sabitBasamak) {
        const metin = sabitBasamak ? n.toFixed(2) : String(yuvarla(n));
        return metin.replace('.', ondalik);
    }

    // ----- Satır işlemleri -----
    const satirlar = () => Array.from(govde.querySelectorAll('tr'));
    const alan = (tr, ad) => tr.querySelector('[data-alan="' + ad + '"]');

    function yenidenNumarala() {
        satirlar().forEach(function (tr, i) {
            tr.querySelector('.satir-no').textContent = i + 1;
            tr.querySelectorAll('[data-alan]').forEach(function (el) {
                el.name = koleksiyon + '[' + i + '].' + el.dataset.alan;
            });
        });
    }

    function stokBilgisiniUygula(tr, fiyatlariDoldur) {
        const secenek = alan(tr, 'StokId').selectedOptions[0];
        const birimEl = tr.querySelector('.satir-birim');

        if (!secenek || !secenek.value) {
            birimEl.textContent = '';
            return;
        }

        birimEl.textContent = secenek.dataset.birim || '';

        if (fiyatlariDoldur) {
            const fiyat = parseFloat(fiyatTipi === 'alis' ? secenek.dataset.alis : secenek.dataset.satis) || 0;
            const kdv = parseFloat(secenek.dataset.kdv) || 0;
            alan(tr, 'BirimFiyat').value = sunucuBicimi(fiyat, true);
            alan(tr, 'KdvOrani').value = sunucuBicimi(kdv, false);
        }
    }

    function satirEkle(veri) {
        govde.appendChild(sablon.content.cloneNode(true));
        const tr = govde.lastElementChild;

        if (veri) {
            alan(tr, 'StokId').value = veri.stokId || '';
            if (veri.miktar != null) alan(tr, 'Miktar').value = sunucuBicimi(veri.miktar, false);
            if (veri.birimFiyat != null) alan(tr, 'BirimFiyat').value = sunucuBicimi(veri.birimFiyat, true);
            if (veri.kdvOrani != null) alan(tr, 'KdvOrani').value = sunucuBicimi(veri.kdvOrani, false);
            stokBilgisiniUygula(tr, false);
        }

        yenidenNumarala();
        hesapla();
        return tr;
    }

    function satirSil(tr) {
        tr.remove();
        if (satirlar().length === 0) satirEkle();
        yenidenNumarala();
        hesapla();
    }

    // ----- Hesaplama -----
    function hesapla() {
        let ara = 0;
        let kdvToplam = 0;

        satirlar().forEach(function (tr) {
            const miktar = sayi(alan(tr, 'Miktar').value) || 0;
            const fiyat = sayi(alan(tr, 'BirimFiyat').value) || 0;
            const oran = sayi(alan(tr, 'KdvOrani').value) || 0;

            const tutar = yuvarla(miktar * fiyat);
            const kdv = yuvarla(tutar * oran / 100);

            tr.querySelector('.satir-tutar').textContent = paraBicimi.format(tutar);
            ara += tutar;
            kdvToplam += kdv;
        });

        kok.querySelector('[data-toplam="ara"]').textContent = paraBicimi.format(ara) + ' TL';
        kok.querySelector('[data-toplam="kdv"]').textContent = paraBicimi.format(kdvToplam) + ' TL';
        kok.querySelector('[data-toplam="genel"]').textContent = paraBicimi.format(ara + kdvToplam) + ' TL';
    }

    // ----- Olaylar -----
    govde.addEventListener('change', function (e) {
        const tr = e.target.closest('tr');
        if (e.target.matches('[data-alan="StokId"]')) {
            stokBilgisiniUygula(tr, true);
            hesapla();
            alan(tr, 'Miktar').focus();
        }
    });

    govde.addEventListener('input', function (e) {
        if (e.target.matches('input[data-alan]')) {
            e.target.classList.remove('input-validation-error');
            hesapla();
        }
    });

    // Odaktan çıkınca sayıyı sunucu biçimine getir (ör. "2.5" → "2,50")
    govde.addEventListener('focusout', function (e) {
        if (!e.target.matches('input[data-alan]')) return;
        const n = sayi(e.target.value);
        if (!isNaN(n)) e.target.value = sunucuBicimi(n, e.target.dataset.alan === 'BirimFiyat');
    });

    govde.addEventListener('click', function (e) {
        const silButonu = e.target.closest('[data-kalem-sil]');
        if (silButonu) satirSil(silButonu.closest('tr'));
    });

    // Tablo içinde Enter formu göndermesin; son satırdaysa yeni satır açsın
    govde.addEventListener('keydown', function (e) {
        if (e.key !== 'Enter' || !e.target.matches('input')) return;
        e.preventDefault();
        const tr = e.target.closest('tr');
        if (tr === govde.lastElementChild) {
            satirEkle().querySelector('[data-alan="StokId"]').focus();
        }
    });

    kok.querySelector('[data-kalem-ekle]').addEventListener('click', function () {
        satirEkle().querySelector('[data-alan="StokId"]').focus();
    });

    // ----- Gönderim öncesi kontrol -----
    if (form) {
        form.addEventListener('submit', function (e) {
            // Tamamen boş satırları at
            satirlar().forEach(function (tr) {
                if (!alan(tr, 'StokId').value && !alan(tr, 'Miktar').value.trim()) tr.remove();
            });

            let hataVar = false;
            const dolu = satirlar();

            dolu.forEach(function (tr) {
                const stok = alan(tr, 'StokId');
                const miktar = alan(tr, 'Miktar');
                const fiyat = alan(tr, 'BirimFiyat');
                const kdv = alan(tr, 'KdvOrani');

                const kontroller = [
                    [stok, !!stok.value],
                    [miktar, sayi(miktar.value) > 0],
                    [fiyat, sayi(fiyat.value) >= 0],
                    [kdv, sayi(kdv.value) >= 0]
                ];

                kontroller.forEach(function ([el, gecerli]) {
                    el.classList.toggle('input-validation-error', !gecerli);
                    if (!gecerli) hataVar = true;
                });

                // Değerleri sunucu biçimine normalize et
                [miktar, fiyat, kdv].forEach(function (el) {
                    const n = sayi(el.value);
                    if (!isNaN(n)) el.value = sunucuBicimi(n, el === fiyat);
                });
            });

            if (dolu.length === 0) {
                satirEkle();
                hataVar = true;
                uyari.textContent = 'Faturaya en az bir kalem ekleyin.';
            } else if (hataVar) {
                uyari.textContent = 'İşaretli alanları kontrol edin: ürün seçilmeli, miktar sıfırdan büyük olmalı.';
            }

            yenidenNumarala();
            hesapla();

            uyari.hidden = !hataVar;
            if (hataVar) {
                e.preventDefault();
                e.stopImmediatePropagation();
            }
        });
    }

    // ----- Başlangıç -----
    // Doğrulama hatasıyla sayfa geri döndüğünde girilen kalemler korunur
    const ilkVeri = kok.querySelector('script[data-ilk-kalemler]');
    let ilkKalemler = [];
    try {
        ilkKalemler = JSON.parse(ilkVeri ? ilkVeri.textContent : '[]') || [];
    } catch (_) { /* geçersiz JSON: boş başla */ }

    if (ilkKalemler.length) {
        ilkKalemler.forEach(satirEkle);
    } else {
        satirEkle();
    }
})();
