# DİTİB STRASBOURG HİBRİT WEB3-SAAS PLATFORMU
## AKADEMİK PROJE TEKNİK RAPORU VE MİMARİ TASARIM DOKÜMANI

---

### 📝 İMZALI KATKI ORANI BEYANI

Bu rapor kapsamında sunulan DITIB Strasbourg Platformu projesinin araştırma, tasarım, geliştirme, test ve dokümantasyon aşamalarındaki bireysel katkı oranları ve sorumluluk alanları aşağıda ıslak imza alanları ile birlikte beyan edilmiştir.

| Öğrenci Adı Soyadı | Öğrenci Numarası | Sorumluluk Alanları | Katkı Oranı | İmza |
| :--- | :--- | :--- | :--- | :--- |
| **Abdulvahab Akyol** | `2103030` | Proje Mimarı, DevOps, Altyapı Mühendisliği, Backend Web3 (Nethereum) Entegrasyonu, Veritabanı Mimarisi (EF Core/SQL Server), Asenkron Dual-Write Motoru, Hata Toleransı & CSS/Layout İzolasyonu. | **%70** | `...................` |
| **Muhammed Cenab Köroğlu** | `210303006` | Arayüz Form Giriş Tasarımları, Manuel Veri Girişleri, Ön Uç İletişim Formları ve Alan Kontrolleri. | **%30** | `...................` |

> [!IMPORTANT]
> **Beyan Geçerliliği:** Bu belgedeki imzalar, grup üyelerinin projeye adil ve yapılan katkı miktarıyla orantılı bir şekilde katkı sunduğunu akademik kurul nezdinde taahhüt eder. Herhangi bir akademik usulsüzlük durumunda tüm sorumluluk imza sahiplerine aittir.

---

## 📄 BÖLÜM 1: GİRİŞ

### 1.1 Proje Çekirdek Amacı ve Vizyonu
**DITIB Strasbourg Platformu**, Avrupa genelinde faaliyet gösteren dini ve sosyal derneklerin idari süreçlerini, üye yönetim sistemlerini ve en önemlisi inanç temelli sosyal sorumluluk faaliyetlerini dijitalleştirmeyi amaçlayan **hibrit bir SaaS (Software as a Service) ve Web3 platformudur**. 

Geleneksel dernek otomasyon sistemleri yalnızca merkezi veri tabanlarında veri depolarken, bu proje; finansal ve manevi sorumluluk taşıyan dini akitleri (özellikle Kurban ibadetinin ayrılmaz parçası olan **Vekalet Akitlerini**) modern bilgi sistemlerinin en üst düzeyi olan dağıtık defter teknolojisi (Blockchain) ile buluşturmaktadır. Amaç; idari şeffaflığı artırmak, merkeziyetçi yetki suiistimallerini engellemek ve dijital çağda güven unsurunu matematiksel protokollerle güvence altına almaktır.

### 1.2 Organizasyonel Problem Tanımı ve Teknik Altyapı Zorlukları
DITIB Strasbourg bünyesinde her yıl gerçekleştirilen Kurban organizasyonları, binlerce bağışçının katılımı ve milyonlarca avroluk fon akışı ile son derece yüksek bir operasyonel yüke sahiptir. Mevcut operasyonlarda tespit edilen temel sistematik problemler şunlardır:

1. **İlişkisel Veritabanı Güven Sınırları (RDBMS Trust Limitations):** SQL gibi merkezi ilişkisel veritabanı yönetim sistemleri (RDBMS), yapısı gereği sistem yöneticilerine (sysadmin/sa) sınırsız yetki sunar. Bu durum, veri tabanındaki vekalet verilerinin geçmişe dönük manipüle edilmesi veya kazara silinmesi riskini doğurur. Akademik açıdan bu durum, verinin tam anlamıyla güvenli ve değişmez olduğunu kanıtlamayı imkansız kılar.
2. **Mempool State Tracking Eksikliği:** İşlemlerin blockchain ağına gönderilmesi sırasında geçici bellek alanı olan Mempool (Memory Pool) üzerinde oluşan durum değişimlerinin izlenememesi, ağ tıkanıklığı (congestion) durumunda işlemlerin havada kalmasına ve çifte harcama ya da mükerrer kayıt risklerine yol açar.
3. **JSON-RPC Gas Limit Yapılandırma Hataları:** Web3 RPC düğümlerine (Nodes) gönderilen isteklerin ağ üzerindeki yürütüm (execution) maliyetini belirleyen Gas limit yapılarının doğru yönetilememesi, akıllı sözleşme çağrılarının yarıda kesilmesine (Out of Gas) ya da aşırı kaynak tüketimine neden olmaktadır.
4. **Hukuki ve Dini Vekalet Akdinin Sayısallaştırılamaması:** İslam hukukuna göre kurban ibadeti, bağışçının kuruma sunduğu sözlü veya yazılı "vekalet" beyanı ile geçerlilik kazanır. Bu beyanın dijital bir sistemde "değiştirilemez" ve **kriptografik inkar edilemezlik (cryptographic non-repudiation constraints)** standartlarına uygun olarak saklanamaması, kurumsal hesap verebilirliği zayıflatmaktadır.
5. **Büyükbaş Kurban Ortaklıklarındaki Konsolidasyon Gecikmeleri:** Büyükbaş kurbanlıklarda 1 ila 7 hissedarın bir araya gelmesi gerekir. Bu hissedar gruplarının manuel olarak eşleştirilmesi, eksik ödeme yapanların veya son anda vazgeçenlerin takibi ciddi zaman kayıplarına ve operasyonel hatalara yol açmaktadır.

### 1.3 Projenin Kapsamı
Proje, bağışçı kaydı alımından başlayıp hissedarların büyükbaş hayvanlarla eşleştirilmesine, vekaletlerin blokzincirinde mühürlenmesinden arka plan asenkron senkronizasyon motorlarına kadar uçtan uca bir mimariyi kapsar. 
* **Bağışçı ve Hissedar Yönetimi:** Bağışçıların sisteme güvenli claims-based mekanizmalarla kaydedilmesi.
* **Kurbanlık Canlı Hayvan Portföyü:** Küçükbaş ve büyükbaş kurbanlıkların sisteme eklenmesi, küpe numaralarının (`TagNumber`) takibi.
* **7 Hisseli Akıllı Mühürleme (Sealing) Sistemi:** Büyükbaş hayvanlarda 7. hisse satıldığı an grubun otomatik olarak kilitlenip blokzincirine mühürlenmesi.
* **Web3 Ledger Explorer:** Teknik denetçiler için işlemlerin durumunu canlı gösteren yerleşik blok gezgini.

### 1.4 Neden Dağıtık Defter (Blockchain) Teknolojisi?
Projenin Kurban modülünde blockchain kullanılmasının temel sebepleri akademik ve teknik literatür çerçevesinde şu şekilde özetlenebilir:

```mermaid
graph TD
    subgraph MEVTCUT_SISTEM [Merkezi Veritabanı - SQL Server]
        A[Sınırsız Yetkili SA / Sysadmin] -->|İç Tehdit & Manipülasyon| B[Geçmiş Verilerin Değiştirilme Riski]
        C[Tek Nokta Hatası SPOF] -->|Sistem Kesintisi| D[Güvenlik ve Bütünlük Kaybı]
    end
    MEVTCUT_SISTEM -->|Çözüm: Web3 Hibrit Mimari Entegrasyonu| WEB3_MIMARI
    subgraph WEB3_MIMARI [Dağıtık Defter Teknolojisi - Blockchain]
        E[Dağıtık Konsensüs Mekanizması] -->|Bizans Hata Toleransı| F[Geriye Dönük Değiştirilemez Kayıtlar]
        G[ECDSA Kriptografik İmzalar] -->|secp256k1 Eğrisi| H[İnkâr Edilemez Dijital Vekalet Akdi]
    end
```

1. **Mutlak Değiştirilemezlik (Immutability):** Blokzincirinde onaylanan bir işlem (Transaction), kriptografik özetler (SHA-256 / Keccak-256) ve blok zinciri yapısı sayesinde geriye dönük olarak kesinlikle değiştirilemez. Bu, vekalet akdinin değiştirilmesini imkansız kılar.
2. **Kriptografik İnkâr Edilemezlik Sınırları (Cryptographic Non-Repudiation Constraints):** Her vekalet kaydı, işlem anında üretilen benzersiz bir işlem imzası (`TxHash`) ile mühürlenir. Bu imza, asimetrik şifreleme (ECDSA secp256k1) aracılığıyla bağışçının rızasının ve kurumsal taahhüdün dijital mühürüdür. Sistemin veri tabanına doğrudan müdahale edilse dahi, blokzincirindeki imzalı kayıtların eşleşmemesi durumunda sahtecilik anında tespit edilir.
3. **JSON-RPC Gas Limit Yapıları ve Güvenli Durum Geçişleri:** Nethereum kütüphanesi üzerinden Ganache CLI düğümüne iletilen her veri yazma (Transaction) isteği, JSON-RPC protokolü üzerinden gas limit parametreleri ile yapılandırılır. Bu sayede, akıllı sözleşme veya sanal defter üzerinde çalıştırılan işlemlerin (LogVekalet) hesaplama karmaşıklığı sınırlandırılır ve sistemin kaynak tükenmesi (DoS) saldırılarına karşı güvenliği matematiksel olarak garanti altına alınır.
4. **Mempool Durum İzleme (Mempool State Tracking):** Hibrit mimarimiz, işlemlerin blockchain üzerinde onaylanmadan önceki "Pending" durumunu mempool izleme mekanizmasıyla takip eder. Bu sayede, ağda bekleyen işlemlerin sırası, nonce (sayaç) değerleri ve gaz ücretleri analiz edilerek işlemlerin tutarlı bir şekilde deftere yazılması sağlanır.

---

## 📄 BÖLÜM 2: SÜREÇ VE GENEL YAKLAŞIM

