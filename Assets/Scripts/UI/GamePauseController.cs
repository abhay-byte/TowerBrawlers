using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class GamePauseController : MonoBehaviour
{
    [SerializeField] private Button pauseButton;
    [SerializeField] private Text pauseButtonText;
    [SerializeField] private string pauseLabel = "Pause";
    [SerializeField] private string resumeLabel = "Resume";

    private bool isPaused;

    private void Awake()
    {
        ResolveReferences();
        BindButton();
        RefreshLabel();
    }

    private void OnEnable()
    {
        ResolveReferences();
        BindButton();
        RefreshLabel();
    }

    private void OnDisable()
    {
        if (pauseButton != null)
            pauseButton.onClick.RemoveListener(TogglePause);
    }

    private void OnDestroy()
    {
        if (isPaused)
            Time.timeScale = 1f;
    }

    public void TogglePause()
    {
        isPaused = !isPaused;
        Time.timeScale = isPaused ? 0f : 1f;
        RefreshLabel();
    }

    private void BindButton()
    {
        if (pauseButton == null)
            return;

        pauseButton.onClick.RemoveListener(TogglePause);
        pauseButton.onClick.AddListener(TogglePause);
    }

    private void RefreshLabel()
    {
        if (pauseButtonText != null)
            pauseButtonText.text = isPaused ? resumeLabel : pauseLabel;
    }

    private void ResolveReferences()
    {
        if (pauseButton == null)
            pauseButton = transform.Find("Panel/PauseButton")?.GetComponent<Button>();

        if (pauseButtonText == null)
            pauseButtonText = transform.Find("Panel/PauseButton/Label")?.GetComponent<Text>();
    }
}
