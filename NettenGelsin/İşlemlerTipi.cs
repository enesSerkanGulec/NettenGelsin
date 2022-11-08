using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NettenGelsin
{
    public enum İşlemler_Tipi
    {
        motoraşinVeriÇek,
        motoraşinDeğişim,
        motoraşinÇokluKayıtlarıSil,
        motoraşinOrtağaAktar,
        motoraşinResimleriİçeAktar,
        dinamikVerileriÇek,
        dinamikDeğişim,
        dinamikÇokluKayıtlarıSil,
        dinamikOrtağaAktar,
        dinamikResimleriİçeAktar,
        ortak_TablosunuOluştur,
        ortakTekrarlıKayıtlarınBilgileriniDinamiktekiGibiYap,
        ortakİçindeGeçenlerinFiyatlarınıArttır,
        ortakSilinecekleriSil,
        ortaktan_Stok_Fiyat_PaketMiktarı_SıfırOlanlarıSil,
        ortakTekrarlıKayıtlardanYüksekFiyatlılarıSil,
        ortakTablosunuBoyutununKüçült,
        ortakTablosunaEldeOlanlarıEkle,
        ortakDeğişim,
        ortakBinekDüzeltme,
        ortakDiscountSortOrderAyarlanıyor,
        ideaSoftTablosuOluştur,
        ortaktaOlmayanKayıtlarSitedenSiliniyor,
        sitedeOlmayanKayıtlarEkleniyor,
        güncellemeYapılıyor,
        yayındakiÜrünlerdenResmiOlmayanlarınResmiGönderiliyor
    }
}
