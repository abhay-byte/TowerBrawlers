using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Loads scenes by their build settings index.
/// </summary>
public class SceneLoader : MonoBehaviour
{
    public static SceneLoader Instance { get; private set; }

    public int TotalScenes => SceneManager.sceneCountInBuildSettings;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    /// <summary>
    /// Load a scene by its build order index (0 = first scene in Build Settings).
    /// </summary>
    public void LoadScene(int sceneIndex)
    {
        if (sceneIndex < 0 || sceneIndex >= SceneManager.sceneCountInBuildSettings)
        {
            Debug.LogError($"[SceneLoader] Invalid scene index {sceneIndex}. Valid range: 0-{SceneManager.sceneCountInBuildSettings - 1}");
            return;
        }

        SceneManager.LoadScene(sceneIndex);
    }

    /// <summary>
    /// Load a scene additively by build order index.
    /// </summary>
    public void LoadSceneAdditive(int sceneIndex)
    {
        if (sceneIndex < 0 || sceneIndex >= SceneManager.sceneCountInBuildSettings)
        {
            Debug.LogError($"[SceneLoader] Invalid scene index {sceneIndex}. Valid range: 0-{SceneManager.sceneCountInBuildSettings - 1}");
            return;
        }

        SceneManager.LoadScene(sceneIndex, LoadSceneMode.Additive);
    }

    /// <summary>
    /// Reload the current active scene.
    /// </summary>
    public void ReloadCurrentScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    /// <summary>
    /// Go to the next scene in build order.
    /// </summary>
    public void LoadNextScene()
    {
        int currentIndex = SceneManager.GetActiveScene().buildIndex;
        int nextIndex = currentIndex + 1;

        if (nextIndex >= SceneManager.sceneCountInBuildSettings)
        {
            Debug.LogWarning("[SceneLoader] No next scene — already at the end.");
            return;
        }

        LoadScene(nextIndex);
    }

    /// <summary>
    /// Go to the previous scene in build order.
    /// </summary>
    public void LoadPreviousScene()
    {
        int currentIndex = SceneManager.GetActiveScene().buildIndex;
        int previousIndex = currentIndex - 1;

        if (previousIndex < 0)
        {
            Debug.LogWarning("[SceneLoader] No previous scene — already at the start.");
            return;
        }

        LoadScene(previousIndex);
    }
}
