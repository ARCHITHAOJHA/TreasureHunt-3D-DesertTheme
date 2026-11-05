using UnityEngine;
using UnityEngine.SceneManagement;

namespace DesertTreasureHunt
{
    public enum GameState
    {
        Menu,
        Playing,
        Paused,
        GameOver
    }

    public enum DifficultyLevel
    {
        Easy,
        Medium,
        Hard
    }

    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [Header("Game State")]
        public GameState currentState = GameState.Playing;
        public DifficultyLevel difficulty = DifficultyLevel.Medium;

        [Header("Run Progression")]
        public float currentSpeed = 12f;
        public float baseSpeed = 12f;
        public float maxSpeed = 24f;
        public float speedAccelerationRate = 0.15f; // Speed increase per second
        public float distanceRun = 0f;

        [Header("Stats & Score")]
        public int score = 0;
        public int coinsCollected = 0;
        public int gemsCollected = 0;
        public int keysCollected = 0;
        public int scoreMultiplier = 1;
        public int highScore = 0;

        [Header("Power-Up Status")]
        public bool isMagnetActive = false;
        public float magnetDuration = 10f;
        private float magnetTimer = 0f;

        public bool isShieldActive = false;

        public bool isMultiplierActive = false;
        public float multiplierDuration = 10f;
        private float multiplierTimer = 0f;

        public bool isSprintActive = false;
        public float sprintDuration = 5f;
        private float sprintTimer = 0f;

        // Key Finder — highlights keys on screen
        public bool isKeyFinderActive = false;
        public float keyFinderDuration = 12f;
        private float keyFinderTimer = 0f;

        // Treasure Bonus — stacks bonus score for game-over screen
        public int treasureBonusScore = 0;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            LoadSettings();
        }

        private void Start()
        {
            ApplyDifficulty(difficulty);
            highScore = PlayerPrefs.GetInt("DesertHunt_HighScore", 0);
            Time.timeScale = 1f;
        }

        private void Update()
        {
            if (currentState != GameState.Playing) return;

            // Distance & Speed
            float deltaDist = currentSpeed * Time.deltaTime;
            distanceRun += deltaDist;

            // Gradually accelerate
            if (currentSpeed < maxSpeed && !isSprintActive)
            {
                currentSpeed += speedAccelerationRate * Time.deltaTime;
            }

            // Score based on distance & multiplier
            score += Mathf.RoundToInt(deltaDist * 2f * scoreMultiplier);

            // Handle Powerup Timers
            UpdatePowerUps();
        }

        public void ApplyDifficulty(DifficultyLevel level)
        {
            difficulty = level;
            switch (level)
            {
                case DifficultyLevel.Easy:
                    baseSpeed = 9f;
                    maxSpeed = 18f;
                    speedAccelerationRate = 0.08f;
                    break;
                case DifficultyLevel.Medium:
                    baseSpeed = 13f;
                    maxSpeed = 24f;
                    speedAccelerationRate = 0.15f;
                    break;
                case DifficultyLevel.Hard:
                    baseSpeed = 17f;
                    maxSpeed = 30f;
                    speedAccelerationRate = 0.25f;
                    break;
            }
            currentSpeed = baseSpeed;
        }

        private void UpdatePowerUps()
        {
            if (isMagnetActive)
            {
                magnetTimer -= Time.deltaTime;
                if (magnetTimer <= 0f) isMagnetActive = false;
            }

            if (isMultiplierActive)
            {
                multiplierTimer -= Time.deltaTime;
                if (multiplierTimer <= 0f)
                {
                    isMultiplierActive = false;
                    scoreMultiplier = 1;
                }
            }

            if (isSprintActive)
            {
                sprintTimer -= Time.deltaTime;
                if (sprintTimer <= 0f)
                {
                    isSprintActive = false;
                    currentSpeed = baseSpeed;
                }
            }

            if (isKeyFinderActive)
            {
                keyFinderTimer -= Time.deltaTime;
                if (keyFinderTimer <= 0f) isKeyFinderActive = false;
            }
        }

        public void AddCoins(int amount)
        {
            coinsCollected += amount;
            score += amount * 10 * scoreMultiplier;
        }

        public void AddGem(int value, string type)
        {
            gemsCollected++;
            score += value * scoreMultiplier;
            coinsCollected += value / 5;
        }

        public void AddKey()
        {
            keysCollected++;
            score += 250 * scoreMultiplier;
        }

        public void ActivateMagnet(float duration)
        {
            isMagnetActive = true;
            magnetTimer = duration;
            magnetDuration = duration;
        }

        public void ActivateShield()
        {
            isShieldActive = true;
        }

        public void ActivateMultiplier(int mult, float duration)
        {
            isMultiplierActive = true;
            scoreMultiplier = mult;
            multiplierTimer = duration;
            multiplierDuration = duration;
        }

        public void ActivateSprint(float duration)
        {
            isSprintActive = true;
            sprintTimer = duration;
            sprintDuration = duration;
            currentSpeed = maxSpeed * 1.3f;
        }

        public void ActivateKeyFinder(float duration)
        {
            isKeyFinderActive = true;
            keyFinderTimer = duration;
            keyFinderDuration = duration;
            // Bonus: immediately grant score for every key already collected
            score += keysCollected * 100;
            Debug.Log("[Desert Hunt] Key Finder activated! Keys revealed for " + duration + "s");
        }

        public void ActivateTreasureBonus()
        {
            // Stacks a 500pt bonus redeemed at game-over
            treasureBonusScore += 500;
            score += 500 * scoreMultiplier;
            Debug.Log("[Desert Hunt] Treasure Bonus! +" + (500 * scoreMultiplier) + " pts");
        }

        public void GameOver()
        {
            if (currentState == GameState.GameOver) return;
            currentState = GameState.GameOver;

            if (score > highScore)
            {
                highScore = score;
                PlayerPrefs.SetInt("DesertHunt_HighScore", highScore);
                PlayerPrefs.Save();
            }

            if (UIManager.Instance != null)
            {
                UIManager.Instance.ShowGameOver(score, distanceRun, coinsCollected, gemsCollected, keysCollected, score >= highScore);
            }
        }

        public void PauseGame()
        {
            if (currentState == GameState.Playing)
            {
                currentState = GameState.Paused;
                Time.timeScale = 0f;
                if (UIManager.Instance != null) UManager_SetPause(true);
            }
            else if (currentState == GameState.Paused)
            {
                ResumeGame();
            }
        }

        public void ResumeGame()
        {
            currentState = GameState.Playing;
            Time.timeScale = 1f;
            if (UIManager.Instance != null) UManager_SetPause(false);
        }

        private void UManager_SetPause(bool isPaused)
        {
            if (UIManager.Instance != null)
            {
                UIManager.Instance.SetPauseMenuVisible(isPaused);
            }
        }

        public void RestartGame()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        public void LoadMainMenu()
        {
            Time.timeScale = 1f;
            try
            {
                SceneManager.LoadScene("MainMenu");
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning("[Desert Hunt] Could not load MainMenu directly: " + ex.Message + ". Falling back to scene reload.");
                RestartGame();
            }
        }

        private void LoadSettings()
        {
            int diffIndex = PlayerPrefs.GetInt("DesertHunt_Difficulty", 1);
            difficulty = (DifficultyLevel)diffIndex;
        }

        public void SaveDifficulty(DifficultyLevel level)
        {
            difficulty = level;
            PlayerPrefs.SetInt("DesertHunt_Difficulty", (int)level);
            PlayerPrefs.Save();
            ApplyDifficulty(level);
        }
    }
}