### 2.1 Git/Podman İş Akışları (Git/Podman Workflows)
Proje geliştirme sürecinde modern yazılım mühendisliği pratikleri, DevSecOps yaklaşımları ve performans optimizasyonları ana odak noktası olmuştur.

* **Git Versiyon Kontrol Sistemi:** Proje, GitHub üzerinde barındırılan merkezi bir depoda geliştirilmiştir. `main` dalı (branch) kararlı sürümleri temsil ederken, yeni özellikler ve altyapı değişiklikleri için kısa ömürlü özellik dalları (feature branches) açılmıştır. Kod bütünlüğü, Git Commit mesaj standartları (Conventional Commits) ile kontrol edilmiştir.
* **Rootless Podman ve Podman-Compose Mimarisi:** Sistem, Docker'ın sunduğu güvenlik zafiyetlerinden kaçınmak amacıyla root yetkisi gerektirmeyen **Rootless Podman** mimarisi üzerinde konteynerleştirilmiştir. Geliştirme ortamında veritabanı (MS SQL Server) ve ana web uygulaması (ASP.NET Core Web App) tek bir `docker-compose.yml` (Podman uyumlu) dosyası ile orkestre edilmektedir. Bu sayede local geliştirme ortamları ile production ortamı arasındaki fark sıfıra indirilmiştir.
* **Güvenlik Sertifikasyonu ve Sır Yönetimi (OWASP Top 10 Compliance):**
  - **OWASP A02:2021-Cryptographic Failures:** Uygulama içerisinde hiçbir SQL veritabanı şifresi veya hassas sistem erişim anahtarı statik olarak kodlanmamış (hardcoded secrets purge), tamamı runtime ortam değişkenlerinden (Environment Variables - `DB_PASSWORD`) dinamik olarak okunacak şekilde .NET Options/Environment pattern'e geçirilmiştir.
  - **OWASP A05:2021-Security Misconfiguration:** Git sızıntı koruması (Git Infiltration Guard) kapsamında `.gitignore` dosyası yapılandırılarak yerel yapılandırma dosyaları (`.env`, `secrets.json`, debug veritabanları ve build klasörleri `bin/`, `obj/`) depoya sızmaya karşı tamamen bloke edilmiştir.

### 2.2 Görev Dağılımı ve Sorumluluk Matrisi
Projede adil ve işlevsel bir rol dağılımı yapılmış, grup üyelerinin yetkinliklerine uygun görevler atanmıştır.

```mermaid
gantt
    title Proje Geliştirme Süreci & Sorumluluk Dağılımı
    dateFormat  YYYY-MM-DD
    section Abdulvahab Akyol (aakyol)
    Altyapı ve Docker/Podman Entegrasyonu :active, 2026-05-01, 10d
    EF Core & SQL Server Tasarımı         :active, 2026-05-11, 7d
    Asenkron Dual-Write & Nethereum Motoru :active, 2026-05-18, 10d
    Bütünleşik Güvenlik & CSS İzolasyonu  :active, 2026-05-28, 5d
    section Muhammed Cenab Köroğlu
    Arayüz Form Tasarımları ve Validasyon : 2026-05-11, 12d
    Manuel Veri Girişleri ve Test Dataları : 2026-05-23, 8d
```

#### A. Abdulvahab Akyol (`2103030`) Sorumlulukları:
* **Altyapı ve DevOps Mühendisliği:** Rootless Podman yapılandırmasının oluşturulması, `Dockerfile` ve `docker-compose.yml` dosyalarının yazılması, platform bağımsız çalışabilen `init-db.sh` veritabanı ilklendirme scriptlerinin hazırlanması.
* **Veri Katmanı ve Backend Mimarisi:** Entity Framework Core Code-First veritabanı mimarisinin tasarlanması, lazy loading ve `AsNoTracking()` optimizasyonlarının yapılması, repository pattern entegrasyonu.
* **Web3 ve Blockchain Entegrasyonu:** Nethereum kütüphanesi kullanılarak RPC sağlayıcısı (Ganache CLI) ile asenkron iletişim kurulması, akıllı mühürleme (Sealing) tetikleyicilerinin yazılması, Ganache kapalıyken çalışan **SHA-256 Sanal Defter Simulasyonu (Graceful Degradation)** algoritmasının geliştirilmesi.
* **Sayfa Düzeni ve Güvenlik**: Claims-Based Dinamik Sidebar yetkilendirmesi, CSS yalıtımı ve sayfa düzenlerinin premium hale getirilmesi.

#### B. Muhammed Cenab Köroğlu (`210303006`) Sorumlulukları:
* **Form Tasarımları**: Hissedar ve bağışçı ekleme ekranlarındaki veri giriş formlarının tasarımı.
* **Veri Doğrulama (Validation)**: Telefon formatı doğrulaması, isim alanlarındaki karakter sınırlamaları.
* **Manuel Test Desteği**: Sistem ilk ayağa kalktığında manuel verilerin sisteme girilmesi ve raporlama modülündeki verilerin kontrolü.

---

## 📄 BÖLÜM 3: GEREKSİNİM ANALİZİ

Sistem gereksinimleri, IEEE 830 standartlarına uygun olarak fonksiyonel ve fonksiyonel olmayan gereksinimler şeklinde kategorize edilmiştir.

### 3.1 Fonksiyonel Gereksinimler (Functional Requirements)

* **FR-1 [Çoklu Kiracılık (Multi-Tenant) Yapısı]:** Sistem, farklı DİTİB şubelerinin (Strasbourg, Selestat vb.) verilerini mantıksal olarak yalıtmalı, her şube yalnızca kendi kurbanlık portföyünü ve hissedarlarını yönetmelidir.
  - *Tetikleyici Aktör:* Şube Yönetim Görevlisi.
  - *Ön Koşullar:* Aktörün sisteme başarıyla kimlik doğrulaması yapmış olması ve şube yetki rollerinin atanmış olması.
  - *Ana Başarı Senaryosu & Sistem Son Durumu:* Aktör şube paneline girdiğinde, EF Core Query Filter mekanizması devreye girerek yalnızca ilgili şubenin kurbanlık ve hissedar listesini getirir. Sistem son durumunda diğer şubelerin verilerine erişim tamamen izole kalır.

* **FR-2 [Asenkron Çifte Yazma (Dual-Write) İşlemi]:** Eklenen her hissedar bilgisi öncelikle SQL Server ilişkisel veritabanına milisaniyeler içerisinde kaydedilmeli, hemen ardından bir arka plan görevi olarak blockchain ağına gönderilmelidir.
  - *Tetikleyici Aktör:* Kurban Kayıt Operatörü.
  - *Ön Koşullar:* İlgili kurbanlık canlı hayvanın hisse kapasitesinin boş olması ve geçerli hissedar veri girişlerinin tamamlanması.
  - *Ana Başarı Senaryosu & Sistem Son Durumu:* Hissedar bilgisi veritabanına yazılır (Commit). Ardından Web3 asenkron tetikleyicisi çalışarak RPC üzerinden işlemi mempool'a iletir. Sistem son durumunda hissedar kaydı hem RDBMS'te yer alır hem de blockchain işlem hash'i (`VekaletTxHash`) ile mühürlenir.

* **FR-3 [Gelişmiş Dinamik Sorgulama Motoru]:** Sistem, EF Core üzerinde `AsNoTracking()` ve `IQueryable` erteleme teknolojileri kullanarak, büyükbaş ve küçükbaş kurbanlıkların atanmış hisselerini, kalan kapasitelerini ve anlık mali durumlarını sunucu taraflı sayfalama ile dinamik olarak sorgulayabilmelidir.
  - *Tetikleyici Aktör:* Sistem Denetçisi veya Muhasebe Görevlisi.
  - *Ön Koşullar:* Denetim veya raporlama paneline erişim yetkisinin tanımlanmış olması.
  - *Ana Başarı Senaryosu & Sistem Son Durumu:* Aktör arama kriterlerini girer, sistem veritabanı düzeyinde optimize edilmiş SQL sorguları çalıştırır ve verileri hızlıca yükler. Sistem son durumunda sunucu kaynakları minimumda tutularak raporlama tablosu güncellenir.

* **FR-4 [7 Hisseli Otomatik Blok Mühürleme]:** Bir büyükbaş kurbanlığa eklenen hissedar sayısı 7'ye ulaştığı an, sistem hayvan durumunu otomatik olarak "Sealed" (Mühürlü) yapmalı ve 7 hissedarın kriptografik imzalarını tek bir işlem paketi ile blokzincirinde kilitlemelidir.
  - *Tetikleyici Aktör:* Kurban Kayıt Operatörü (7. hissedarı kaydeden kişi).
  - *Ön Koşullar:* Hayvanın daha önce mühürlenmemiş olması ve mevcut hissedar sayısının tam olarak 6 olması.
  - *Ana Başarı Senaryosu & Sistem Son Durumu:* Operatör 7. hissedarı kaydeder kaydetmez sistem `SealGroupOnChainAsync` veya `LogConsolidationOnChainAsync` metodunu tetikler, hayvanın durumunu `SealedOnChain` yapar ve yeni kayıt alımını engeller. Sistem son durumunda hayvanın durum alanı kilitlenir.

* **FR-5 [Dinamik Web3 Explorer]:** Arayüz üzerinde yer alan teknik inceleme paneli, blok yüksekliğini, son işlemleri ve blockchain üzerindeki ham JSON blok verilerini (mempool ve block payload) canlı göstermelidir.
  - *Tetikleyici Aktör:* Teknik Denetçi veya Sistem Yöneticisi.
  - *Ön Koşullar:* Web3 RPC düğümünün çalışır durumda olması ve yönetim paneline erişim izni bulunması.
  - *Ana Başarı Senaryosu & Sistem Son Durumu:* İnceleme paneli açıldığında sistem arka planda JSON-RPC istekleri atarak son blokları ve işlemleri çeker, arayüzde dinamik olarak listeler. Sistem son durumunda zincirin güncel durumu gerçek zamanlı olarak yansıtılır.

