using UnityEngine;
using UnityEngine.UI;
using Runn.Systems;

namespace Runn.UI
{
    public class PauseScreen : MonoBehaviour
    {
        public Button ResumeButton;
        public Button RestartButton;
        public Button MainMenuButton;
        public GameManager Game;

        private void Start()
        {
            if (ResumeButton != null) ResumeButton.onClick.AddListener(() => Game?.TogglePause());
            if (RestartButton != null) RestartButton.onClick.AddListener(() => Game?.Restart());
            if (MainMenuButton != null) MainMenuButton.onClick.AddListener(() => Game?.ReturnToMainMenu());
            gameObject.SetActive(false);
        }

        public void Show()
        {
            gameObject.SetActive(true);
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }
    }
}
