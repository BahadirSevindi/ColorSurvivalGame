// PlayerController.cs
using UnityEngine;
using UnityEngine.SceneManagement; // Eğer GameManager SceneManagement kullanıyorsa bu gereklidir.

[RequireComponent(typeof(Rigidbody2D))]    // Rigidbody2D bileşenini zorunlu kılar
[RequireComponent(typeof(SpriteRenderer))]   // SpriteRenderer bileşenini zorunlu kılar
[RequireComponent(typeof(CircleCollider2D))] // CircleCollider2D bileşenini zorunlu kılar

public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;             // Oyuncunun hareket hızı
    public float acceleration = 0.8f;        // Hızlanma yumuşaklığı (Lerp için)

    [Header("Color Settings")]
    public Color[] colors = { Color.red, Color.green, Color.blue }; // Oyuncunun alabileceği renkler
    public KeyCode[] colorKeys = { KeyCode.Keypad1, KeyCode.Keypad2, KeyCode.Keypad3 }; // Renk değiştirme tuşları (Numpad 1, 2, 3)

    [Header("Mobile Controls")]
    public VirtualJoystick virtualJoystick; // Joystick script'inize referans

    [Header("Movement Boundaries")] // YENİ: Oyuncunun hareket edebileceği sınırlar
    public float minX = -8.5f; // Inspector'dan ayarlanacak min X
    public float maxX = 8.5f;  // Inspector'dan ayarlanacak max X
    public float minY = -4.5f; // Inspector'dan ayarlanacak min Y
    public float maxY = 4.5f;  // Inspector'dan ayarlanacak max Y

    private Rigidbody2D rb;
    private SpriteRenderer sr;
    private Vector2 movementInput;
    private int currentColorIndex = 0;
    private bool controlsReversed = false;   // Kontrollerin tersine dönüp dönmediği durumu

    void Awake()
    {
        // Gerekli bileşen referanslarını al
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();

        // Rigidbody 2D ayarları
        rb.gravityScale = 0; // Yerçekiminden etkilenmesin
        rb.freezeRotation = true; // Kendi ekseni etrafında dönmesini engelle
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous; // Hızlı hareketlerde çarpışma kaçırmayı önler

        // Player'ın başlangıç durumunu burada sıfırla.
        ResetPlayerState();
    }

    void Update()
    {
        HandleMovementInput();       // Oyuncu hareket girişini işle
        HandleColorChangeInput();    // Klavye ile renk değiştirme girişini işle (mobil buton ayrıca çağıracak)
    }

    void FixedUpdate()
    {
        ApplyMovement();             // Fiziksel hareketleri uygula
        ClampPlayerPosition();       // YENİ EKLENEN: Oyuncunun pozisyonunu sınırlama
    }

    void HandleMovementInput()
    {
        Vector2 currentInput = Vector2.zero;

        // Mobil Joystick Kontrolleri (eğer atanmış ve aktifse öncelik ver)
        if (virtualJoystick != null && virtualJoystick.gameObject.activeInHierarchy)
        {
            currentInput = virtualJoystick.joystickVector;
        }
        else // Klavye inputları (eğer joystick kullanılmıyorsa veya atanmamışsa)
        {
            float moveX = Input.GetAxisRaw("Horizontal"); // A/D veya Sol/Sağ Ok
            float moveY = Input.GetAxisRaw("Vertical");    // W/S veya Yukarı/Aşağı Ok
            currentInput = new Vector2(moveX, moveY).normalized;
        }

        // Kontroller tersine dönmüşse, hareket girişlerini ters çevir.
        if (controlsReversed)
        {
            movementInput = currentInput * -1f; // X ve Y eksenlerini birden tersine çevir
        }
        else
        {
            movementInput = currentInput;
        }
    }

    void HandleColorChangeInput()
    {
        // Tanımlı tuşlara basıldığında renk değiştir (klavye ile)
        for (int i = 0; i < colorKeys.Length; i++)
        {
            if (Input.GetKeyDown(colorKeys[i]))
            {
                currentColorIndex = i;  // Yeni renk indeksini ata
                UpdatePlayerColor();    // Rengi güncelle
                Debug.Log("Player: Klavye ile renk değişti -> " + sr.color.ToString()); // Konsola bilgi yaz
                break;                  // Bir tuşa basıldıktan sonra döngüyü kır
            }
        }
    }

    // Mobil buton için çağrılacak public fonksiyon
    public void ChangePlayerColor()
    {
        // Renk indeksini bir sonraki renge çevir, sona gelince başa dön
        currentColorIndex = (currentColorIndex + 1) % colors.Length;
        UpdatePlayerColor(); // Rengi güncelle
        Debug.Log("Player: Buton ile renk değişti -> " + sr.color.ToString()); // Konsola bilgi yaz
    }

    void UpdatePlayerColor()
    {
        // SpriteRenderer'ın rengini seçilen renge ayarla
        sr.color = colors[currentColorIndex];
    }

    void ApplyMovement()
    {
        Vector2 targetVelocity = movementInput * moveSpeed;
        // Yumuşak hareket için Lerp kullan
        rb.linearVelocity = Vector2.Lerp(rb.linearVelocity, targetVelocity, acceleration * Time.fixedDeltaTime * 10f);
    }

    // YENİ EKLENEN METOT: Oyuncunun pozisyonunu belirli sınırlar içinde tutar
    void ClampPlayerPosition()
    {
        Vector3 currentPosition = transform.position;

        // X ve Y koordinatlarını tanımlanan sınırlar içinde tut
        currentPosition.x = Mathf.Clamp(currentPosition.x, minX, maxX);
        currentPosition.y = Mathf.Clamp(currentPosition.y, minY, maxY);

        transform.position = currentPosition; // Oyuncunun pozisyonunu güncellenmiş sınırlı pozisyona ayarla
    }

    // GameManager tarafından çağrılan kontrolleri tersine çevirme metodu
    public void ToggleControls()
    {
        controlsReversed = !controlsReversed; // controlsReversed değişkeninin değerini tersine çevir
        Debug.Log(controlsReversed ? "Kontroller ters çevrildi!" : "Kontroller normale döndü!"); // Konsola güncel kontrol durumunu yazdır
    }

    // Oyun yeniden başladığında Player'ın durumunu sıfırlayan metod
    public void ResetPlayerState()
    {
        controlsReversed = false; // Kontrolleri normale döndür
        currentColorIndex = 0;    // Başlangıç rengine dön (ilk renk)
        UpdatePlayerColor();      // Rengi güncelle
        rb.linearVelocity = Vector2.zero; // Hareketi durdur

        // Player'ın pozisyonunu ekranın ortasına sıfırla.
        // Bu değerleri sahnenizin ortasına göre ayarlayın.
        transform.position = Vector3.zero;

        // Joystick'i sıfırlamak için (oyun bitip tekrar başladığında kolu ortala)
        if (virtualJoystick != null && virtualJoystick.joystickHandle != null) // virtualJoystick null kontrolü eklendi
        {
            virtualJoystick.joystickHandle.anchoredPosition = Vector2.zero;
            virtualJoystick.joystickVector = Vector2.zero; // Vektörü sıfırla
        }

        Debug.Log("PlayerController: Player durumu sıfırlandı ve pozisyon resetlendi.");
    }

    // Fiziksel çarpışma algılandığında çağrılır
    void OnCollisionEnter2D(Collision2D collision)
    {
        // Çarpan objenin "Enemy" tag'ine sahip olup olmadığını kontrol et
        if (!collision.gameObject.CompareTag("Enemy"))
        {
            return; // Düşman değilse fonksiyondan çık
        }

        // Çarpan düşmanın SpriteRenderer bileşenini al
        SpriteRenderer enemySR = collision.gameObject.GetComponent<SpriteRenderer>();
        if (enemySR == null)
        {
            Debug.LogWarning("PlayerController: Çarpan düşmanda SpriteRenderer bulunamadı. Renk karşılaştırması yapılamıyor.");
            return; // SpriteRenderer yoksa işlemi sonlandır
        }

        // Debug mesajı ile çarpışma ve renk bilgilerini logla
        Debug.Log($"Çarpışma Algılandı! Player Rengi: {sr.color.ToString()}, Düşman Rengi: {enemySR.color.ToString()}");

        // Player'ın rengi ile düşmanın rengini karşılaştır (RGB değerleri üzerinden)
        // Mathf.Approximately, float değerlerindeki küçük sapmaları tolere eder.
        bool colorsMatch =
            Mathf.Approximately(sr.color.r, enemySR.color.r) &&
            Mathf.Approximately(sr.color.g, enemySR.color.g) &&
            Mathf.Approximately(sr.color.b, enemySR.color.b);

        if (!colorsMatch)
        {
            // Renkler uyuşmuyorsa Game Over
            Debug.Log("Player: " + sr.color.ToString() + " Düşman: " + enemySR.color.ToString() + " - Renkler Uyuşmuyor! Oyun Bitti!");
            if (GameManager.Instance != null)
            {
                GameManager.Instance.GameOver(); // GameManager'ın GameOver metodunu çağır
            }
            else
            {
                Debug.LogError("PlayerController: GameManager.Instance bulunamadı! GameOver çağrılamadı.");
            }
        }
        else
        {
            // Renkler uyuşuyorsa düşmanı yok et
            Debug.Log("Player: " + sr.color.ToString() + " Düşman: " + enemySR.color.ToString() + " - Renkler Eşleşti! Düşman Yok Ediliyor!");
            Destroy(collision.gameObject); // Aynı renkli düşmanı yok et
        }
    }

    // Editor'da debug amaçlı görsel çizimi
    void OnDrawGizmos()
    {
        // Oyuncunun çarpışma alanını temsil eden sarı tel küre çiz
        Gizmos.color = Color.yellow;
        // CircleCollider2D'nin gerçek yarıçapını kullanarak çizim yap
        CircleCollider2D circleCollider = GetComponent<CircleCollider2D>();
        if (circleCollider != null)
        {
            Gizmos.DrawWireSphere(transform.position + (Vector3)circleCollider.offset, circleCollider.radius);
        }
        else
        {
            // Collider yoksa varsayılan bir yarıçapla çiz
            Gizmos.DrawWireSphere(transform.position, 0.5f);
        }

        // YENİ EKLENEN: Hareket sınırlarını gösteren kırmızı bir dikdörtgen çiz
        Gizmos.color = Color.red;
        Vector3 topLeft = new Vector3(minX, maxY, 0);
        Vector3 topRight = new Vector3(maxX, maxY, 0);
        Vector3 bottomLeft = new Vector3(minX, minY, 0);
        Vector3 bottomRight = new Vector3(maxX, minY, 0);

        Gizmos.DrawLine(topLeft, topRight);
        Gizmos.DrawLine(topRight, bottomRight);
        Gizmos.DrawLine(bottomRight, bottomLeft);
        Gizmos.DrawLine(bottomLeft, topLeft);
    }
}