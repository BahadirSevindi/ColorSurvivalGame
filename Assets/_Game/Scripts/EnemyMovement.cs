// EnemyMovement.cs
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))] // Rigidbody2D bileşenini zorunlu kılar
[RequireComponent(typeof(BoxCollider2D))] // BoxCollider2D bileşenini zorunlu kılar
[RequireComponent(typeof(SpriteRenderer))] // SpriteRenderer bileşenini zorunlu kılar
public class EnemyMovement : MonoBehaviour
{
    // Inspector'dan atanacak düşman hızı (EnemySpawner tarafından atanacak)
    // Bu değişkenin değeri, EnemySpawner tarafından dinamik olarak ayarlanacaktır.
    public float speed; 

    [Header("Color Settings")]
    public Color[] enemyColors = { Color.red, Color.green, Color.blue }; // Düşmanın alabileceği renk paleti
    
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private Vector2 moveDirection; // Düşmanın hareket edeceği yön
    private Camera mainCamera; // Kamera referansı

    void Awake()
    {
        // Gerekli bileşen referanslarını al.
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        mainCamera = Camera.main; // Ana kamerayı al

        // Rigidbody 2D ayarları (PlayerController ile tutarlı olmalı)
        rb.gravityScale = 0; // Yerçekiminden etkilenmesin
        rb.freezeRotation = true; // Dönmesini engelle
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous; // Sürekli çarpışma algılama

        // SpriteRenderer bulunamazsa hata logla
        if (spriteRenderer == null)
        {
            Debug.LogError("EnemyMovement: SpriteRenderer bileşeni düşmanda bulunamadı! Lütfen prefab'ı kontrol edin.");
        }

        // Eğer Inspector'dan renkler atanmamışsa, varsayılan renkleri kullan.
        if (enemyColors == null || enemyColors.Length == 0)
        {
            Debug.LogWarning("EnemyMovement: Düşman renkleri atanmamış veya boş. Varsayılan renkler (Kırmızı, Yeşil, Mavi) kullanılacak.");
            enemyColors = new Color[] { Color.red, Color.green, Color.blue };
        }

        // Düşmana rastgele bir renk ata
        AssignRandomColor();
        // Düşmanın oluşur oluşmaz Player'a doğru yönlenmesini sağla
        SetInitialDirectionToPlayer();
    }

    void FixedUpdate()
    {
        // Düşmanın hızını ayarla
        rb.linearVelocity = moveDirection * speed;

        // Ekran sınırları dışına çıkıp çıkmadığını kontrol et ve yok et
        CheckBoundaryAndDestroy();
    }

    // Düşmanın hareket yönünü ayarlayan public metot (EnemySpawner tarafından çağrılacak)
    public void SetMoveDirection(Vector2 direction)
    {
        moveDirection = direction.normalized; // Yönü normalize et
    }

    // Düşmanın ilk oluştuğunda Player'a doğru yönlenmesini sağlayan metot
    void SetInitialDirectionToPlayer()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if(player != null)
        {
            // Spawn anındaki player pozisyonuna göre yön belirle
            SetMoveDirection((player.transform.position - transform.position).normalized);
        }
        else
        {
            // Player yoksa rastgele yön (yedek olarak)
            Debug.LogWarning("EnemyMovement: 'Player' tag'ine sahip GameObject bulunamadı! Düşman rastgele yöne hareket edecek.");
            SetMoveDirection(new Vector2(Random.Range(-1f, 1f), Random.Range(-1f, 1f)).normalized);
        }
    }

    // Düşmana rastgele bir renk atayan metot
    void AssignRandomColor()
    {
        if (spriteRenderer != null && enemyColors.Length > 0)
        {
            int randomIndex = Random.Range(0, enemyColors.Length);
            Color selectedColor = enemyColors[randomIndex];
            // Renk ataması yaparken Alfa değerinin tam opak (255) olduğundan emin ol.
            selectedColor.a = 1f; // Alfa değerini 1 (tam opak) yap
            spriteRenderer.color = selectedColor;
            // Debug.Log("Düşmana renk atandı: " + selectedColor.ToString() + " @ Konum: " + transform.position);
        }
    }

    // Düşmanın ekran sınırları dışına çıktığında yok olmasını sağlayan metot
    void CheckBoundaryAndDestroy()
    {
        if (mainCamera == null)
        {
            Debug.LogError("EnemyMovement: Ana kamera referansı yok. Düşman yok ediliyor.");
            Destroy(gameObject);
            return;
        }

        Vector3 viewportPos = mainCamera.WorldToViewportPoint(transform.position);
        
        // Ekranın %20 dışına çıkınca yok et (tolerans ile)
        float destroyOffset = 0.2f; 
        if(viewportPos.x < -destroyOffset || viewportPos.x > 1f + destroyOffset || 
           viewportPos.y < -destroyOffset || viewportPos.y > 1f + destroyOffset)
        {
            Destroy(gameObject); // Düşmanı yok et
        }
    }

    // Editor'da debug amaçlı görsel çizimi
    void OnDrawGizmos()
    {
        // Hareket yönünü gösteren debug çizgisi
        if(Application.isPlaying && rb != null && rb.linearVelocity.magnitude > 0.1f) // Sadece oyun oynarken ve hareket ederken
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawLine(transform.position, transform.position + (Vector3)rb.linearVelocity.normalized * 3f);
        }
        else if (!Application.isPlaying && rb != null) // Editor modunda Rigidbody varsa
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawLine(transform.position, transform.position + (Vector3)moveDirection * 3f);
        }

        // Collider çizimini göster
        BoxCollider2D boxCollider = GetComponent<BoxCollider2D>();
        if(boxCollider != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(transform.position + (Vector3)boxCollider.offset, boxCollider.size);
        }
    }
}