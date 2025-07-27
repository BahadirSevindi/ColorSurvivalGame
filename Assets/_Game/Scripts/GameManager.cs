// GameManager.cs
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("UI Elementleri")]
    public GameObject gameOverPanel;
    public TextMeshProUGUI scoreText;

    [Header("Kontrol Elementleri")]
    public GameObject touchPanelControl;

    [Header("Oyun Ayarları")]
    public float controlReverseInterval = 30f;

    private float gameTimer;
    private float nextReverseTime;
    private bool isGameOver;

    void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Debug.Log("GameManager: Singleton örneği oluşturuldu ve DontDestroyOnLoad.");
        }
        else
        {
            Debug.Log("GameManager: Zaten bir GameManager örneği var. Bu objeyi yok ediyorum.");
            Destroy(gameObject);
        }
        // FindUIRefs(); // Bu çağrı kaldırıldı, OnSceneLoaded'da yapılacak.
    }

    void OnEnable()
    {
        Debug.Log("GameManager: OnEnable çağrıldı.");
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        Debug.Log("GameManager: OnDisable çağrıldı.");
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log($"GameManager: OnSceneLoaded çağrıldı. Sahne: {scene.name}, Index: {scene.buildIndex}");

        if (scene.name == "SampleScene")
        {
            FindUIRefs(); // Sadece oyun sahnesi yüklendiğinde UI referanslarını bul
            ResetGameData();

            PlayerController player = FindAnyObjectByType<PlayerController>();
            if (player != null)
            {
                player.ResetPlayerState();
                Debug.Log("GameManager: PlayerController durumu yeniden başlatma sonrası sıfırlandı.");
            }
            else
            {
                Debug.LogWarning("GameManager: OnSceneLoaded anında PlayerController bulunamadı. PlayerState sıfırlanamadı. 'Player' tag'ine sahip bir objenin sahnede olduğundan emin olun.");
            }

            if (touchPanelControl != null)
            {
                touchPanelControl.SetActive(true);
                Debug.Log("GameManager: Dokunmatik kontrol paneli etkinleştirildi (OnSceneLoaded).");
            }
        }
        else if (scene.name == "StartScene")
        {
            FindUIRefs(); // Başlangıç sahnesi yüklendiğinde de UI referanslarını bul
            isGameOver = false;
            Time.timeScale = 1f;
            if (gameOverPanel != null) gameOverPanel.SetActive(false);
            Debug.Log("GameManager: Başlangıç ekranı yüklendi.");

            if (touchPanelControl != null)
            {
                touchPanelControl.SetActive(false);
                Debug.Log("GameManager: Dokunmatik kontrol paneli ana menüye dönerken devre dışı bırakıldı.");
            }
        }
    }

    void FindUIRefs()
    {
        Debug.Log("GameManager: FindUIRefs çağrıldı.");

        if (gameOverPanel == null || gameOverPanel.gameObject == null || ReferenceEquals(gameOverPanel, null))
        {
            gameOverPanel = null;
            Debug.Log("GameManager: GameOverPanel referansı boş veya geçersiz, sahneden aranıyor...");
            Canvas canvas = FindAnyObjectByType<Canvas>();
            if (canvas != null)
            {
                Transform panelTransform = canvas.transform.Find("GameOverPanel");
                if (panelTransform != null)
                {
                    gameOverPanel = panelTransform.gameObject;
                    Debug.Log("GameManager: GameOverPanel referansı Canvas.transform.Find ile bulundu!");
                }
                else
                {
                    Debug.LogWarning("GameManager: Canvas altında 'GameOverPanel' objesi bulunamadı (Canvas.transform.Find). Adını ve hiyerarşiyi kontrol edin.");
                }
            }
            else
            {
                Debug.LogWarning("GameManager: Sahnedeki Canvas objesi bulunamadı! UI elementleri bulunamadı.");
            }
        }
        else
        {
            Debug.Log("GameManager: GameOverPanel referansı Inspector'dan atanmış ve hala geçerli. Yeniden aramaya gerek yok.");
        }

        if (scoreText == null || scoreText.gameObject == null || ReferenceEquals(scoreText, null))
        {
            scoreText = null;
            Debug.Log("GameManager: ScoreText referansı boş veya geçersiz, sahneden aranıyor...");
            Canvas canvas = FindAnyObjectByType<Canvas>();
            if (canvas != null)
            {
                Transform textTransform = canvas.transform.Find("ScoreText");
                if (textTransform != null)
                {
                    scoreText = textTransform.gameObject.GetComponent<TextMeshProUGUI>();
                    Debug.Log("GameManager: ScoreText referansı Canvas.transform.Find ile bulundu!");
                }
                else
                {
                    Debug.LogWarning("GameManager: Canvas altında 'ScoreText' objesi bulunamadı (Canvas.transform.Find). Adını ve hiyerarşiyi kontrol edin.");
                }
            }
        }
        else
        {
            Debug.Log("GameManager: ScoreText referansı Inspector'dan atanmış ve hala geçerli. Yeniden aramaya gerek yok.");
        }

        if (touchPanelControl == null || touchPanelControl.gameObject == null || ReferenceEquals(touchPanelControl, null))
        {
            touchPanelControl = GameObject.FindWithTag("TouchControls");
            if (touchPanelControl != null)
            {
                Debug.Log("GameManager: Dokunmatik kontrol paneli 'TouchControls' tag'i ile bulundu!");
            }
            else
            {
                if (SceneManager.GetActiveScene().name == "SampleScene")
                {
                    Debug.LogWarning("GameManager: Dokunmatik kontrol paneli Inspector'dan atanmadı ve 'TouchControls' tag'ine sahip bir obje bulunamadı. Lütfen Inspector'dan atayın veya doğru tag kullandığınızdan emin olun.");
                }
            }
        }
        else
        {
            Debug.Log("GameManager: Dokunmatik kontrol paneli referansı Inspector'dan atanmış ve hala geçerli. Yeniden aramaya gerek yok.");
        }

        if (SceneManager.GetActiveScene().name == "SampleScene")
        {
            if (gameOverPanel != null) gameOverPanel.SetActive(false);
            if (scoreText != null) scoreText.text = "Süre: 0.00s";
        }
    }

    void ResetGameData()
    {
        gameTimer = 0f;
        nextReverseTime = controlReverseInterval;
        isGameOver = false;
        Time.timeScale = 1f;

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
            Debug.Log("GameManager: GameOverPanel gizlendi (ResetGameData).");
        }
        if (scoreText != null)
        {
            scoreText.text = "Süre: 0.00s";
            Debug.Log("GameManager: Skor texti sıfırlandı (ResetGameData).");
        }

        if (touchPanelControl != null)
        {
            touchPanelControl.SetActive(true);
            Debug.Log("GameManager: Dokunmatik kontrol paneli ResetGameData ile etkinleştirildi.");
        }

        Debug.Log("GameManager: Oyun verileri sıfırlandı.");
    }

    void Update()
    {
        if (isGameOver) return;

        gameTimer += Time.deltaTime;

        if (scoreText != null)
        {
            scoreText.text = $"Süre: {gameTimer:F2}s";
        }

        if (gameTimer >= nextReverseTime)
        {
            PlayerController player = FindAnyObjectByType<PlayerController>();

            if(player != null)
            {
                player.ToggleControls();
                Debug.Log($"Kontroller tersine çevrildi! Geçen Süre: {gameTimer:F2}s");
                nextReverseTime += controlReverseInterval;
            }
            else
            {
                Debug.LogWarning("GameManager: PlayerController bulunamadı! Kontroller tersine çevrilemedi. 'Player' tag'ine sahip bir objenin sahnede olduğundan emin olun.");
            }
        }
    }

    public bool IsGameOver()
    {
        return isGameOver;
    }

    public void AddScore(int points)
    {
        // Şu an için AddScore metodu skor artışı için kullanılmıyor.
    }

    // Oyun bittiğinde çağrılan metod
    public void GameOver()
    {
        if(isGameOver) return;

        isGameOver = true;
        Time.timeScale = 0f;

        if (touchPanelControl != null)
        {
            touchPanelControl.SetActive(false);
            Debug.Log("GameManager: Dokunmatik kontrol paneli devre dışı bırakıldı (GameOver).");
        }
        else
        {
            Debug.LogWarning("GameManager: Dokunmatik kontrol paneli referansı null! Devre dışı bırakılamadı.");
        }

        if(gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
            Debug.Log("GameManager: GameOverPanel aktif hale getirildi.");

            Button restartButtonComponent = gameOverPanel.GetComponentInChildren<Button>();
            if(restartButtonComponent != null)
            {
                restartButtonComponent.interactable = true; // Butonu etkileşime açık yap

                // YENİ EKLENDİ: Butonun OnClick olayına RestartGame metodunu programatik olarak bağla
                restartButtonComponent.onClick.RemoveAllListeners(); // Önceki dinleyicileri temizle
                restartButtonComponent.onClick.AddListener(RestartGame); // Metodu ekle
                Debug.Log("GameManager: RestartButton OnClick olayı programatik olarak atandı.");
            }
            else
            {
                Debug.LogWarning("GameManager: RestartButton, GameOverPanel altında bulunamadı veya Button bileşeni yok. interactable yapılamadı.");
            }
        }
        else
        {
            Debug.LogError("GameManager: GameOverPanel referansı hala null! Panelin Inspector'dan atandığından veya adının doğru olduğundan emin olun.");
        }
        Debug.Log("Oyun Bitti! Toplam Süre: " + gameTimer.ToString("F2") + "s");
    }

    public void RestartGame()
    {
        Debug.Log("GameManager: RestartGame çağrıldı!"); // Daha belirgin bir debug mesajı
        Time.timeScale = 1f;
        SceneManager.LoadScene("SampleScene");
        Debug.Log("GameManager: Ana oyun sahnesi yükleme komutu verildi (SampleScene).");
    }

    public void GoToMainMenu()
    {
        Debug.Log("GameManager: GoToMainMenu çağrıldı.");
        Time.timeScale = 1f;

        if (touchPanelControl != null)
        {
            touchPanelControl.SetActive(false);
            Debug.Log("GameManager: Dokunmatik kontrol paneli ana menüye dönerken devre dışı bırakıldı.");
        }

        SceneManager.LoadScene("StartScene");
        Debug.Log("GameManager: Başlangıç ekranı yükleme komutu verildi (StartScene).");
    }
}