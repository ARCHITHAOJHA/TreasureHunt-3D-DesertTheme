using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace DesertTreasureHunt
{
    public class UIManager : MonoBehaviour
    {
        public static UIManager Instance { get; private set; }

        [Header("HUD Text Elements")]
        public Text scoreText;
        public Text distanceText;
        public Text coinsText;
        public Text gemsText;
        public Text keysText;
        public Text powerUpText;

        [Header("Heart Icons")]
        public List<Image> heartImages = new List<Image>();

        [Header("Panels")]
        public GameObject pausePanel;
        public GameObject gameOverPanel;

        [Header("Game Over Summary Texts")]
        public Text goFinalScoreText;
        public Text goDistanceText;
        public Text goCoinsText;
        public Text goGemsText;
        public Text goKeysText;
        public Text goHighScoreText;
        public GameObject newHighScoreBadge;

        private Canvas mainCanvas;

        private void Awake()
        {
            Instance = this;
            EnsureCanvasAndHUD();
        }

        private void Update()
        {
            if (GameManager.Instance == null) return;

            // Update live numbers
            if (scoreText != null) scoreText.text = $"SCORE: {GameManager.Instance.score:N0}";
            if (distanceText != null) distanceText.text = $"{Mathf.FloorToInt(GameManager.Instance.distanceRun)}m";
            if (coinsText != null) coinsText.text = $"{GameManager.Instance.coinsCollected}";
            if (gemsText != null) gemsText.text = $"{GameManager.Instance.gemsCollected}";
            if (keysText != null) keysText.text = $"{GameManager.Instance.keysCollected}";

            // Powerup status text
            if (powerUpText != null)
            {
                string pStatus = "";
                if (GameManager.Instance.isShieldActive)       pStatus += " [🛡 SHIELD]";
                if (GameManager.Instance.isMagnetActive)       pStatus += " [🧲 MAGNET]";
                if (GameManager.Instance.isMultiplierActive)   pStatus += " [⭐ 2X SCORE]";
                if (GameManager.Instance.isSprintActive)       pStatus += " [👟 SPEED BOOST]";
                if (GameManager.Instance.isKeyFinderActive)    pStatus += " [🗝 KEY FINDER]";
                powerUpText.text = pStatus;
            }

            // Shortcut for Pause (Works with both New Input System and Legacy Input)
            if (DesertInput.GetPause())
            {
                GameManager.Instance.PauseGame();
            }
        }

        public void UpdateHearts(int currentHearts, int maxHearts)
        {
            for (int i = 0; i < heartImages.Count; i++)
            {
                if (heartImages[i] != null)
                {
                    heartImages[i].color = (i < currentHearts) ? new Color(1f, 0.2f, 0.25f, 1f) : new Color(0.25f, 0.25f, 0.25f, 0.4f);
                }
            }
        }

        public void SetPauseMenuVisible(bool visible)
        {
            if (pausePanel != null) pausePanel.SetActive(visible);
        }

        public void ShowGameOver(int finalScore, float distance, int coins, int gems, int keys, bool isHighScore)
        {
            if (gameOverPanel != null)
            {
                gameOverPanel.SetActive(true);
                if (goFinalScoreText != null) goFinalScoreText.text = $"SCORE: {finalScore:N0}";
                if (goDistanceText != null) goDistanceText.text = $"Distance: {Mathf.FloorToInt(distance)}m";
                if (goCoinsText != null) goCoinsText.text = $"Coins: {coins}";
                if (goGemsText != null) goGemsText.text = $"Gems: {gems}";
                if (goKeysText != null) goKeysText.text = $"Keys: {keys}";
                if (goHighScoreText != null) goHighScoreText.text = $"BEST: {GameManager.Instance.highScore:N0}";
                if (newHighScoreBadge != null) newHighScoreBadge.SetActive(isHighScore);
            }
        }

        private void EnsureCanvasAndHUD()
        {
            EventSystem existingEs = FindFirstObjectByType<EventSystem>();
            GameObject es = (existingEs != null) ? existingEs.gameObject : new GameObject("EventSystem");
            EventSystemHelper.EnsureCorrectInputModule(es);

            mainCanvas = FindFirstObjectByType<Canvas>();
            if (mainCanvas == null)
            {
                GameObject canvasObj = new GameObject("MainUICanvas");
                mainCanvas = canvasObj.AddComponent<Canvas>();
                mainCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvasObj.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                canvasObj.AddComponent<GraphicRaycaster>();
            }

            // Build UI Elements if they don't exist
            Font standardFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (standardFont == null) standardFont = Font.CreateDynamicFontFromOSFont("Arial", 16);

            // 1. Top HUD Bar
            GameObject hudRoot = new GameObject("HUD_Root");
            hudRoot.transform.SetParent(mainCanvas.transform, false);

            // Distance meter (Center)
            distanceText = CreateText("DistanceText", hudRoot.transform, new Vector2(0.5f, 1f), new Vector2(0f, -35f), "0m", standardFont, 36, Color.white, TextAnchor.MiddleCenter);

            // Score (Center below distance)
            scoreText = CreateText("ScoreText", hudRoot.transform, new Vector2(0.5f, 1f), new Vector2(0f, -75f), "SCORE: 0", standardFont, 24, new Color(1f, 0.88f, 0.4f), TextAnchor.MiddleCenter);

            // Coins counter (Top Right)
            GameObject coinIcon = CreateIcon("CoinIcon", hudRoot.transform, new Vector2(1f, 1f), new Vector2(-140f, -35f), new Vector2(30f, 30f), new Color(1f, 0.84f, 0f));
            coinsText = CreateText("CoinsText", hudRoot.transform, new Vector2(1f, 1f), new Vector2(-80f, -35f), "0", standardFont, 28, new Color(1f, 0.9f, 0.2f), TextAnchor.MiddleLeft);

            // Gems counter (Top Right below coins)
            GameObject gemIcon = CreateIcon("GemIcon", hudRoot.transform, new Vector2(1f, 1f), new Vector2(-140f, -70f), new Vector2(24f, 24f), new Color(0.2f, 0.6f, 1f));
            gemsText = CreateText("GemsText", hudRoot.transform, new Vector2(1f, 1f), new Vector2(-80f, -70f), "0", standardFont, 22, new Color(0.4f, 0.8f, 1f), TextAnchor.MiddleLeft);

            // Pause Button (Top Right corner)
            GameObject pauseBtn = CreateButton("PauseButton", hudRoot.transform, new Vector2(1f, 1f), new Vector2(-35f, -35f), new Vector2(40f, 40f), "||", standardFont, 22);
            pauseBtn.GetComponent<Button>().onClick.AddListener(() => GameManager.Instance.PauseGame());

            // 3 Hearts (Top Left - matching user reference image)
            heartImages.Clear();
            for (int i = 0; i < 3; i++)
            {
                GameObject heart = CreateIcon($"Heart_{i}", hudRoot.transform, new Vector2(0f, 1f), new Vector2(40f + (i * 38f), -35f), new Vector2(32f, 32f), new Color(1f, 0.2f, 0.25f));
                heartImages.Add(heart.GetComponent<Image>());
            }

            // Power-up status text
            powerUpText = CreateText("PowerUpText", hudRoot.transform, new Vector2(0.5f, 1f), new Vector2(0f, -115f), "", standardFont, 22, new Color(0.3f, 1f, 0.5f), TextAnchor.MiddleCenter);

            // 2. Pause Menu Modal
            BuildPausePanel(standardFont);

            // 3. Game Over Modal
            BuildGameOverPanel(standardFont);
        }

        private void BuildPausePanel(Font font)
        {
            pausePanel = new GameObject("PausePanel");
            pausePanel.transform.SetParent(mainCanvas.transform, false);
            RectTransform rt = pausePanel.AddComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.sizeDelta = Vector2.zero;

            Image bg = pausePanel.AddComponent<Image>();
            bg.color = new Color(0.1f, 0.08f, 0.05f, 0.85f);

            CreateText("PauseTitle", pausePanel.transform, new Vector2(0.5f, 0.5f), new Vector2(0f, 120f), "GAME PAUSED", font, 44, new Color(1f, 0.85f, 0.4f), TextAnchor.MiddleCenter);

            GameObject resumeBtn = CreateButton("ResumeBtn", pausePanel.transform, new Vector2(0.5f, 0.5f), new Vector2(0f, 30f), new Vector2(220f, 50f), "RESUME", font, 24);
            resumeBtn.GetComponent<Button>().onClick.AddListener(() => GameManager.Instance.ResumeGame());

            GameObject restartBtn = CreateButton("RestartBtn", pausePanel.transform, new Vector2(0.5f, 0.5f), new Vector2(0f, -35f), new Vector2(220f, 50f), "RETRY RUN", font, 24);
            restartBtn.GetComponent<Button>().onClick.AddListener(() => GameManager.Instance.RestartGame());

            GameObject menuBtn = CreateButton("MenuBtn", pausePanel.transform, new Vector2(0.5f, 0.5f), new Vector2(0f, -100f), new Vector2(220f, 50f), "MAIN MENU", font, 24);
            menuBtn.GetComponent<Button>().onClick.AddListener(() => GameManager.Instance.LoadMainMenu());

            pausePanel.SetActive(false);
        }

        private void BuildGameOverPanel(Font font)
        {
            gameOverPanel = new GameObject("GameOverPanel");
            gameOverPanel.transform.SetParent(mainCanvas.transform, false);
            RectTransform rt = gameOverPanel.AddComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.sizeDelta = Vector2.zero;

            Image bg = gameOverPanel.AddComponent<Image>();
            bg.color = new Color(0.12f, 0.06f, 0.02f, 0.92f);

            CreateText("GOTitle", gameOverPanel.transform, new Vector2(0.5f, 0.5f), new Vector2(0f, 150f), "RUINS CONQUERED", font, 46, new Color(1f, 0.3f, 0.2f), TextAnchor.MiddleCenter);

            goFinalScoreText = CreateText("GOScore", gameOverPanel.transform, new Vector2(0.5f, 0.5f), new Vector2(0f, 90f), "SCORE: 0", font, 36, new Color(1f, 0.88f, 0.35f), TextAnchor.MiddleCenter);
            goDistanceText = CreateText("GODist", gameOverPanel.transform, new Vector2(0.5f, 0.5f), new Vector2(0f, 45f), "Distance: 0m", font, 24, Color.white, TextAnchor.MiddleCenter);
            goCoinsText = CreateText("GOCoins", gameOverPanel.transform, new Vector2(0.5f, 0.5f), new Vector2(0f, 10f), "Coins: 0", font, 22, new Color(1f, 0.84f, 0f), TextAnchor.MiddleCenter);
            goGemsText = CreateText("GOGems", gameOverPanel.transform, new Vector2(0.5f, 0.5f), new Vector2(0f, -20f), "Gems: 0", font, 22, new Color(0.3f, 0.7f, 1f), TextAnchor.MiddleCenter);
            goHighScoreText = CreateText("GOBest", gameOverPanel.transform, new Vector2(0.5f, 0.5f), new Vector2(0f, -55f), "BEST: 0", font, 22, new Color(0.9f, 0.7f, 0.3f), TextAnchor.MiddleCenter);

            GameObject playAgainBtn = CreateButton("PlayAgainBtn", gameOverPanel.transform, new Vector2(0.5f, 0.5f), new Vector2(0f, -120f), new Vector2(240f, 55f), "PLAY AGAIN", font, 26);
            playAgainBtn.GetComponent<Button>().onClick.AddListener(() => GameManager.Instance.RestartGame());

            GameObject menuBtn = CreateButton("MenuBtn", gameOverPanel.transform, new Vector2(0.5f, 0.5f), new Vector2(0f, -185f), new Vector2(240f, 45f), "MAIN MENU", font, 20);
            menuBtn.GetComponent<Button>().onClick.AddListener(() => GameManager.Instance.LoadMainMenu());

            gameOverPanel.SetActive(false);
        }

        private Text CreateText(string name, Transform parent, Vector2 anchor, Vector2 anchoredPos, string initialText, Font font, int fontSize, Color color, TextAnchor alignment)
        {
            GameObject obj = new GameObject(name);
            obj.transform.SetParent(parent, false);

            RectTransform rt = obj.AddComponent<RectTransform>();
            rt.anchorMin = anchor;
            rt.anchorMax = anchor;
            rt.pivot = anchor;
            rt.anchoredPosition = anchoredPos;
            rt.sizeDelta = new Vector2(400f, 50f);

            Text t = obj.AddComponent<Text>();
            t.font = font;
            t.fontSize = fontSize;
            t.color = color;
            t.text = initialText;
            t.alignment = alignment;

            return t;
        }

        private GameObject CreateIcon(string name, Transform parent, Vector2 anchor, Vector2 anchoredPos, Vector2 size, Color color)
        {
            GameObject obj = new GameObject(name);
            obj.transform.SetParent(parent, false);

            RectTransform rt = obj.AddComponent<RectTransform>();
            rt.anchorMin = anchor;
            rt.anchorMax = anchor;
            rt.pivot = anchor;
            rt.anchoredPosition = anchoredPos;
            rt.sizeDelta = size;

            Image img = obj.AddComponent<Image>();
            img.color = color;

            return obj;
        }

        private GameObject CreateButton(string name, Transform parent, Vector2 anchor, Vector2 anchoredPos, Vector2 size, string label, Font font, int fontSize)
        {
            GameObject obj = new GameObject(name);
            obj.transform.SetParent(parent, false);

            RectTransform rt = obj.AddComponent<RectTransform>();
            rt.anchorMin = anchor;
            rt.anchorMax = anchor;
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = anchoredPos;
            rt.sizeDelta = size;

            Image img = obj.AddComponent<Image>();
            img.color = new Color(0.85f, 0.55f, 0.2f); // Desert sandstone orange

            Button btn = obj.AddComponent<Button>();
            ColorBlock cb = btn.colors;
            cb.highlightedColor = new Color(0.95f, 0.70f, 0.25f);
            cb.pressedColor = new Color(0.70f, 0.40f, 0.15f);
            btn.colors = cb;

            // Label
            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(obj.transform, false);
            RectTransform trt = textObj.AddComponent<RectTransform>();
            trt.anchorMin = Vector2.zero;
            trt.anchorMax = Vector2.one;
            trt.sizeDelta = Vector2.zero;

            Text t = textObj.AddComponent<Text>();
            t.font = font;
            t.fontSize = fontSize;
            t.text = label;
            t.color = Color.white;
            t.alignment = TextAnchor.MiddleCenter;
            t.raycastTarget = false;

            return obj;
        }
    }
}
