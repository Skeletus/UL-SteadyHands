using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance { get; private set; }

    [Header("Level References")]
    [SerializeField] private Lever[] levers;
    [SerializeField] private Door exitDoor;

    [Header("Next Level")]
    [SerializeField] private string nextLevelName;

    private bool levelCompleted;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        if (levers == null || levers.Length == 0)
        {
            levers = FindObjectsByType<Lever>(FindObjectsSortMode.None);
        }

        CheckLevelCompletion();
    }

    public void CheckLevelCompletion()
    {
        if (levelCompleted) return;

        foreach (Lever lever in levers)
        {
            if (!lever.IsActivated)
            {
                return;
            }
        }

        CompleteLevel();
    }

    private void CompleteLevel()
    {
        levelCompleted = true;

        if (exitDoor != null)
        {
            exitDoor.Open();
        }
    }

    public void LoadNextLevel()
    {
        if (string.IsNullOrWhiteSpace(nextLevelName))
        {
            Debug.LogWarning("No next level assigned.");
            return;
        }

        SceneManager.LoadScene(nextLevelName);
    }
}