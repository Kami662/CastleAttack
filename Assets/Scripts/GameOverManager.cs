using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using TMPro;

public class GameOverManager : MonoBehaviour
{
    public TextMeshProUGUI gameOverText;
    public GameObject retryButton;
    public GameManager gameManager;
    public Castle castle;

    private bool gameOver = false;

    void Update()
    {
        if (gameOver)
        {
            // Quick restart while the game-over screen is up
            if (Input.GetKeyDown(KeyCode.R))
            {
                RestartGame();
            }
            return;
        }

        if (castle != null && castle.isDestroyed)
        {
            ShowWin();
            return;
        }

        // Lose only once regeneration is over too — until the budget is spent,
        // waiting for currency is a valid move.
        if (gameManager != null && gameManager.BudgetSpent && !gameManager.CanPlayAnyCard)
        {
            // Ask the spawner's live registry rather than scanning the whole
            // scene. Consistent with Tower, and no FindObjectsByType per frame.
            if (MonsterSpawner.ActiveMonsters.Count == 0)
            {
                ShowLose();
            }
        }
    }

    void ShowWin()
    {
        gameOver = true;
        // Cheap now, useful once the run economy needs tuning data (GDD §7:
        // "one resource never binds" is the risk to watch for).
        int currencyLeft = gameManager != null ? gameManager.Currency : -1;
        Debug.Log($"[EncounterEnd] WIN — castle destroyed. Currency left: {currencyLeft}. {EconomySummary()} Time: {Time.timeSinceLevelLoad:F1}s.");
        gameOverText.text = "YOU WIN!";
        gameOverText.gameObject.SetActive(true);
        if (retryButton != null) retryButton.SetActive(true);
        Time.timeScale = 0f;
    }

    void ShowLose()
    {
        gameOver = true;
        int hpLeft = castle != null ? castle.currentHP : -1;
        int hpMax = castle != null ? castle.maxHP : -1;
        Debug.Log($"[EncounterEnd] LOSE — budget spent, no affordable cards, no monsters left. Castle HP left: {hpLeft}/{hpMax}. {EconomySummary()} Time: {Time.timeSinceLevelLoad:F1}s.");
        gameOverText.text = "YOU LOSE";
        gameOverText.gameObject.SetActive(true);
        if (retryButton != null) retryButton.SetActive(true);
        Time.timeScale = 0f;
    }

    string EconomySummary()
    {
        if (gameManager == null) return "";
        return $"Budget left: {gameManager.BudgetRemaining:F0}/{gameManager.encounterBudget}. " +
               $"Bounties earned: {gameManager.BountiesEarned}.";
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;

        // Clear the UI selection first. Without this, the Button's colour-tint
        // transition tries to touch its Image after the scene has been torn
        // down, which throws MissingReferenceException on reload.
        if (EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(null);
        }

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public bool IsGameOver()
    {
        return gameOver;
    }
}
