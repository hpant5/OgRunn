using UnityEngine;
using Runn.UI;

namespace Runn.Ogre
{
    public class OgreSpeech : MonoBehaviour
    {
        public HUD Hud;
        public float MinIntervalBetweenLines = 4f;
        private float _lastSpoken = -10f;

        public void Say(string line, float duration = 2.5f)
        {
            if (Time.time - _lastSpoken < MinIntervalBetweenLines) return;
            _lastSpoken = Time.time;
            if (Hud != null) Hud.ShowOgreLine(line, duration);
        }
    }
}
