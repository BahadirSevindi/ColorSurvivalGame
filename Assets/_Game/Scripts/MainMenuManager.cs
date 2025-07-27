using UnityEngine;
using UnityEngine.SceneManagement; // Sahne yönetimi için gerekli

public class MainMenuManager : MonoBehaviour
{
    // Bu fonksiyonu Başlat butonunun On Click() olayına atayacağız
    public void StartGame()
    {
        // Build Settings'deki oyun sahnenizin adını veya index'ini kullanın
        // Eğer oyun sahnenizin adı "SampleScene" ise:
        SceneManager.LoadScene("SampleScene"); 
        // Veya Build Settings'deki index'i 1 ise:
        // SceneManager.LoadScene(1); 
    }
}