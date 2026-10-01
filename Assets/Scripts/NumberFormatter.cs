using UnityEngine;

// MonoBehaviour'dan MİRAS ALMIYORUZ ve class'ı "static" yapıyoruz.
public static class NumberFormatter
{
    // Idle oyunlarındaki klasik harf sıralaması
    // K: Kilo (Bin), M: Million, B: Billion, T: Trillion, Qa: Quadrillion, Qi: Quintillion...
    private static readonly string[] suffixes = { "", "K", "M", "B", "T", "Qa", "Qi", "Sx", "Sp", "Oc", "No", "Dc" };

    public static string Format(double number)
    {
        // Sayı 1000'den küçükse harf eklemeye gerek yok, direkt göster (Örn: 999)
        if (number < 1000d)
        {
            return number.ToString("F0"); 
        }

        int suffixIndex = 0;
        
        // Sayı 1000'den büyük olduğu sürece 1000'e böl ve bir sonraki harfe geç
        while (number >= 1000d && suffixIndex < suffixes.Length - 1)
        {
            number /= 1000d;
            suffixIndex++;
        }

        // F2: Virgülden sonra 2 basamak gösterir (Örn: 1.50M veya 2.34B)
        // Eğer 1.5M gibi tek basamak istersen "F1" yapabilirsin.
        return number.ToString("F2") + suffixes[suffixIndex];
    }
}