using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class CastleGameOverController : MonoBehaviour
{
    [SerializeField] private CombatTarget playerCastle;
    [SerializeField] private CombatTarget enemyCastle;
    [SerializeField] private Text statusText;
    [SerializeField] private GameObject gameOverRoot;

    private bool hasEnded;

    private void Update()
    {
        if (hasEnded)
            return;

        if (playerCastle == null || !playerCastle.IsAlive)
        {
            EndMatch("Game Over", "Your castle was destroyed.");
            return;
        }

        if (enemyCastle == null || !enemyCastle.IsAlive)
            EndMatch("Victory", "Enemy castle destroyed.");
    }

    private void EndMatch(string title, string message)
    {
        hasEnded = true;

        if (gameOverRoot != null)
            gameOverRoot.SetActive(true);

        if (statusText != null)
            statusText.text = $"{title}\n{message}";

        Time.timeScale = 0f;
    }
}
