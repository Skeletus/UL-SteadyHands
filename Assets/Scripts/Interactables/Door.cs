using UnityEngine;

public class Door : MonoBehaviour
{
    [Header("Door Settings")]
    [SerializeField] private bool isOpen;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Collider2D doorCollider;

    [SerializeField] private Color closedColor = Color.red;
    [SerializeField] private Color openColor = Color.green;

    private void Awake()
    {
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        if (doorCollider == null)
        {
            doorCollider = GetComponent<Collider2D>();
        }

        UpdateVisual();
    }

    public void Open()
    {
        isOpen = true;
        UpdateVisual();
    }

    private void UpdateVisual()
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.color = isOpen ? openColor : closedColor;
        }

        if (doorCollider != null)
        {
            doorCollider.isTrigger = isOpen;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!isOpen) return;

        if (collision.CompareTag("Player"))
        {
            LevelManager.Instance.LoadNextLevel();
        }
    }
}