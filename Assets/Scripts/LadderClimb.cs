using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Collider2D))]
public class LadderClimb : MonoBehaviour
{
    [SerializeField] private float lowerY;
    [SerializeField] private float upperY;
    [SerializeField] private GameObject promptUp;
    [SerializeField] private GameObject promptDown;

    private Transform player;

    private void Reset()
    {
        GetComponent<Collider2D>().isTrigger = true;
    }

    private void Update()
    {
        if (player == null)
        {
            return;
        }

        bool onUpperSide = Mathf.Abs(player.position.y - upperY) < Mathf.Abs(player.position.y - lowerY);
        if (promptUp != null)
        {
            promptUp.SetActive(!onUpperSide);
        }
        if (promptDown != null)
        {
            promptDown.SetActive(onUpperSide);
        }

        Keyboard keyboard = Keyboard.current;
        if (keyboard == null)
        {
            return;
        }

        if (!onUpperSide && (keyboard.upArrowKey.wasPressedThisFrame || keyboard.wKey.wasPressedThisFrame))
        {
            Climb(upperY);
        }
        else if (onUpperSide && (keyboard.downArrowKey.wasPressedThisFrame || keyboard.sKey.wasPressedThisFrame))
        {
            Climb(lowerY);
        }
    }

    private void Climb(float targetY)
    {
        Vector3 position = player.position;
        position.x = transform.position.x;
        position.y = targetY;
        player.position = position;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
        {
            return;
        }
        player = other.transform;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
        {
            return;
        }
        player = null;
        if (promptUp != null)
        {
            promptUp.SetActive(false);
        }
        if (promptDown != null)
        {
            promptDown.SetActive(false);
        }
    }
}
