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

        if (gameManager != null && !gameManager.CanPlayAnyCard)
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
        int currencyLeft = gameManager != null ? gameManager.currency : -1;
        Debug.Log($"[EncounterEnd] WIN — castle destroyed. Currency left: {currencyLeft}. Time: {Time.timeSinceLevelLoad:F1}s.");
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
        Debug.Log($"[EncounterEnd] LOSE — no affordable cards, no monsters left. Castle HP left: {hpLeft}/{hpMax}. Time: {Time.timeSinceLevelLoad:F1}s.");
        gameOverText.text = "YOU LOSE";
        gameOverText.gameObject.SetActive(true);
        if (retryButton != null) retryButton.SetActive(true);
        Time.timeScale = 0f;
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
