using UnityEngine;

public class Lever : MonoBehaviour
{
    [Header("Lever Settings")]
    [SerializeField] private bool isActivated;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Color inactiveColor = Color.yellow;
    [SerializeField] private Color activeColor = Color.green;

    public bool IsActivated => isActivated;

    private void Awake()
    {
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        UpdateVisual();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (isActivated) return;

        if (collision.gameObject.CompareTag("Ball"))
        {
            Activate();
        }
    }

    public void Activate()
    {
        isActivated = true;
        UpdateVisual();

        LevelManager.Instance.CheckLevelCompletion();
    }

    private void UpdateVisual()
    {
        if (spriteRenderer == null) return;

        spriteRenderer.color = isActivated ? activeColor : inactiveColor;
    }
}