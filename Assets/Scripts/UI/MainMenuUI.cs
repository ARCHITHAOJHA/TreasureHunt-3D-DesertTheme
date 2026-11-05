using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace DesertTreasureHunt
{
    public class MainMenuUI : MonoBehaviour
    {
        public Text highScoreText;
        public GameObject instructionsPanel;
        public Text selectedDifficultyText;

        private DifficultyLevel currentDifficulty = DifficultyLevel.Medium;

        private void Start()
        {
            EventSystem existingEs = FindFirstObjectByType<EventSystem>();
            GameObject es = (existingEs != null) ? existingEs.gameObject : new GameObject("EventSystem");
            EventSystemHelper.EnsureCorrectInputModule(es);

            currentDifficulty = (DifficultyLevel)PlayerPrefs.GetInt("DesertHunt_Difficulty", 1);
            int high = PlayerPrefs.GetInt("DesertHunt_HighScore", 0);

            BuildMenuUI(high);
            UpdateDifficultyUI();
        }

        public void PlayGame()
        {
            PlayerPrefs.SetInt("DesertHunt_Difficulty", (int)currentDifficulty);
            PlayerPrefs.Save();
            SceneManager.LoadScene("MainGame");
        }

        public void SetDifficulty(int level)
        {
            currentDifficulty = (DifficultyLevel)level;
            PlayerPrefs.SetInt("DesertHunt_Difficulty", level);
            PlayerPrefs.Save();
            UpdateDifficultyUI();
        }

        private void UpdateDifficultyUI()
        {
            if (selectedDifficultyText != null)
            {
                selectedDifficultyText.text = $"DIFFICULTY: {currentDifficulty.ToString().ToUpper()}";
                selectedDifficultyText.color = currentDifficulty switch
                {
                    DifficultyLevel.Easy => new Color(0.4f, 0.9f, 0.4f),
                    DifficultyLevel.Medium => new Color(1.0f, 0.85f, 0.2f),
                    DifficultyLevel.Hard => new Color(1.0f, 0.35f, 0.25f),
                    _ => Color.white
                };
            }
        }

        public void ToggleInstructions(bool show)
        {
            if (instructionsPanel != null)
            {
                instructionsPanel.SetActive(show);
            }
        }

        private void BuildMenuUI(int highScore)
        {
            Canvas canvas = FindFirstObjectByType<Canvas>();
            if (canvas == null)
            {
                GameObject canvasObj = new GameObject("MainMenuCanvas");
                canvas = canvasObj.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvasObj.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                canvasObj.AddComponent<GraphicRaycaster>();
            }

            Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (font == null) font = Font.CreateDynamicFontFromOSFont("Arial", 16);

            // Background Panel
            GameObject bgObj = new GameObject("MenuBG");
            bgObj.transform.SetParent(canvas.transform, false);
            RectTransform bgRt = bgObj.AddComponent<RectTransform>();
            bgRt.anchorMin = Vector2.zero;
            bgRt.anchorMax = Vector2.one;
            bgRt.sizeDelta = Vector2.zero;
            Image bgImg = bgObj.AddComponent<Image>();
            bgImg.color = new Color(0.18f, 0.12f, 0.08f, 0.96f);
            bgImg.raycastTarget = false;

            // Title
            CreateText("GameTitle", canvas.transform, new Vector2(0.5f, 0.5f), new Vector2(0f, 160f), "DESERT TREASURE HUNT 3D", font, 44, new Color(1f, 0.82f, 0.25f), TextAnchor.MiddleCenter);
            CreateText("GameSubtitle", canvas.transform, new Vector2(0.5f, 0.5f), new Vector2(0f, 115f), "~ ANCIENT RUINS EXPEDITION ~", font, 20, new Color(0.9f, 0.72f, 0.45f), TextAnchor.MiddleCenter);

            // High Score
            highScoreText = CreateText("HighScoreText", canvas.transform, new Vector2(0.5f, 0.5f), new Vector2(0f, 70f), $"BEST SCORE: {highScore:N0}", font, 24, new Color(1f, 0.95f, 0.6f), TextAnchor.MiddleCenter);

            // Difficulty Label
            selectedDifficultyText = CreateText("DiffLabel", canvas.transform, new Vector2(0.5f, 0.5f), new Vector2(0f, 25f), "DIFFICULTY: MEDIUM", font, 22, Color.white, TextAnchor.MiddleCenter);

            // Difficulty Buttons (Easy, Medium, Hard)
            GameObject easyBtn = CreateButton("EasyBtn", canvas.transform, new Vector2(0.5f, 0.5f), new Vector2(-120f, -25f), new Vector2(100f, 38f), "EASY", font, 16);
            easyBtn.GetComponent<Button>().onClick.AddListener(() => SetDifficulty(0));

            GameObject medBtn = CreateButton("MedBtn", canvas.transform, new Vector2(0.5f, 0.5f), new Vector2(0f, -25f), new Vector2(110f, 38f), "MEDIUM", font, 16);
            medBtn.GetComponent<Button>().onClick.AddListener(() => SetDifficulty(1));

            GameObject hardBtn = CreateButton("HardBtn", canvas.transform, new Vector2(0.5f, 0.5f), new Vector2(120f, -25f), new Vector2(100f, 38f), "HARD", font, 16);
            hardBtn.GetComponent<Button>().onClick.AddListener(() => SetDifficulty(2));

            // Play Button
            GameObject playBtn = CreateButton("PlayBtn", canvas.transform, new Vector2(0.5f, 0.5f), new Vector2(0f, -85f), new Vector2(260f, 55f), "START EXPEDITION", font, 24);
            playBtn.GetComponent<Button>().onClick.AddListener(PlayGame);

            // How To Play Button
            GameObject howBtn = CreateButton("HowToPlayBtn", canvas.transform, new Vector2(0.5f, 0.5f), new Vector2(0f, -150f), new Vector2(200f, 40f), "HOW TO PLAY", font, 18);
            howBtn.GetComponent<Button>().onClick.AddListener(() => ToggleInstructions(true));

            // Instructions Modal
            BuildInstructionsModal(canvas.transform, font);
        }

        private void BuildInstructionsModal(Transform parent, Font font)
        {
            instructionsPanel = new GameObject("InstructionsModal");
            instructionsPanel.transform.SetParent(parent, false);

            RectTransform rt = instructionsPanel.AddComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.sizeDelta = Vector2.zero;

            Image bg = instructionsPanel.AddComponent<Image>();
            bg.color = new Color(0.08f, 0.05f, 0.03f, 0.95f);

            CreateText("InsTitle", instructionsPanel.transform, new Vector2(0.5f, 0.5f), new Vector2(0f, 150f), "HOW TO PLAY", font, 36, new Color(1f, 0.85f, 0.3f), TextAnchor.MiddleCenter);

            string bodyText =
                "CONTROLS:\n" +
                "- Left / Right Arrow or A/D or Swipe: Change Lanes\n" +
                "- Up Arrow / W / Space or Swipe Up: Jump over spikes and gaps\n" +
                "- Down Arrow / S or Swipe Down: Slide under archways and traps\n\n" +
                "OBJECTIVES:\n" +
                "- Run as far as possible through ancient desert ruins\n" +
                "- Collect Golden Coins (+10), Rubies (+50), and Sapphires (+100)\n" +
                "- Grab Ancient Keys to unlock bonus treasure chests!\n" +
                "- Power-ups: Magnet attracts coins, Shield absorbs 1 trap hit\n" +
                "- Avoid traps and pits to protect your 3 Hearts!";

            CreateText("InsBody", instructionsPanel.transform, new Vector2(0.5f, 0.5f), new Vector2(0f, 0f), bodyText, font, 18, Color.white, TextAnchor.MiddleCenter);

            GameObject closeBtn = CreateButton("CloseBtn", instructionsPanel.transform, new Vector2(0.5f, 0.5f), new Vector2(0f, -160f), new Vector2(160f, 45f), "GOT IT!", font, 20);
            closeBtn.GetComponent<Button>().onClick.AddListener(() => ToggleInstructions(false));

            instructionsPanel.SetActive(false);
        }

        private Text CreateText(string name, Transform parent, Vector2 anchor, Vector2 anchoredPos, string text, Font font, int fontSize, Color color, TextAnchor alignment)
        {
            GameObject obj = new GameObject(name);
            obj.transform.SetParent(parent, false);

            RectTransform rt = obj.AddComponent<RectTransform>();
            rt.anchorMin = anchor;
            rt.anchorMax = anchor;
            rt.pivot = anchor;
            rt.anchoredPosition = anchoredPos;
            rt.sizeDelta = new Vector2(650f, 220f);

            Text t = obj.AddComponent<Text>();
            t.font = font;
            t.fontSize = fontSize;
            t.color = color;
            t.text = text;
            t.alignment = alignment;

            return t;
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
            img.color = new Color(0.85f, 0.55f, 0.2f);

            Button btn = obj.AddComponent<Button>();
            ColorBlock cb = btn.colors;
            cb.highlightedColor = new Color(0.95f, 0.70f, 0.25f);
            cb.pressedColor = new Color(0.65f, 0.38f, 0.12f);
            btn.colors = cb;

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