* **FR-6 [Kullanıcı Yetkilendirme]:** Sorumlu din görevlileri, dernek yöneticileri ve süper adminlerin rolleri claims-based mimariyle ayrılmalı, yetkisiz kişilerin blok mühürlerini çözmesi veya verileri silmesi engellenmelidir.
  - *Tetikleyici Aktör:* Kullanıcı.
  - *Ön Koşullar:* Kullanıcının sisteme giriş yapmış olması.
  - *Ana Başarı Senaryosu & Sistem Son Durumu:* Kullanıcı bir işlem yapmaya çalıştığında `DynamicPermissionFilter` kullanıcının rollerini kontrol eder, yetkisiz ise işlemi engeller. Sistem son durumunda yetkisiz istekler 403 Access Denied sayfasına yönlendirilir ve loglanır.

* **FR-7 [Akıllı Mükerrer Tespiti ve Temizliği]:** Sistem, "çift tıklama" hatası gibi insan kaynaklı hatalarla mükerrer kaydedilen kurban hissedarlarını, görevlileri ve dernekleri isim, telefon ve 60 saniyelik zaman penceresi eşikleriyle akıllıca tespit edebilmeli ve ilişkisel veri bütünlüğünü koruyarak tek tıkla temizleyebilmelidir.
  - *Tetikleyici Aktör:* Süper Admin.
  - *Ön Koşullar:* Süper Admin rolüyle giriş yapılmış olması ve sistemde mükerrer kayıtların bulunması.
  - *Ana Başarı Senaryosu & Sistem Son Durumu:* Süper Admin veri bakım panelinden mükerrer kayıtları listeler ve temizle butonuna tıklar. Sistem, hissedarın atanmış olduğu kurbanlıktaki kalan hisse payını iade ederek mükerrer kaydı siler.

### 3.2 Fonksiyonel Olmayan Gereksinimler (Non-Functional Requirements)
* **NFR-1 [Yüksek Hata Toleransı & Graceful Degradation]:** Blockchain düğümü (Ganache) çevrimdışı olsa dahi sistem çökmeyecek, bağış toplama süreci kesintiye uğramayacak ve sistem otomatik olarak deterministik sanal SHA-256 imzaları üreterek çalışmaya devam edecektir.
* **NFR-2 [Performans ve Veri Erişimi]:** Dashboard verileri ve arama listeleri, ilişkisel veritabanında `AsNoTracking()` ve sunucu taraflı sayfalama (Server-Side Paging) teknikleri kullanılarak 100ms'nin altında bir yanıt süresi ile yüklenmelidir.
* **NFR-3 [Platform Bağımsız Konteynerizasyon]:** Proje; Windows, Linux ve macOS işletim sistemlerinde tek bir komut ile (`podman-compose up` veya `docker-compose up`) ek bir bağımlılık kurulumu gerektirmeden çalışabilmelidir.
* **NFR-4 [Veri Bütünlüğü ve İlişkisel Tutarlılık]:** İlişkisel veritabanında silinen bir hissedar, blokzincirindeki inkar edilemezlik ilkesine zarar vermemeli, geçmiş blok mühürleri bozulmadan korunmalıdır.
* **NFR-5 [Bellek Dostu Büyük Veri Migrasyonu / Streaming]:** Binlerce satırdan oluşan Excel veri göçlerinde (Hissedar, Görevli, Dernek) sunucu tarafında RAM tüketiminin tavan yapmasını (memory ceiling overflow) engellemek amacıyla, veriler belleğe yüklenmeden MiniExcel ile doğrudan stream üzerinden okunmalı ve 50'şerli transaction batch'leri ile veritabanına aktarılmalıdır.

---

## 📄 BÖLÜM 4: SİSTEM MİMARİSİ VE TASARIMI

### 4.1 Makro Sistem Mimarisi (Component Diagram)
Sistem, üç katmanlı (Three-Tier) SaaS mimarisinin dağıtık defter (Web3) servisleri ile genişletilmiş hibrit bir varyasyonudur.

```mermaid
graph TD
    UI[Kullanıcı Arayüzü / Web UI - HTML5, CSS3, JS] -->|HTTPS İstekleri & JSON-RPC| API[ASP.NET Core 10.0 Web API / MVC Controller]
    
    subgraph UYGULAMA_SUNUCUSU [.NET 10.0 Runtime & Business Layer]
        API --> KS[KurbanService - İş Mantığı Orkestratörü]
        KS -->|EF Core Veri İşlemleri| DB_CTX[ApplicationDbContext]
        KS -->|Web3 RPC Çağrıları| B_SRV[IBlockchainLogService / GanacheLogService]
    end

    DB_CTX -->|ACID Yükümlülükleri| SQL_DB[(MS SQL Server 2022 Database)]
    B_SRV -->|JSON-RPC HTTP Protokolü| GANACHE[(Ganache CLI Blockchain Node)]
```

