using UnityEngine;
using UnityEngine.InputSystem;

// 대화 없이 배경 이미지 하나만 보여주는 단순 장면 전환용 오버레이. ESC로 닫는다.
public class RoomImageOverlay : MonoBehaviour
{
    private void Update()
    {
        Keyboard keyboard = Keyboard.current;
        if (keyboard != null && keyboard.escapeKey.wasPressedThisFrame)
        {
            gameObject.SetActive(false);
        }
    }
}
