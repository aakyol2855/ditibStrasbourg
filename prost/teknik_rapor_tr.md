# CartoBureau DITIB - Kapsamlı Teknik Proje Raporu

Bu rapor, DITIB Doğu Fransa bölgesi derneklerinin yönetimi, harita üzerinde gösterimi ve yazdırılabilir özel afiş tasarımlarının oluşturulması amacıyla geliştirilen web tabanlı uygulamanın teknik altyapısını, mimarisini ve çalışma mantığını detaylandırmaktadır.

---

## 1. Mimari ve Teknoloji Yığını (Tech Stack)

Proje, istemci tarafında (Frontend) zengin bir kullanıcı deneyimi sunarken, sunucu tarafında (Backend) hafif ve taşınabilir bir Python sunucusu kullanacak şekilde tasarlanmıştır.

- **Frontend (Kullanıcı Arayüzü):** HTML5, CSS3 (Vanilla), JavaScript (ES6+).
- **Harita Motoru:** [Leaflet.js](https://leafletjs.com/) (Hafif ve açık kaynaklı interaktif harita kütüphanesi).
- **Harita Altlıkları (Tile Layers):** OpenStreetMap & CartoDB (Açık ve Koyu tema harita katmanları).
- **Backend (Sunucu):** Python 3 (`http.server` modülü kullanılarak özel olarak yazılmış REST API sunucusu).
- **Veri Depolama:** JSON tabanlı yerel dosya sistemi (`adresses.json` ve `config.json`).
- **Dış API'ler:**
  - **data.gouv.fr (API Adresse):** Adresleri GPS koordinatlarına çevirmek (Geocoding) için.
  - **OSRM (Open Source Routing Machine):** İki dernek arası mesafe ve rota hesaplaması için.

---

## 2. Dosya Yapısı ve Bileşenler

### 2.1. Frontend Bileşenleri

* **`index.html` (Ana Dashboard):**
  Son kullanıcıların dernekleri harita üzerinde gördüğü ana ekrandır. Yan menü (sidebar) üzerinden arama, departmana göre filtreleme ve interaktif rota hesaplama işlemlerini barındırır.
* **`admin.html` (Yönetim Paneli):**
  Yöneticilerin dernek ekleyip silebildiği ve afiş tasarımını yapılandırdığı sayfadır. İki ana sekmeden oluşur:
  1. *Dernek Yönetimi:* Yeni dernek ekleme, otomatik koordinat bulma ve veri tabanını güncelleme.
  2. *Afiş Konfigürasyonu:* A4 çıktısı alınacak harita üzerindeki noktaların sürükle-bırak (drag & drop) yöntemiyle kaydırılması, yazıların hizalanması (sağ, sol, üst, alt) ve renk/boyut ayarlarının yapılması.
* **`print.html` (A4 Afiş Önizleme):**
  Sadece yazdırma (Print) işlemine özel olarak tasarlanmış, kontrollerden (zoom, pan) arındırılmış temiz bir harita sayfasıdır. `admin.html` üzerinden yapılan değişiklikleri eşzamanlı (real-time) dinleyerek anında önizleme sunar.
* **`style.css` & `app.js`:**
  Karanlık/Aydınlık (Dark/Light) tema değişkenlerini (CSS variables) ve ana haritanın tüm JavaScript mantığını (OSRM API çağrıları, Leaflet marker yönetimi) barındırır.

### 2.2. Backend ve Veri Bileşenleri

* **`serveur.py` (Python REST API Sunucusu):**
  Standart bir statik dosya sunucusunun ötesine geçerek uygulamanın veri tabanı işlemlerini yönetir. POST ve GET isteklerini ayrıştırarak aşağıdaki API uç noktalarını (endpoints) sağlar:
  - `/api/add-address`, `/api/edit-address`, `/api/delete-address`: Dernek verilerini günceller ve eşzamanlı olarak hem `adresses.json` hem de `adresses.js` dosyalarına yazar.
  - `/api/config`, `/api/save-config`: Afiş üzerindeki özel değişiklikleri (sürüklenmiş noktalar, font renkleri) `config.json` dosyasına kaydeder.
  - `/api/upload-logo`, `/api/logo`: Base64 formatında özel logo yüklenmesini ve sunulmasını sağlar.
* **`adresses.json` / `adresses.js`:**
  Tüm derneklerin ID, İsim, Adres, Posta Kodu ve Enlem/Boylam (Lat/Lon) verilerini tutan ana veri kaynağıdır. (JS versiyonu CORS sorunlarını aşmak için statik olarak içeri aktarılır).
* **`config.json`:**
  Afiş tasarımı için yapılan manuel müdahalelerin (örn: bir noktanın orijinal adresinden 2 km sağa kaydırılması) saklandığı (Override) dosyadır.

---

## 3. Temel İşlevler ve Teknik Uygulama (Nasıl Çalışıyor?)

### 3.1. İnteraktif Harita ve Filtreleme (`app.js`)
Uygulama açıldığında Leaflet.js başlatılır ve `departements.js` içerisindeki GeoJSON verileri okunarak Doğu Fransa departmanlarının sınırları (poligonları) çizilir.
- **Filtreleme:** Kullanıcı bir arama yaptığında veya departman seçtiğinde, `state.addressMarkers` dizisindeki Leaflet marker'ları döngüye girer. Kriterlere uymayan marker'lar `map.removeLayer(marker)` ile haritadan kaldırılırken, uyanlar `marker.addTo(map)` ile geri eklenir.

### 3.2. Geocoding (Adresten Koordinat Bulma)
Yönetici `admin.html` üzerinden yeni bir dernek adresi girdiğinde "Vérifier par l'adresse" butonuna basar.
- **Teknik İşlem:** Tarayıcı, Fransa hükümetinin açık API'sine (`https://api-adresse.data.gouv.fr/search/?q=...`) istek atar. Dönen JSON verisinden enlem (lat), boylam (lon), departman kodu ve posta kodu otomatik olarak çekilip forma doldurulur. Marker anında haritada belirir.

### 3.3. Rota Hesaplama (OSRM Entegrasyonu)
Kullanıcı `index.html` üzerinde bir başlangıç ve bitiş noktası seçip hesapla dediğinde:
- **Teknik İşlem:** Başlangıç ve bitiş koordinatları alınarak `router.project-osrm.org/route/v1/driving/lon1,lat1;lon2,lat2` adresine HTTP GET isteği gönderilir.
- OSRM'den dönen rota geometrisi (GeoJSON formatında) alınır ve Leaflet üzerinde `L.geoJSON` kullanılarak parlayan turuncu kalın bir çizgi (Polyline) olarak haritaya çizilir. Mesafe ve süre bilgileri ekrana yansıtılır.

### 3.4. Afiş Tasarımı ve Real-Time (Eşzamanlı) Senkronizasyon
Bu proje modülünün en kritik teknik başarımlarından biri, yönetici sayfasındaki harita (Ajustement) ile çıktı sayfası (`print.html` iframe) arasındaki kusursuz veri akışıdır.
- **Sorun:** Leaflet haritalarında yazılar (tooltip/divIcon) bazen birbirine girebilir (overlapping).
- **Çözüm:** `admin.html` içerisinde her bir marker sürüklenebilir (`draggable: true`) yapılmıştır. Yönetici bir noktayı sürüklediğinde `drag` olayı tetiklenir.
- **postMessage API Kullanımı:** Sürüklenen noktanın yeni koordinatları, anında `iframe.contentWindow.postMessage()` kullanılarak `print.html` içine aktarılır.
- `print.html` içindeki olay dinleyicisi (`window.addEventListener('message')`) bu yeni koordinatları yakalar ve sayfayı yenilemeye gerek kalmadan (`dynamicLayerGroup.clearLayers()`) noktayı anında yeni yerine çizer.
- Bu sayede kullanıcı, afişi yazdırmadan önce son halini pürüzsüz bir şekilde canlı önizleme olarak görür.

### 3.5. CSS ile Ölçeklenebilirlik (Responsive ve Print)
- **Ekran Görünümü:** Uygulama `flexbox` ve `grid` yapıları kullanılarak her ekrana (mobil, tablet, masaüstü) uyarlanmıştır.
- **Yazdırma (Print):** `@media print` CSS direktifi kullanılarak, yazdır komutu verildiğinde yan menüler, butonlar ve arka planlar otomatik gizlenir (`display: none`). Sadece `210mm x 297mm` (A4 Boyutu) boyutlarındaki harita kapsayıcısı sayfaya tam oturtulur ve tarayıcı tarafından kayıpsız yazdırılması sağlanır.

---

## 4. Veri Akışı Örneği: "Yeni Bir Dernek Ekleme" Senaryosu

1. Yönetici `admin.html` formunu doldurur ve kaydet'e tıklar.
2. JavaScript, `fetch('/api/add-address')` komutuyla form verilerini (JSON formatında) `serveur.py`'ye gönderir.
3. `serveur.py` içindeki `do_POST` fonksiyonu isteği yakalar.
4. Sunucu `adresses.json` dosyasını okur, yeni veriyi listeye ekler ve dosyayı kaydeder.
5. Hemen ardından sunucu, `adresses.js` dosyasını (`const ADRESSES_DATA = [...]` formatında) baştan yazar. (Böylece tarayıcılar önbelleğe takılmadan doğrudan veriyi okuyabilir).
6. Sunucu HTTP 200 OK yanıtı döndürür, UI (Arayüz) tabloyu anında günceller ve Toast (bildirim) mesajı gösterilir.

---

## Sonuç
Bu uygulama; sunucu tarafında ağır veritabanları (MySQL, PostgreSQL) kullanmak yerine **statik dosya mimarisini ve açık API'leri** avantaja çevirerek oldukça hızlı, taşınabilir (sadece Python yüklü bir bilgisayarda bile saniyeler içinde çalıştırılabilir) ve modern bir "Single Page Application" (SPA) deneyimi sunmaktadır. Leaflet.js'in esnek yapısı sayesinde de harita üzerinde matbaa kalitesinde piksel-mükemmel (pixel-perfect) afiş düzenlemelerine olanak tanımaktadır.