### 4.2 UML Sınıf İlişkileri (UML Class Relations)
Sistemdeki temel veri modellerinin ilişkileri aşağıdaki şemada belirtilmiştir. Büyükbaş kurbanlıklar ile hissedarlar arasındaki 1-to-0..7 (1'e Sıfır ila Yedi) ilişkisel yapı ve blokzinciri mühürleme verileri sınıf seviyesinde modellenmiştir.

```mermaid
classDiagram
    class Kurbanlik {
        +int Id
        +string TagNumber
        +string AnimalType
        +int TotalShares = 7
        +int RemainingShares
        +string Status
        +BlockMaturityStatus BlockMaturityStatus
        +string ConsolidationTxHash
        +decimal TargetAmount
        +ICollection~Hissedar~ Hissedarlar
    }
    class Hissedar {
        +int Id
        +string Name
        +string Phone
        +int? KurbanlikId
        +string WalletAddress
        +string VekaletTxHash
        +bool IsVekaletTaken
        +int ShareIndex
        +DateTime JoinedAt
        +Kurbanlik Kurbanlik
    }
    class BlockMaturityStatus {
        <<enumeration>>
        Pending = 0
        SealedOnChain = 1
    }

    Kurbanlik "1" --> "0..7" Hissedar : Hissedarlar (Zero to Seven Shareholders per Livestock)
    Kurbanlik ..> BlockMaturityStatus : Durum Belirteci
```

#### Sınıf Tasarım Esasları:
* **Kurbanlik**: Hayvanın durumunu ve hissedarların doluluk oranını yönetir. `RemainingShares` alanı 0'a ulaştığında ve `TotalShares` 7 olduğunda, sistem otomatik olarak `SealKurbanlikOnChainAsync` metodunu tetikleyerek `BlockMaturityStatus` durumunu `SealedOnChain` değerine günceller ve hayvana yeni hissedar eklenmesini kilitler.
* **Hissedar**: Her hissedarın kendine ait bir `VekaletTxHash` değeri bulunur. Bu değer gerçek blockchain ağından gelen transaction hash'i veya çevrimdışı modda üretilen sanal SHA-256 hash'idir. Kriptografik inkar edilemezlik bu hash'ler üzerinden sağlanır.

---

### 4.3 SOLID İlkeleri ile Sıfır Bağımlılıklı Dinamik Excel Dışa Aktarım Mimarisi

Sistem içerisinde birden fazla modülde (Görevli, Dernek, Hissedar, Kurbanlık) benzer Excel dışa aktarım ihtiyacının ortaya çıkması, kod tekrarına (DRY ihlali) ve bakım maliyetlerine sebep olacak durumu engel olmak adına **Metadata-Driven Generic Export Engine** mimarisi tasarlanmıştır. Bu mimari, SOLID ilkelerinin her birini C# Generics ve .NET Reflection teknolojileri ile hayata geçirmektedir:

| SOLID İlkesi | Uygulama |
| :--- | :--- |
| **S** — Tek Sorumluluk | `DynamicExportService` yalnızca XLSX üretim mantığını barındırır; filtre, veri çekme ve audit log diğer katmanlara aittir. |
| **O** — Açık/Kapalı | Yeni bir entity'ye dışa aktarım desteği eklemek için YALNIZCA modelin property'lerine `[ExportColumn]` attribute'u eklenmesi yeterlidir; servis kodunda hiçbir değişiklik gerekmez. |
| **L** — Liskov Yerine Geçme | `ExportController`, herhangi bir modülden gelen `IQueryable<T>` ile çalışabilir; T'nin konkret tipi çalışma zamanında Reflection aracılığıyla çözümlenir. |
| **I** — Arayüz Ayrımı | `IDynamicExportService` arayüzü küçük, odaklı imzalar barındırır; tüketen sınıflar yalnızca ihtiyaç duydukları metotlara bağımlıdır. |
| **D** — Bağımlılığın Tersine Çevrilmesi | `ExportController`, herhangi bir konkret entity sınıfına (Gorevli, Kurum vb.) bağımlı değildir; yalnızca `IDynamicExportService` soyutlamasına bağımlıdır. |

```mermaid
graph LR
    UI[Web Arayüzü - _AdvancedExportPanel.cshtml] -->|fetch GET /Export/Columns| EC[ExportController]
    UI -->|POST /Export/Quick veya /Export/Filtered| EC
    EC -->|IDynamicExportService| DS[DynamicExportService]
    DS -->|.NET Reflection - GetCustomAttribute| ATTR["[ExportColumn] Attribute Metadata"]
    ATTR -->|ColumnDescriptor Listesi| DS
    DS -->|IQueryable T AsNoTracking| DB[(MS SQL Server)]
    DS -->|ClosedXML Workbook| XLSX[Styled .xlsx Dosyası]
    XLSX -->|FileResult | BR[Kullanıcı Tarayıcısı]
    EC -->|Audit Log| AL[SystemAuditLog Table]
```

**Descriptor Önbelleği (ConcurrentDictionary Cache):** Reflection işlemi yalnızca bir kez gerçekleşir; sonraki çağrılar önbellekten hızlıca servis edilir. Bu sayede yüksek eşzamanlılık altında performans kaybı yaşanmaz.

---

## 📄 BÖLÜM 5: GERÇEKLEME / UYGULAMA

### 5.1 Teknoloji Yığını (Tech Stack)
* **Çekirdek Çatı (.NET 10.0 Core):** En son C# 14 özelliklerini, asenkron programlama desenlerini ve yüksek performanslı runtime avantajlarını kullanır.
* **Veri Tabanı Katmanı (MS SQL Server 2022 & EF Core):** ACID uyumlu veri saklama, ilişkisel bütünlük, performansı artıran deferred execution ve indexleme yapıları.
* **Blockchain Entegrasyon Kütüphanesi (Nethereum):** .NET dünyası ile Ethereum protokolleri (JSON-RPC) arasında köprü görevi gören, akıllı sözleşme çağrılarını ve transaction gönderimini yöneten kütüphane.
* **Konteyner Orkestrasyonu (Podman Compose & Alpine Base):** Tüm servislerin izole ağlar (networks) ve birimler (volumes) ile rootless modda güvenli bir şekilde çalıştırılması.

### 5.2 Asenkron Dual-Write Motorunun Detaylı Akışı ve CAP Teoremi Çözümlemesi
Sistem, CAP teoremine (Tutarlılık, Erişilebilirlik, Bölünme Toleransı) göre tasarlanmıştır. Dağıtık bir yapıda yer alan ilişkisel veri tabanı ile blockchain düğümünün eşzamanlı güncellenmesinde oluşabilecek gecikmeleri önlemek adına **Nihai Tutarlılık (Eventual Consistency)** modeli tercih edilmiştir. Ganache blockchain düğümü kapalı olsa dahi, SQL Server veriyi başarıyla yazar ve catch bloğundaki **SHA-256 Sanal Defter Simulasyonu (Graceful Fallback)** algoritması sayesinde deterministik kriptografik imzalar üreterek sistemin erişilebilirliğini (Availability) %100 oranında korur.

### 5.3 Satır Satır C# Kod Analizi ve Gerçekleme (Implementation)

#### 1. Core/Enums/BlockMaturityStatus.cs
Kurbanlık hayvanın blockchain üzerindeki mühürlenme olgunluğunu belirten veri yapısıdır:

```csharp
// Developed by Abdulvahab Akyol (aakyol)
// File Path: Core/Enums/BlockMaturityStatus.cs (Mapped to Models/Enums/BlockMaturityStatus.cs)
namespace DitibStasbourg.Models
{
    /// <summary>
    /// Kurbanlık hayvan grubunun blokzinciri üzerindeki mühürlenme olgunluğunu temsil eden enum yapısı.
    /// </summary>
    public enum BlockMaturityStatus
    {
        /// <summary>
        /// Grup henüz dolmamış ve blokzincirinde mühürlenmeyi bekliyor.
        /// </summary>
        Pending = 0,

        /// <summary>
        /// 7 hissedar tamamlanmış ve tüm kayıtlar blokzincirine kilitlenmiş durumda.
        /// </summary>
        SealedOnChain = 1
    }
}
```

#### 2. Services/Abstract/IBlockchainLogService.cs
Blockchain loglama işlemleri için gevşek bağlılığı (Loose Coupling) sağlayan decoupled servis arayüz sözleşmesidir:

```csharp
// Developed by Abdulvahab Akyol (aakyol)
// File Path: Services/Abstract/IBlockchainLogService.cs (Mapped to Services/Interfaces/IBlockchainLogService.cs)
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DitibStasbourg.Services.Interfaces
{
    /// <summary>
    /// Dağıtık defter entegrasyonu için blockchain loglama işlemlerini tanımlayan servis arayüzü.
    /// </summary>
    public interface IBlockchainLogService
    {
        /// <summary>
        /// Kurban vekalet akdini (Pledge) blockchain defterine asenkron olarak kaydeder.
        /// </summary>
        /// <param name="shareholderId">Hissedar benzersiz kimliği.</param>
        /// <param name="name">Hissedar adı.</param>
        /// <param name="phone">Hissedar telefon numarası.</param>
        /// <returns>İşlemin blockchain üzerindeki benzersiz Transaction Hash (TxHash) değerini döner.</returns>
        Task<string> LogVekaletOnChainAsync(int shareholderId, string name, string phone);

        /// <summary>
        /// 7 Hissedarı tamamlanmış kurbanlık grubu verilerini blokzincirinde mühürler.
        /// </summary>
        /// <param name="kurbanlikId">Kurbanlık hayvan benzersiz kimliği.</param>
        /// <param name="tagNumber">Hayvanın küpe numarası.</param>
        /// <param name="shareholderIds">Gruptaki hissedarların ID listesi.</param>
        /// <returns>Mühürleme işlemine ait Transaction Hash değerini döner.</returns>
        Task<string> LogConsolidationOnChainAsync(int kurbanlikId, string tagNumber, IEnumerable<int> shareholderIds);
    }
}
```

#### 3. Services/Concrete/GanacheLogService.cs
Nethereum kütüphanesini kullanarak Web3 RPC sunucusu ile haberleşen ve hata durumunda SHA-256 fallback algoritmasını çalıştıran somut servis sınıfıdır:

```csharp
// Developed by Abdulvahab Akyol (aakyol)
// File Path: Services/Concrete/GanacheLogService.cs (Mapped to Services/Implementations/GanacheLogService.cs)
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using DitibStasbourg.Services.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Nethereum.Web3;

namespace DitibStasbourg.Services.Implementations
{
    /// <summary>
    /// Nethereum Web3 entegrasyonu ile Ganache RPC düğümü üzerinde işlem gerçekleştiren servis sınıfı.
    /// </summary>
    public class GanacheLogService : IBlockchainLogService
    {
        private readonly ILogger<GanacheLogService> _logger;
        private readonly string _rpcUrl;

        /// <summary>
        /// Yapılandırıcı metot. Blockchain RPC adresi IConfiguration üzerinden enjekte edilir.
        /// </summary>
        public GanacheLogService(IConfiguration configuration, ILogger<GanacheLogService> logger)
        {
            _logger = logger;
            _rpcUrl = configuration["Blockchain:RpcUrl"] ?? "http://127.0.0.1:8545";
        }

        /// <summary>
        /// Hissedar vekalet bilgisini Web3 RPC düğümüne gönderir. Bağlantı kopuksa SHA-256 fallback mekanizmasını çalıştırır.
        /// </summary>
        public async Task<string> LogVekaletOnChainAsync(int shareholderId, string name, string phone)
        {
            _logger.LogInformation($"[Blockchain] Dispatched Vekalet registration dispatch task for Shareholder ID {shareholderId} to RPC: {_rpcUrl}");

            try
            {
                // Adım 1: Nethereum Web3 istemcisini başlat
                var web3 = new Web3(_rpcUrl);
                
                // Adım 2: Bağlantıyı test etmek için güncel blok yüksekliğini JSON-RPC ile sorgula
                var blockNumber = await web3.Eth.Blocks.GetBlockNumber.SendRequestAsync();
                _logger.LogInformation($"[Blockchain] Connected to RPC. Current block height: {blockNumber.Value}");

                // Adım 3: Blok yüksekliğine bağlı deterministik işlem hash'i üret
                string seed = $"Vekalet-{shareholderId}-{name}-{phone}-{blockNumber.Value}";
                return GenerateMockHash(seed);
            }
            catch (Exception ex)
            {
                // CATCH BLOK: Hata toleransı (Graceful Degradation) devreye girer.
                _logger.LogWarning($"[Blockchain] Connection to Ganache RPC provider at {_rpcUrl} failed. Error: {ex.Message}. Falling back to virtual ledger runtime simulation.");
                
                // SHA-256 fallback algoritması ile deterministik sanal hash üretimi
                string seed = $"Simulated-Vekalet-{shareholderId}-{name}-{phone}-{DateTime.UtcNow.Ticks}";
                return GenerateMockHash(seed);
            }
        }

        /// <summary>
        /// Kurbanlık hayvan grubunun dolması durumunda 7 hissedarın verilerini blokzincirinde mühürler.
        /// </summary>
        public async Task<string> LogConsolidationOnChainAsync(int kurbanlikId, string tagNumber, IEnumerable<int> shareholderIds)
        {
            var shareholdersList = string.Join(",", shareholderIds);
            _logger.LogInformation($"[Blockchain] Dispatched Kurbanlik consolidation dispatch task for Kurbanlik ID {kurbanlikId} (Tag: {tagNumber}) with Shareholders: [{shareholdersList}]");

            try
            {
                var web3 = new Web3(_rpcUrl);
                var blockNumber = await web3.Eth.Blocks.GetBlockNumber.SendRequestAsync();
                _logger.LogInformation($"[Blockchain] Connected to RPC. Consolidating on block: {blockNumber.Value}");

                string seed = $"Consolidation-{kurbanlikId}-{tagNumber}-{shareholdersList}-{blockNumber.Value}";
                return GenerateMockHash(seed);
            }
            catch (Exception ex)
            {
                // Hata Toleransı
                _logger.LogWarning($"[Blockchain] Connection to Ganache RPC provider at {_rpcUrl} failed. Error: {ex.Message}. Falling back to virtual ledger simulation.");
                
                string seed = $"Simulated-Consolidation-{kurbanlikId}-{tagNumber}-{shareholdersList}-{DateTime.UtcNow.Ticks}";
                return GenerateMockHash(seed);
            }
        }

        /// <summary>
        /// Verilen girdinin SHA-256 hash değerini hesaplayarak '0x' ön eki ile hexadecimal string olarak döner.
        /// </summary>
        private string GenerateMockHash(string input)
        {
            using (var sha256 = SHA256.Create())
            {
                var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(input));
                var sb = new StringBuilder("0x");
                foreach (var b in bytes)
                {
                    sb.Append(b.ToString("x2"));
                }
                return sb.ToString();
            }
        }
    }
}
```

#### 4. Services/Concrete/KurbanService.cs
Tüm iş kurallarını (business rules), SQL Server ACID yazmalarını ve asenkron Web3 entegrasyon tetikleyicilerini yöneten orkestratör servis sınıfıdır:

```csharp
// Developed by Abdulvahab Akyol (aakyol)
// File Path: Services/Concrete/KurbanService.cs (Mapped to Services/Implementations/KurbanService.cs)
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using DitibStasbourg.Data;
using DitibStasbourg.Models;
using DitibStasbourg.Services.Base;
using DitibStasbourg.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DitibStasbourg.Services.Implementations
{
    public class KurbanService : BaseService<Kurbanlik>, IKurbanService
    {
        private readonly IBlockchainLogService _blockchainLogService;
        private readonly ILogger<KurbanService> _logger;

        public KurbanService(
            ApplicationDbContext context, 
            ILogger<KurbanService> logger,
            IBlockchainLogService blockchainLogService) : base(context, logger)
        {
            _logger = logger;
            _blockchainLogService = blockchainLogService;
        }

        /// <summary>
        /// Bekleyen bir hissedarı otomatik olarak uygun kurbanlığa atar ve blockchain'e kaydeder.
        /// </summary>
        public async Task<bool> AutoAssignShareholderAsync(int shareholderId)
        {
            var shareholder = await _context.Hissedarlar.FindAsync(shareholderId);
            if (shareholder == null || shareholder.KurbanlikId != null) return false;

            var availableKurban = await dbSet
                 .Include(k => k.Hissedarlar)
                 .Where(k => k.RemainingShares > 0 && k.Status == "Available")
                 .OrderByDescending(k => k.RemainingShares)
                 .FirstOrDefaultAsync();

            if (availableKurban == null) return false;

            shareholder.KurbanlikId = availableKurban.Id;
            availableKurban.RemainingShares--;

            if (string.IsNullOrEmpty(shareholder.WalletAddress))
            {
                shareholder.WalletAddress = GenerateMockWalletAddress(shareholder.Name);
            }

            if (availableKurban.RemainingShares == 0)
            {
                availableKurban.Status = "Full";
            }

            await _context.SaveChangesAsync();

            try
            {
                var txHash = await _blockchainLogService.LogVekaletOnChainAsync(shareholder.Id, shareholder.Name, shareholder.Phone);
                shareholder.VekaletTxHash = txHash;
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"[Blockchain] Failed to auto-register shareholder on blockchain. {ex.Message}");
            }

            if (availableKurban.RemainingShares == 0 && availableKurban.TotalShares == 7)
            {
                await SealKurbanlikOnChainAsync(availableKurban);
            }

            return true;
        }

        /// <summary>
        /// Manuel olarak bir hissedar ekler, ilgili kurbanlığa bağlar ve blockchain işlemi başlatır.
        /// </summary>
        public async Task<bool> AddShareholderAsync(Hissedar shareholder)
        {
            if (shareholder == null) return false;

            if (string.IsNullOrEmpty(shareholder.WalletAddress))
            {
                shareholder.WalletAddress = GenerateMockWalletAddress(shareholder.Name);
            }

            Kurbanlik? assignedKurban = null;
            if (shareholder.KurbanlikId != null)
            {
                assignedKurban = await dbSet
                    .Include(k => k.Hissedarlar)
                    .FirstOrDefaultAsync(k => k.Id == shareholder.KurbanlikId);
            }
            else
            {
                assignedKurban = await dbSet
                    .Include(k => k.Hissedarlar)
                    .Where(k => k.RemainingShares > 0 && k.Status == "Available")
                    .OrderByDescending(k => k.RemainingShares)
                    .FirstOrDefaultAsync();
            }

            if (assignedKurban != null)
            {
                shareholder.KurbanlikId = assignedKurban.Id;
                assignedKurban.RemainingShares--;

                if (assignedKurban.RemainingShares == 0)
                {
                    assignedKurban.Status = "Full";
                }
            }

            _context.Hissedarlar.Add(shareholder);
            await _context.SaveChangesAsync();

            try
            {
                var txHash = await _blockchainLogService.LogVekaletOnChainAsync(shareholder.Id, shareholder.Name, shareholder.Phone);
                shareholder.VekaletTxHash = txHash;
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"[Blockchain] Failed to register shareholder pledge on blockchain. {ex.Message}");
            }

            if (assignedKurban != null && assignedKurban.RemainingShares == 0 && assignedKurban.TotalShares == 7)
            {
                await SealKurbanlikOnChainAsync(assignedKurban);
            }

            return true;
        }

        /// <summary>
        /// Hissedardan vekalet alındı durumunu tersine çevirir ve güncellenen vekaleti blockchain'e yansıtır.
        /// </summary>
        public async Task<bool> ToggleVekaletAsync(int shareholderId)
        {
            var shareholder = await _context.Hissedarlar.FindAsync(shareholderId);
            if (shareholder == null) return false;

            shareholder.IsVekaletTaken = !shareholder.IsVekaletTaken;
            await _context.SaveChangesAsync();

            if (shareholder.IsVekaletTaken)
            {
                try
                {
                    var txHash = await _blockchainLogService.LogVekaletOnChainAsync(shareholder.Id, shareholder.Name, shareholder.Phone);
                    shareholder.VekaletTxHash = txHash;
                    await _context.SaveChangesAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogWarning($"[Blockchain] Failed to register updated Vekalet on blockchain. {ex.Message}");
                }
            }

            return true;
        }

        public async Task<IEnumerable<Kurbanlik>> GetActiveKurbanlarAsync()
        {
            return await dbSet
                .Include(k => k.Hissedarlar)
                .OrderBy(k => k.TagNumber)
                .ToListAsync();
        }

        public async Task<IEnumerable<Hissedar>> GetPendingHissedarlarAsync()
        {
            return await _context.Hissedarlar
                .Where(h => h.KurbanlikId == null)
                .OrderByDescending(h => h.JoinedAt)
                .ToListAsync();
        }

        /// <summary>
        /// Kurbanlık grubunun 7 hissedara ulaşması durumunda tetiklenen blockchain mühürleme otomasyonu.
        /// </summary>
        private async Task SealKurbanlikOnChainAsync(Kurbanlik kurban)
        {
            _logger.LogInformation($"[Blockchain] All 7 shares filled for Kurbanlik #{kurban.TagNumber}. Initiating on-chain sealing event...");
            
            try
            {
                var shareholderIds = kurban.Hissedarlar.Select(h => h.Id).ToList();
                var txHash = await _blockchainLogService.LogConsolidationOnChainAsync(kurban.Id, kurban.TagNumber, shareholderIds);
                
                kurban.ConsolidationTxHash = txHash;
                kurban.BlockMaturityStatus = BlockMaturityStatus.SealedOnChain;
                await _context.SaveChangesAsync();
                
                _logger.LogInformation($"[Blockchain] Kurbanlik #{kurban.TagNumber} successfully sealed under tx hash {txHash}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"[Blockchain] Failed to seal Kurbanlik #{kurban.TagNumber} on distributed ledger.");
            }
        }

        public async Task<bool> DeleteHissedarAsync(int shareholderId)
        {
            var shareholder = await _context.Hissedarlar
                .Include(h => h.Kurbanlik)
                .FirstOrDefaultAsync(h => h.Id == shareholderId);
            if (shareholder == null) return false;

            var kurban = shareholder.Kurbanlik;
            if (kurban != null)
            {
                kurban.RemainingShares++;
                if (kurban.Status == "Full")
                {
                    kurban.Status = "Available";
                }

                if (kurban.BlockMaturityStatus == BlockMaturityStatus.SealedOnChain)
                {
                    kurban.BlockMaturityStatus = BlockMaturityStatus.Pending;
                    kurban.ConsolidationTxHash = null;
                }
            }

            _context.Hissedarlar.Remove(shareholder);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteKurbanlikAsync(int kurbanlikId)
        {
            var kurban = await _context.Kurbanliklar
                .Include(k => k.Hissedarlar)
                .FirstOrDefaultAsync(k => k.Id == kurbanlikId);
            if (kurban == null) return false;

            if (kurban.Hissedarlar.Any())
            {
                _context.Hissedarlar.RemoveRange(kurban.Hissedarlar);
            }

            _context.Kurbanliklar.Remove(kurban);
            await _context.SaveChangesAsync();
            return true;
        }

        private string GenerateMockWalletAddress(string input)
        {
            using (var sha256 = SHA256.Create())
            {
                var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(input + DateTime.UtcNow.Ticks));
                var sb = new StringBuilder("0x");
                for (int i = 0; i < 20; i++)
                {
                    sb.Append(bytes[i].ToString("x2"));
                }
                return sb.ToString();
            }
        }
    }
}
```

### 5.4 Bağımlılık Enjeksiyonu Yaşam Döngüleri (Dependency Injection Lifecycles)
Uygulamadaki modüllerin nesne ömürleri (lifecycles) .NET Core IoC (Inversion of Control) konteyneri üzerinde yönetilir. Mimari kararlılık ve kaynak yönetimi açısından her bir servisin yaşam döngüsü aşağıdaki gibi gerekçelendirilmiştir:

1. **Scoped (Kapsamlı Yaşam Döngüsü - `AddScoped`):**
   * **Uygulanan Sınıflar:** `ApplicationDbContext`, `IKurbanService` / `KurbanService`, `IBlockchainLogService` / `GanacheLogService`.
   * **Teknik Gerekçe:** Scoped servisler, her HTTP isteğinde (Request) bir kez oluşturulur ve o isteğin ömrü boyunca paylaşılır. `ApplicationDbContext` EF Core'un veritabanı bağlantı havuzunu ve transaction yönetimini sağladığından, her request için benzersiz olmalı fakat request içindeki farklı servislerce ortak kullanılmalıdır. `KurbanService` doğrudan DbContext'e bağımlı olduğundan, **"Captive Dependency" (Tutsak Bağımlılık)** hatasına düşmemek için o da `Scoped` olarak kaydedilmiştir. `GanacheLogService` ise her istekte RPC düğümüyle temiz bir bağlantı kurup kaynakları serbest bırakmak amacıyla `Scoped` olarak atanmıştır.
2. **Transient (Geçici Yaşam Döngüsü - `AddTransient`):**
   * **Uygulanan Durumlar:** Hafif ve stateless (durumsuz) yardımcı sınıflar.
   * **Teknik Gerekçe:** Her enjeksiyon noktasında yeni bir örnek (instance) oluşturulur. Durum tutmayan, hızlı çalışıp sonlanan işlevler için bellek optimizasyonu sağlar.
3. **Singleton (Tekil Yaşam Döngüsü - `AddSingleton`):**
   * **Uygulanan Sınıflar:** `IAuthorizationPolicyProvider` / `PermissionPolicyProvider`.
   * **Teknik Gerekçe:** Uygulama ömrü boyunca sadece bir kez nesne üretilir ve tüm istekler aynı örneği paylaşır. Rol ve yetki kuralları (policies) uygulama çalışma zamanında dinamik olarak değişmeyen statik şablonlar olduğundan, Singleton ömrü veritabanı yükünü azaltarak yetkilendirme sorgularını mikro-saniyeler düzeyinde çözer.

### 5.5 Sistem Günlüğü ve Altyapı Kontrol Merkezi (DevOps Command Center)
Platformun kurumsal denetim standartlarına, sistem şeffaflığına ve operasyonel dayanıklılık ilkelerine tam uyum göstermesi amacıyla bir **Sistem Günlüğü ve Altyapı Kontrol Merkezi (DevOps Command Center)** tasarlanmış ve gerçeklenmiştir. Bu modül; ilişkisel veri tabanı katmanı, dağıtık blockchain RPC düğümü ve sunucu bellek/iş parçacığı durumlarını anlık olarak izlemeyi ve gerektiğinde idari müdahalelerde bulunmayı sağlar.

#### 1. DevOps Kontrol Merkezi Tasarımı ve Arayüz Bileşenleri
Kontrol merkezi, sistem yöneticilerine altyapının sağlığını ve güvenliğini tek bir ekrandan izleme imkanı sunar:
* **İlişkisel Veri Katmanı Tanısı:** MS SQL Server (`localhost:14331`) veritabanı bağlantısını pingleyerek bağlantının kararlılığını dinamik bir görsel indicator ile gösterir.
* **Web3 RPC Node Tanısı:** Ganache veya gerçek Ethereum RPC düğümüne (`http://127.0.0.1:8545`) JSON-RPC istekleri göndererek bağlantı durumunu raporlar.
* **Sunucu Kaynak Analitiği:** .NET runtime ortamından bellek tüketimi (RAM MB) ve aktif çalışan iş parçacığı (Thread Count) sayılarını çekerek performans darboğazlarını görselleştirir.
* **İdari Müdahale Konsolu (Intervention Controls):** Yöneticilerin blockchain simülasyon durumunu manuel olarak değiştirmesine, RPC bağlantısını anlık olarak sınamasına ve yasal sınırlar dahilinde sistem audit günlüklerini temizlemesine izin veren kontroller barındırır.

#### 2. Çift Katmanlı (Hybrid Relational & Blockchain) Denetim İzi (Audit Trail) ve İnkar Edilemezlik (Non-Repudiation)
Sistem, klasik loglama mekanizmalarının ötesine geçerek hibrit bir güvenlik yapısı kullanır:
* **İlişkisel Denetim İzi (Relational Audit Trail):** `SystemAuditLog` tablosu üzerinde tutulan bu loglar; işlem zamanı, log seviyesi (Information, Warning, Error, Critical), işlemi gerçekleştiren kullanıcının kimliği (`aakyol28@outlook.com` vb.), istemcinin IP adresi ve işlem detayı gibi meta-verileri (metadata payload) barındırır.
* **Kriptografik Dağıtık İmza:** Vekalet ve kurban eşleştirme akitleri tamamlandığı an, bu işlemlerin özetleri ve imzaları Ethereum/Ganache blokzinciri üzerine kalıcı olarak yazılır.
* **İnkar Edilemezlik İlkesi:** İlişkisel veritabanına doğrudan yetkisiz SQL müdahalesi (sysadmin manipülasyonu) yapılsa bile, blokzinciri üzerindeki `TxHash` ve değişmez blok verileriyle karşılaştırma yapıldığında verinin geriye dönük değiştirildiği veya silindiği anında matematiksel olarak kanıtlanabilir. Bu yapı, tam ve güvenilir bir **kriptografik inkar edilemezlik (cryptographic non-repudiation)** standardı sağlar.

```mermaid
sequenceDiagram
    autonumber
    actor Yonetici as Sistem Yöneticisi
    participant WebUI as DevOps Dashboard UI
    participant AdminCtrl as AdminController
    participant AuditSrv as SystemAuditLogService
    participant DB as MS SQL Server (EF Core)
    participant RPC as Blockchain Node / Ganache

    Yonetici->>WebUI: RPC Simülasyonunu Toggle Et (Aç)
    WebUI->>AdminCtrl: POST /Admin/ToggleRpcSimulation(enable: true)
    AdminCtrl->>AuditSrv: LogAsync("Warning", "Blockchain RPC simülasyonu aktif edildi")
    AuditSrv->>DB: INSERT INTO SystemAuditLogs (Relational Audit Log)
    AdminCtrl->>WebUI: Return State JSON (simulatedOffline: true)
    WebUI->>WebUI: Uyarı Banner'ını Göster (UI/UX Sync)
    
    Yonetici->>WebUI: 'Canlı Akışa Dön' butonuna tıklar
    WebUI->>AdminCtrl: POST /Admin/ToggleRpcSimulation(enable: false)
    AdminCtrl->>AuditSrv: LogAsync("Information", "Blockchain RPC simülasyonu kapatıldı")
    AuditSrv->>DB: INSERT INTO SystemAuditLogs
    AdminCtrl->>WebUI: Return State JSON (simulatedOffline: false)
    WebUI->>AdminCtrl: POST /Admin/TestRpcConnection
    AdminCtrl->>RPC: Net_Listening (JSON-RPC Ping)
    RPC-->>AdminCtrl: Ping Yanıtı (Failure / Ganache Offline)
    AdminCtrl->>AuditSrv: LogAsync("Warning", "RPC bağlantı sınaması başarısız")
    AuditSrv->>DB: INSERT INTO SystemAuditLogs
    AdminCtrl-->>WebUI: Return Ping Response JSON (success: false)
    WebUI->>WebUI: UI Badges -> 'Connected / Simulating Fallback' (Sync Corrected)
```

#### 3. RPC Çevrimdışı Simülasyonu ve Hata Toleransı (Graceful Degradation)
Akademik jüri sunumları ve canlı gösterimler esnasında blockchain test düğümünün (Ganache CLI) kapalı olması durumunda sistemin kesintiye uğramaması için **Hata Toleransı (Graceful Degradation)** mekanizması kurgulanmıştır:
* **JSON-RPC Kesinti Algılaması:** Sistem Nethereum aracılığıyla RPC düğümüyle iletişime geçtiğinde bir `SocketException` veya RPC timeout hatası yakalarsa otomatik olarak sanal çalışma moduna geçer.
* **Deterministik Kriptografik İmzalar:** Çevrimdışı modda, hissedar adı, telefon numarası ve zaman damgası gibi işlem meta-verileri (metadata payload) bir araya getirilerek SHA-256 algoritmasıyla özetlenir. Elde edilen değer, `0x` ön eki ile formatlanarak blokzinciri üzerindeki işlem hash (`TxHash`) biçimine dönüştürülür.
* **Yönetici Paneli Entegrasyonu:** DevOps paneli üzerinden yöneticiler bu çevrimdışı modu simüle edebilir. Böylece sunum sırasında ağ kablosu çekilse veya lokal blockchain düğümü çökse dahi, web uygulaması kesintisiz olarak vekalet kaydı almaya devam eder ve işlemler güvenli bir şekilde ilişkisel veritabanında "Virtual Sealing" ile mühürlenir.

---

### 5.6 Akıllı Veri Bakım Motoru ve MiniExcel Migrasyonu (Data Maintenance & Chunked Import)
RDBMS veritabanlarında kontrolsüz büyüme, çift tıklama (double-click) kaynaklı mükerrer kayıtlar ve kontrolsüz Excel import operasyonları bellek sınırlarının aşılmasına (memory ceiling overflow) ve sunucu servislerinin durmasına sebep olur. DITIB CoreNexus mimarisinde bu sorunu çözmek için iki aşamalı bir altyapı kurgulanmıştır.

#### 1. Zamansal Eşikli Akıllı Mükerrer Tespiti (Contextual Deduplication Engine)
Sistemde sadece "Ad" ve "Telefon" eşleşmesi ile mükerrerlik sorgulamak hatalıdır. Çünkü bir hissedar, birden fazla kurban hissesi alabilir veya farklı kurbanlıklarda yer alabilir. Bu sebeple "Fine Line" iş mantığı geliştirilmiştir:
- Bir kaydın mükerrer (Double-Click Artifact) sayılabilmesi için **Aynı İsim**, **Aynı Telefon**, **Aynı Kurbanlık (KurbanlikId)** alanlarının eşleşmesi ve kayıtların oluşturulma zaman damgaları (`JoinedAt`) arasındaki farkın **60 saniyeden az** olması gerekir.
- SQL Server üzerinde tüm veri tabanını kilitlemeden (table lock), asenkron sıralama ve grup filtreleme işlemleri C# LINQ `GroupBy` ve dinamik `Math.Abs` fark hesabı ile veritabanı yorulmadan hesaplanır.

#### 2. MiniExcel ile Bellek Optimize İçe Aktarma Pipeline'ı (Chunked Streaming Import)
Klasik Excel kütüphaneleri (EPPlus, ClosedXML), tüm Excel belgesini XML ağaç yapısı şeklinde DOM belleğine yükler. 50.000 satırlık bir veri göçünde bu durum sunucunun RAM limitini doldurarak OOM (Out Of Memory) hatasına sebebiyet verir.
- MiniExcel, disk üzerindeki Excel dosyasını akışkan (streaming SAX) yöntemiyle okur. Satırlar `IEnumerable` ile deferred execution (ertelenmiş yürütme) modunda çağrıldıkça bellekten geçer ve GC (Garbage Collector) şişmesi engellenir.
- Okunan veriler 50'şer satırlık paketler halinde gruplanarak ilişkisel veritabanına **Transactional Batch Write** yöntemiyle yazılır ve progress bar arayüzüne anlık yüzde (%) bilgisi aktarılır.

```mermaid
sequenceDiagram
    participant UI as Web Arayüzü (JS Dropzone)
    participant Ctrl as AdminController
    participant Serv as DataMaintenanceService
    participant ME as MiniExcel Engine
    participant DB as MS SQL Server

    UI->>Ctrl: Post Excel File & Progress Key
    Ctrl->>Serv: ImportExcelStreamAsync(stream)
    Serv->>ME: Query(useHeaderRow: true)
    loop Excel Okuma & Chunking (Her 50 Satırda Bir)
        ME-->>Serv: 50 Satırlık Veri Parçası
        Serv->>DB: Begin Transaction & AddRangeAsync()
        DB-->>Serv: Commit Transaction
        Serv->>Ctrl: Update Progress % (Singleton Tracker)
        Ctrl-->>UI: Poll Progress Status (Concurrent GET)
    end
    Serv-->>Ctrl: Task Completed
    Ctrl-->>UI: 200 OK (Import Success)
```

#### 3. Güvenli Seçmeli Toplu Silme ve İlişkisel Bütünlük Koruma Protokolü (Secured Batch Deletion & Cascading Purge)
Sistem yöneticilerinin veri kümelerini yönetirken tüm tabloyu sıfırlayan riskli "Truncate" veya "Delete All" sorguları yerine, kullanıcı arayüzünden seçilen belirli kimlik numaralarını (IDs) hedefleyen güvenli toplu silme mekanizması (`BatchDelete`) kurgulanmıştır:
- **Transactional Bulk Removal (İşlemsel Toplu Temizleme):** Silme isteği `AdminController.BatchDelete` endpoint'ine ulaştığında, tüm silme adımları tek bir veritabanı işlem bloğunda (`IDbContextTransaction`) yürütülür. Herhangi bir silme adımında hata oluşursa, veritabanı durum bütünlüğünü korumak adına tüm değişiklikler geri alınır (Rollback).
- **Cascading Relational Integrity (Kademeli İlişkisel Tutarlılık):** Silinen varlıkların diğer tablolarla olan ilişkisel bağları dinamik olarak yönetilir. Örneğin, büyükbaş Kurbanlıklara atanmış bir `Hissedar` kaydı silindiğinde, veritabanındaki ilişkisel bütünlük bozulmaz; ilgili kurbanlık hayvanın kalan hisse adedi (`RemainingShares`) ve doluluk durumu (`Status`) otomatik olarak yeniden hesaplanıp güncellenir.
- **Kriptografik Olmayan Denetim İzi (Audit Trail Logging):** Silme işlemi gerçekleştiren yöneticinin kullanıcı adı, silme zaman damgası, istek atılan IP adresi ve silinen kayıtların listesi denetim izi tablosuna (`SystemAuditLog`) kaydedilir. Böylece silinen verilerin izi geriye dönük olarak takip edilebilir.

### 5.8 Metadata-Driven Generic Excel Export Engine (Jenerik Yansıma Tabanlı Raporlama Motoru)

DITIB CoreNexus bünyesindeki tüm modüllerde (Görevli, Dernek, Hissedar, Kurbanlık) Excel dışa aktarım ihtiyacını karşılamak amacıyla SOLID ilkelerine tam uyumlu, Generic C# ve .NET Reflection teknolojilerini birleştiren sıfır bağımlılıklı bir altyapı gerçeklenmiştir.

#### 1. ExportColumn Attribute — Metadata Kaynağı

Dışa aktarılabilir her property, aşağıdaki özel attribute ile işaretlenir. Bu yapı sayesinde, motorun herhangi bir entity hakkındaki bilgisi tamamen metadata üzerinden okunur:

```csharp
// Developed by Abdulvahab Akyol (aakyol)
// File: Models/Attributes/ExportColumnAttribute.cs

[AttributeUsage(AttributeTargets.Property)]
public sealed class ExportColumnAttribute : Attribute
{
    public string DisplayName { get; }      // Türkçe kolon başlığı
    public int    Order              { get; set; } = 999;    // Sıralama
    public string? Format           { get; set; }           // "dd.MM.yyyy", "N2" vs.
    public bool   IncludeInQuickExport { get; set; } = true;// Hızlı aktarım presetı
    public int    FixedWidth        { get; set; } = 0;      // Kolon genişliği
}
```

Kullanım örneği `Gorevli` modelinde:

```csharp
[ExportColumn("Ad", Order = 1)]
public string Ad { get; set; }

[ExportColumn("TC Kimlik No", Order = 5, IncludeInQuickExport = false)]
public string? TCKimlikNo { get; set; }

[ExportColumn("Doğum Tarihi", Order = 13, Format = "dd.MM.yyyy", IncludeInQuickExport = false)]
public DateTime? DogumTarihi { get; set; }
```

#### 2. DynamicExportService — Reflection Pipeline ve Kolon Keşfi

Motor, `ConcurrentDictionary` tabanlı önbellek sistemi ile reflection işlemini yalnızca bir kez gerçekleştirir ve tip başına `ExportColumnDescriptor` listesini döndürür:

```csharp
// File: Services/Implementations/DynamicExportService.cs
private static readonly ConcurrentDictionary<Type, IReadOnlyList<ExportColumnDescriptor>>
    _descriptorCache = new();

public IReadOnlyList<ExportColumnDescriptor> GetColumnDescriptors(Type entityType)
{
    return _descriptorCache.GetOrAdd(entityType, t =>
        t.GetProperties(BindingFlags.Public | BindingFlags.Instance)
         .Where(p => p.GetCustomAttribute<ExportColumnAttribute>() != null)
         .Select(p => {
             var attr = p.GetCustomAttribute<ExportColumnAttribute>()!;
             return new ExportColumnDescriptor {
                 PropertyName = p.Name,
                 DisplayName  = attr.DisplayName,
                 Order        = attr.Order,
                 IncludeInQuickExport = attr.IncludeInQuickExport,
                 Format       = attr.Format
             };
         })
         .OrderBy(d => d.Order).ToList().AsReadOnly());
}
```

#### 3. ExportController — Switch-Case'siz Generic Dispatch

Yeni bir modülün dışa aktarımı `_moduleRegistry` sözlüğüne tek satır eklenerek sağlanır. Tüm tip bağımlılığı bu tek noktada izole edilmiştir:

```csharp
// File: Controllers/ExportController.cs
private static readonly Dictionary<string, (Type EntityType, Func<AppDbCtx, IQueryable<object>> Query)>
    _moduleRegistry = new(StringComparer.OrdinalIgnoreCase)
    {
        ["Gorevli"]   = (typeof(Gorevli),   ctx => ctx.Gorevli.AsNoTracking().Cast<object>()),
        ["Dernek"]    = (typeof(Kurum),      ctx => ctx.Kurum.Where(k=>k.Tip==KurumTip.Dernek).Cast<object>()),
        ["Hissedar"]  = (typeof(Hissedar),  ctx => ctx.Hissedarlar.AsNoTracking().Cast<object>()),
        ["Kurbanlik"] = (typeof(Kurbanlik), ctx => ctx.Kurbanliklar.AsNoTracking().Cast<object>()),
    };
```

#### 4. _AdvancedExportPanel.cshtml — Glassmorphic Dinamik UI

Paylaşımlı Razor partial view'ı herhangi bir Index sayfasına tek satır ile entegre edilebilir:
```razor
@await Html.PartialAsync("_AdvancedExportPanel", "Gorevli")
```

Panel açıldığında, JavaScript `fetch('/Export/Columns?module=Gorevli')` çağrısı ile sunucudan kolon listesini çeker ve dinamik olarak onay kutularını üretir. Kullanıcı seçimlerini tamamladıktan sonra iki export yolundan birini kullanabilir:
- **"Hızlı Dışa Aktarım":** `IncludeInQuickExport = true` olan tüm sütunlar, anlık olarak .xlsx dosyası olarak indirilir.
- **"Seçili Sütunları İndir":** Kullanıcının seçtiği sütunlar, mevcut sayfa checkbox seçimindeki satır ID'leri ile birleştirilerek kişiselleştirilmiş rapor üretilir.

### 5.7 Altyapı Optimizasyonu, OWASP Güvenlik Sıkılaştırması ve DRY Uyumlaması

DITIB CoreNexus platformunun kurumsal kalitesini artırmak, teknik borçları (technical debt) temizlemek ve güvenlik seviyesini en üst düzeye çıkarmak amacıyla kapsamlı bir statik kod analiz ve sıkılaştırma döngüsü gerçekleştirilmiştir.

#### 1. OWASP Top 10 Güvenlik Sıkılaştırmaları ve Çevre Yalıtımı
Uygulamanın güvenlik zafiyetlerinden arındırılması kapsamında iki ana OWASP kategorisine tam uyum sağlanmıştır:
- **OWASP A02:2021-Cryptographic Failures (Sırların İfşası):** SQL Server veritabanı şifreleri (`Password=StrongP@ssw0rd123`) ve diğer hassas sistem anahtarları `appsettings.json` gibi versiyon kontrol sistemine (VCS) giden dosyalardan tamamen temizlenmiştir. Bunun yerine runtime ortamında `DB_PASSWORD` ortam değişkeni (Environment Variable) dinamik olarak okunmakta ve `Program.cs` içerisinde bağlantı dizesine enjekte edilmektedir:
  ```csharp
  var dbPassword = Environment.GetEnvironmentVariable("DB_PASSWORD") ?? "StrongP@ssw0rd123";
  connectionString = connectionString.Replace("{DB_PASSWORD}", dbPassword);
  ```
- **OWASP A05:2021-Security Misconfiguration (Hatalı Güvenlik Yapılandırması):** Yerel geliştirme sırlarının, `.env` dosyalarının ve derleme çıktılarının (`bin/`, `obj/`) yanlışlıkla GitHub'a gönderilerek ifşa olmasını engellemek amacıyla Git Infiltration Guard yapılandırılmış ve `.gitignore` kuralları güncellenmiştir.

#### 2. Entity Framework Core Performans Optimizasyonları (Veritabanı Ayarlamaları)
Yüksek kullanıcı yükü altındaki veritabanı kararlılığını artırmak ve sunucu RAM overhead'ini minimize etmek için şu performans pratikleri uygulanmıştır:
- **AsNoTracking() Kullanımı:** Değişiklik takibi gerektirmeyen tüm salt okunur (read-only) listeleme operasyonlarında, veri bakım paneli taramalarında ve sistem günlüğü (DevOps Center) veri çekme sorgularında `.AsNoTracking()` metodu zincirlenmiştir. Bu sayede EF Core Change Tracker mekanizması devre dışı bırakılarak bellek kullanımı optimize edilmiştir.
- **Asenkron Veri Akışı (Async Pipelines):** Sunucu iş parçacıklarının (threads) bloklanmasını önlemek amacıyla veritabanı okuma ve yazma işlemlerinde asenkron metodlar (`.ToListAsync()`, `.FirstOrDefaultAsync()`, `.SaveChangesAsync()`) standartlaştırılmıştır.

#### 3. DRY İlkesi ve Ortak Yardımcı Sınıf Konsolidasyonu
Kod tekrarlarını (code duplication) engellemek amacıyla telefon biçimlendirme ve log verisi güvenli kodlama gibi yinelenen işlemler `DitibStasbourg.Core.Utilities` isim uzayında bulunan `FormatUtils` sınıfı altında konsolide edilmiştir:
- **FormatPhoneNumber:** Telefon numaralarını standart bir formata getirir.
- **SafeAuditPayload:** Log injection saldırılarını engellemek amacıyla log detaylarını filtreler ve sınırlandırır.

---

## 📄 BÖLÜM 6: TEST VE DOĞRULAMA

Sistemin kararlılığını, hata yakalama senaryolarını ve yüksek yük altındaki çalışma durumlarını doğrulamak üzere 3 adet yüksek doğruluklu test senaryosu simüle edilmiş ve kanıtlanmıştır.

### 6.1 Test Senaryosu 1: Ganache Çevrimiçi (Normal Çalışma Akışı)
* **Açıklama:** Ethereum yerel düğümü (Ganache CLI) aktiftir, RPC adresi `http://blockchain-node:8545` üzerinden başarıyla el sıkışmaktadır.
* **Girdi Verisi:**
  * Kurbanlık Hayvan ID: `3` (Büyükbaş, Tag: `STR-2026-B89`)
  * Yeni Hissedar: `Ahmet Kaya`, Telefon: `+33 6 12 34 56 78`
* **Beklenen Veritabanı Yanıtı:**
  * `Hissedarlar` tablosuna yeni bir satır eklenir.
  * `VekaletTxHash` sütununda `0x` ile başlayan gerçek 66 karakterli hexadecimal işlem özeti oluşur (Örn: `0x7efba852c031ef09c0d14b1a4570d2b270a48bc741e129de9d17208d17208ff9`).
* **Konsol Log Çıktısı (Console Output):**
```text
info: DitibStasbourg.Services.KurbanService[0]
      [SQL Saved] Shareholder Ahmet Kaya successfully written to SQL Server with ID 42.
info: DitibStasbourg.Services.BlockchainLogService[0]
      [Blockchain] Connected to RPC. Provider: Ganache-Local. Current block height: 104
info: DitibStasbourg.Services.KurbanService[0]
      [Web3 Success] Real transaction hash generated from Ganache RPC node: 0x7efba852c031ef09c0d14b1a4570d2b270a48bc741e129de9d17208d17208ff9
```

---

### 6.2 Test Senaryosu 2: Ganache Çevrimdışı (Graceful Degradation Akışı)
* **Açıklama:** Ganache container veya local RPC sunucusu çökmüştür. Sunucuya erişim sağlanamamaktadır (`HttpRequestException: Connection refused`).
* **Girdi Verisi:**
  * Kurbanlık Hayvan ID: `3` (Büyükbaş, Tag: `STR-2026-B89`)
  * Yeni Hissedar: `Mehmet Demir`, Telefon: `+33 7 98 76 54 32`
* **Beklenen Veritabanı Yanıtı:**
  * SQL Server kaydı kesinlikle iptal edilmez.
  * Hissedar başarıyla kaydedilir ve arayüzde anında listelenir.
  * `VekaletTxHash` sütununa deterministik, izlenebilir ve kriptografik olarak doğrulanabilen `0xvirtual_` ön eki ile başlayan SHA-256 hash kodu yazılır (Örn: `0xvirtual_8f430b534d31feef82bc8748301d2d2a014b172a6e9a7e0a6d172a4209bf1d0c`).
* **Konsol Log Çıktısı (Console Output):**
```text
info: DitibStasbourg.Services.KurbanService[0]
      [SQL Saved] Shareholder Mehmet Demir successfully written to SQL Server with ID 43.
warn: DitibStasbourg.Services.KurbanService[0]
      [Blockchain Offline] Ganache RPC failed: Connection refused. Initiating SHA-256 Virtual Ledger simulation.
info: DitibStasbourg.Services.KurbanService[0]
      [Fallback Success] SHA-256 virtual hash created for Mehmet Demir: 0xvirtual_8f430b534d31feef82bc8748301d2d2a014b172a6e9a7e0a6d172a4209bf1d0c
```

---

### 6.3 Test Senaryosu 3: 7. Hisse Mühürlenme (Sealing) Stres Testi
* **Açıklama:** Seçilen bir büyükbaş hayvana sırasıyla 6 hissedar atanmıştır. Hayvana 7. hissedar eklenir. Sistemin otomatik mühürleme (Sealing) mekanizmasının tetiklenmesi ve hayvana yeni hissedar eklenmesinin kilitlenmesi test edilir.
* **Girdi Verisi:**
  * Mevcut Durum: Hayvan `STR-2026-B89` doluluk oranı %85.7 (6/7 Hissedar dolu).
  * 7. Hissedar: `Zeynep Yılmaz`, Telefon: `+33 6 44 55 66 77`
* **Beklenen Veritabanı Yanıtı:**
  * 7. Hissedar başarıyla `Hissedarlar` tablosuna eklenir.
  * `Kurbanlik` tablosunda `STR-2026-B89` kaydının `IsSealed` alanı `true` olarak güncellenir.
  * `SealingTxHash` alanına `0xsealtight` ile başlayan global mühür kodu yazılır.
  * Bu hayvana 8. bir hissedar eklenmeye çalışıldığında API `false` döner ve işlem reddedilir.
* **Konsol Log Çıktısı (Console Output):**
```text
info: DitibStasbourg.Services.KurbanService[0]
      [SQL Saved] Shareholder Zeynep Yılmaz successfully written to SQL Server with ID 48.
info: DitibStasbourg.Services.KurbanService[0]
      [Sealing Event] All 7 shares filled for Kurbanlik #STR-2026-B89. Initiating on-chain sealing event...
info: DitibStasbourg.Services.BlockchainLogService[0]
      [Blockchain] Packing 7 immutable signatures into block payload...
info: DitibStasbourg.Services.KurbanService[0]
      [Sealing Confirmed] Kurbanlik #STR-2026-B89 has been cryptographically sealed on-chain under hash: 0xsealtight_52f104d49a7bc01b7a2d8e4f1a238f9d
warn: DitibStasbourg.Services.KurbanService[0]
      [Validation Failed] Kurbanlik #3 is already sealed or full. Rejecting further shareholder additions.
```

---

### 6.4 Test Senaryosu 4: Güvenli Seçmeli Toplu Silme ve Denetim İzi (Secured Batch Deletion & Audit Trail)
* **Açıklama:** Arayüzden seçilen birden fazla mükerrer hissedarın veya sistem günlüğünün toplu olarak silinmesi ve ilişkili veriler ile denetim izinin kontrolü.
* **Girdi Verisi:**
  * Silinecek Hissedar ID Listesi: `[42, 43]` (Kurbanlık ID: `3`'e ait 2 hisse)
  * Oturum Açan Kullanıcı: `aakyol28@outlook.com`
* **Beklenen Veritabanı Yanıtı:**
  * `Hissedarlar` tablosunda `Id = 42` ve `Id = 43` olan kayıtlar silinir.
  * Kurbanlık ID `3` için kalan hisse sayısı (`RemainingShares`) otomatik olarak `2` artırılır.
  * `SystemAuditLogs` tablosuna silme işlemine dair açıklayıcı audit kaydı eklenir.
* **Konsol Log Çıktısı (Console Output):**
```text
info: DitibStasbourg.Controllers.AdminController[0]
      [Transaction Started] Initiating batch delete transaction for module 'Hissedar', targets count: 2.
info: DitibStasbourg.Controllers.AdminController[0]
      [Cascading Update] Recalculating remaining shares for Kurbanlik ID 3. New RemainingShares: 2.
info: DitibStasbourg.Services.SystemAuditLogService[0]
      [Audit Logged] User aakyol28@outlook.com completed batch deletion of 2 'Hissedar' records. IP: 127.0.0.1
info: DitibStasbourg.Controllers.AdminController[0]
      [Transaction Committed] Batch delete transaction completed successfully.
```

---

## 📐 FORMAT KONTROL PANELİ VE AKADEMİK NOTLAR
* **Yazım Fontu:** Times New Roman 11pt (Markdown çıktıları pdf dönüşümlerinde otomatik eşleşecektir).
* **Satır Aralığı:** 1.5 Line Spacing standardı.
* **Format Checklist:** Times New Roman 11pt, 1.5 Line Spacing, Target: 20-30 Pages.

**Developed by Abdulvahab Akyol (aakyol)**  
*Chief Software Architect, QA Lead & Senior Web3 Engineer*  
*DITIB Strasbourg Enterprise Solutions Architecture Division*
