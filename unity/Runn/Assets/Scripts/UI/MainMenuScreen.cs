using UnityEngine;
using UnityEngine.UI;
using Runn.Systems;

namespace Runn.UI
{
    public class MainMenuScreen : MonoBehaviour
    {
        public Button PlayButton;
        public Button LevelSelectButton;
        public Button SettingsButton;
        public Text PlaceholderText;
        public GameManager Game;

        private void Start()
        {
            if (PlayButton != null) PlayButton.onClick.AddListener(OnPlay);
            if (LevelSelectButton != null) LevelSelectButton.onClick.AddListener(() => ShowPlaceholder("Level select coming after V1 core is stable."));
            if (SettingsButton != null) SettingsButton.onClick.AddListener(() => ShowPlaceholder("Settings placeholder"));
        }

        public void Show()
        {
            gameObject.SetActive(true);
            Debug.Log("[Runn] Main menu shown.");
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }

        private void OnPlay()
        {
            Hide();
            Game?.StartPlaying();
        }

        private void ShowPlaceholder(string text)
        {
            if (PlaceholderText != null) PlaceholderText.text = text;
        }
    }
}
