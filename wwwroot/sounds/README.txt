ORTAM SESLERİ — Pomodoro / Odaklanma Odası
===========================================

Ortam sesleri artık YÖNETİCİ PANELİNDEN yönetilir:
  SuperAdmin  ›  Ortam Sesleri  (/SuperAdmin/AmbientSounds)

Oradan ses ekleyince:
  - Dosya bu klasöre (wwwroot/sounds/) benzersiz bir adla kaydedilir.
  - Meta bilgisi (ad, ikon, dosya yolu) manifest.json içine yazılır.
  - Pomodoro sayfasındaki "Ortam Sesleri" mikserinde otomatik görünür.

Öneriler:
  - TELİFSİZ (CC0 / Public Domain) sesler kullanın:
    Pixabay (pixabay.com/sound-effects) veya Freesound (CC0 filtresi).
  - Döngü için başı/sonu birleşen ("seamless loop") dosyalar seçin.
  - Boyutu makul tutun (ör. 1-3 dk, ~128 kbps mp3). Üst sınır: 10 MB.

Not: manifest.json'ı elle düzenlemeyin; panel yönetir. Elle dosya
kopyalarsanız panelde görünmez (manifest'e eklenmediği için).
