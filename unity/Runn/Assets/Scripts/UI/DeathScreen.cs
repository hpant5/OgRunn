using UnityEngine;
using UnityEngine.UI;
using Runn.Systems;

namespace Runn.UI
{
    public class DeathScreen : MonoBehaviour
    {
        public Text TitleText;
        public Text BodyText;
        public Button RestartButton;
        public Button MainMenuButton;
        public GameManager Game;

        private void Start()
        {
            if (RestartButton != null) RestartButton.onClick.AddListener(() => Game?.Restart());
            if (MainMenuButton != null) MainMenuButton.onClick.AddListener(() => Game?.ReturnToMainMenu());
            gameObject.SetActive(false);
        }

        public void Show(bool sawHiding)
        {
            gameObject.SetActive(true);
            if (TitleText != null) TitleText.text = "GAME OVER";
            if (BodyText != null) BodyText.text = sawHiding ? "Ogre laugh: I saw you hiding." : "Ogre laugh: you were caught.";
        }
    }
}
