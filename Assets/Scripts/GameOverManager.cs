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

        // Lose = broke: no affordable card in hand, nothing alive that could
        // still earn plunder, and no town tribute still on its way (GDD §3
        // "Encounter economy").
        if (gameManager != null && !gameManager.CanPlayAnyCard && !gameManager.TributePending)
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
        Debug.Log($"[EncounterEnd] WIN — castle destroyed. Currency left: {currencyLeft}. {EconomySummary()} Time: {Time.timeSinceLevelLoad:F1}s. {WinBonuses()}");
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
        Debug.Log($"[EncounterEnd] LOSE — no affordable cards, no monsters left. Castle HP left: {hpLeft}/{hpMax}. {EconomySummary()} Time: {Time.timeSinceLevelLoad:F1}s.");
        gameOverText.text = "YOU LOSE";
        gameOverText.gameObject.SetActive(true);
        if (retryButton != null) retryButton.SetActive(true);
        Time.timeScale = 0f;
    }

    // Which reward-screen bonuses this win earns (GDD §3 "Alpha run rules").
    // Logged for now; the reward screen will read the same rules in Phase 3.
    // Each of all-towers / fast / blitz adds a card choice; all-towns adds a rare.
    string WinBonuses()
    {
        if (gameManager == null) return "";

        bool allTowers = gameManager.TotalTowers > 0 && gameManager.TowersRazed >= gameManager.TotalTowers;
        bool allTowns = gameManager.TotalTowns > 0 && gameManager.TownsRazed >= gameManager.TotalTowns;
        bool blitz = gameManager.TowersRazed == 0 && gameManager.TownsRazed == 0;
        bool fast = Time.timeSinceLevelLoad <= gameManager.fastWinSeconds;

        int choices = 3 + (allTowers ? 1 : 0) + (fast ? 1 : 0) + (blitz ? 1 : 0);
        var earned = new System.Collections.Generic.List<string>();
        if (allTowers) earned.Add("all towers");
        if (allTowns) earned.Add("all towns (rare)");
        if (fast) earned.Add($"fast win (<= {gameManager.fastWinSeconds:0}s)");
        if (blitz) earned.Add("blitz");

        return $"Bonuses: {(earned.Count > 0 ? string.Join(", ", earned) : "none")} -> {choices} card choices" +
               $" (towers razed {gameManager.TowersRazed}/{gameManager.TotalTowers}, towns {gameManager.TownsRazed}/{gameManager.TotalTowns}).";
    }

    string EconomySummary()
    {
        if (gameManager == null) return "";
        return $"Plunder earned: {gameManager.PlunderEarned} (lost at the cap: {gameManager.PlunderWasted}). " +
               $"Bounties: {gameManager.BountiesEarned}. Towns razed: {gameManager.TownsRazed} (tribute {gameManager.TributeEarned}). " +
               $"Cap: {gameManager.HoldingCap}. " +
               $"Plunder rate: x{gameManager.PlunderMultiplier:0.##}.";
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
