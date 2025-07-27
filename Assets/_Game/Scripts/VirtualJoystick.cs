using UnityEngine;
using UnityEngine.EventSystems; // Dokunmatik olayları için gerekli

public class VirtualJoystick : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    [Header("Joystick Ayarları")]
    public RectTransform joystickBackground; // Joystick'in arka planı
    public RectTransform joystickHandle;     // Joystick'in hareket eden kolu
    public float joystickRange = 100f;       // Kolun hareket edebileceği maksimum yarıçap (piksel olarak)

    [HideInInspector] public Vector2 joystickVector; // Dışarıdan okunacak hareket vektörü (x ve y)

    private Vector2 joystickOriginalPos; // Joystick arka planının başlangıç pozisyonu
    private Vector2 handleOriginalPos;   // Joystick kolunun başlangıç pozisyonu

    void Start()
    {
        if (joystickBackground == null || joystickHandle == null)
        {
            Debug.LogError("VirtualJoystick: Joystick Background veya Joystick Handle referansları atanmamış!");
            enabled = false; // Script'i devre dışı bırak
            return;
        }

        joystickOriginalPos = joystickBackground.anchoredPosition; // Canvas'a göre pozisyon
        handleOriginalPos = joystickHandle.anchoredPosition;     // Arka plana göre pozisyon
    }

    // Joystick'e dokunulduğunda
    public void OnPointerDown(PointerEventData eventData)
    {
        OnDrag(eventData); // İlk dokunmada da sürükleme işlemini başlat
    }

    // Joystick üzerinde parmak sürüklenirken
    public void OnDrag(PointerEventData eventData)
    {
        Vector2 touchPos;
        // Dokunma pozisyonunu arka planın yerel koordinatlarına çevir
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(joystickBackground, eventData.position, eventData.pressEventCamera, out touchPos))
        {
            // Kolun hareket edeceği maksimum mesafeyi sınırlayın
            Vector2 moveDirection = touchPos;
            if (moveDirection.magnitude > joystickRange)
            {
                moveDirection = moveDirection.normalized * joystickRange;
            }

            // Kolu hareket ettir
            joystickHandle.anchoredPosition = moveDirection;

            // Joystick vektörünü hesapla (-1.0 ile 1.0 arasında)
            joystickVector = moveDirection / joystickRange;
        }
    }

    // Joystick'ten parmak çekildiğinde
    public void OnPointerUp(PointerEventData eventData)
    {
        joystickHandle.anchoredPosition = handleOriginalPos; // Kolu başlangıç pozisyonuna döndür
        joystickVector = Vector2.zero; // Hareket vektörünü sıfırla
    }
}