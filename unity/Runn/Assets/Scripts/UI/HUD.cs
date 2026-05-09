using UnityEngine;
using UnityEngine.UI;
using Runn.Systems;

namespace Runn.UI
{
    public class HUD : MonoBehaviour
    {
        public Text LevelText;
        public Text PingsText;
        public Text MessageText;
        public Text OgreLineText;
        public Button MapButton;
        public Button HideButton;
        public Button PauseButton;

        public MapPingSystem MapPing;
        public GameManager Game;

        private float _messageUntil;
        private float _ogreLineUntil;

        private void Start()
        {
            if (MapButton != null) MapButton.onClick.AddListener(OnMapPressed);
            if (HideButton != null) HideButton.onClick.AddListener(OnHidePressed);
            if (PauseButton != null) PauseButton.onClick.AddListener(OnPausePressed);
        }

        private void Update()
        {
            if (Game != null && Game.ActiveLevel != null && LevelText != null)
                LevelText.text = $"LEVEL {Game.CurrentLevelIndex}";

            if (MapPing != null && PingsText != null)
                PingsText.text = $"MAP x{MapPing.PingsRemaining}";

            if (MessageText != null)
                MessageText.gameObject.SetActive(Time.time < _messageUntil);

            if (OgreLineText != null)
                OgreLineText.gameObject.SetActive(Time.time < _ogreLineUntil);
        }

        public void ShowMessage(string text, float duration = 2f)
        {
            if (MessageText == null) return;
            MessageText.text = text;
            _messageUntil = Time.time + duration;
        }

        public void ShowOgreLine(string text, float duration = 2.5f)
        {
            if (OgreLineText == null) return;
            OgreLineText.text = text;
            _ogreLineUntil = Time.time + duration;
        }

        private void OnMapPressed()
        {
            if (MapPing != null) MapPing.TryActivate();
        }

        private void OnHidePressed()
        {
            Game?.TryToggleHide();
        }

        private void OnPausePressed()
        {
            Game?.TogglePause();
        }
    }
}
