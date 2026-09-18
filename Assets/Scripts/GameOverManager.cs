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

        if (gameManager != null && gameManager.currency < gameManager.spawnCost)
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
        gameOverText.text = "YOU WIN!";
        gameOverText.gameObject.SetActive(true);
        if (retryButton != null) retryButton.SetActive(true);
        Time.timeScale = 0f;
    }

    void ShowLose()
    {
        gameOver = true;
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
