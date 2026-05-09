using UnityEngine;
using UnityEngine.UI;
using Runn.Systems;

namespace Runn.UI
{
    public class LevelCompleteScreen : MonoBehaviour
    {
        public Text TitleText;
        public Button NextButton;
        public Button ReplayButton;
        public Button MainMenuButton;
        public GameManager Game;

        private void Start()
        {
            if (NextButton != null) NextButton.onClick.AddListener(() => Game?.NextLevel());
            if (ReplayButton != null) ReplayButton.onClick.AddListener(() => Game?.Restart());
            if (MainMenuButton != null) MainMenuButton.onClick.AddListener(() => Game?.ReturnToMainMenu());
            gameObject.SetActive(false);
        }

        public void Show(int levelIndex)
        {
            gameObject.SetActive(true);
            if (TitleText != null) TitleText.text = $"ESCAPED! LEVEL {levelIndex} COMPLETE";
        }
    }
}
