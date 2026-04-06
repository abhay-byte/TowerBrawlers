using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class EnemyAttackCountdownUI : MonoBehaviour
{
    [SerializeField] private AITroopBuyer aiTroopBuyer;
    [SerializeField] private Text countdownText;
    [SerializeField] private string prefix = "Enemy attacks in ";

    private void Awake()
    {
        ResolveReferences();
        Refresh();
    }

    private void Update()
    {
        Refresh();
    }

    private void ResolveReferences()
    {
        if (aiTroopBuyer == null)
            aiTroopBuyer = FindAnyObjectByType<AITroopBuyer>();

        if (countdownText == null)
            countdownText = transform.Find("EnemyAttackCountdown")?.GetComponent<Text>();
    }

    private void Refresh()
    {
        if (countdownText == null)
            return;

        if (aiTroopBuyer == null)
        {
            countdownText.gameObject.SetActive(false);
            return;
        }

        float remaining = aiTroopBuyer.RemainingAttackDelaySeconds;
        bool show = remaining > 0f;
        if (countdownText.gameObject.activeSelf != show)
            countdownText.gameObject.SetActive(show);

        if (!show)
            return;

        int totalSeconds = Mathf.CeilToInt(remaining);
        int minutes = totalSeconds / 60;
        int seconds = totalSeconds % 60;
        countdownText.text = $"{prefix}{minutes:00}:{seconds:00}";
    }
}
