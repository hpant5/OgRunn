using UnityEngine;
using Runn.Player;
using Runn.Ogre;

namespace Runn.Systems
{
    public class MapPingSystem : MonoBehaviour
    {
        public int MaxPingsPerLevel = GameConstants.MapRevealUsesPerLevel;
        public float FreezeDuration = GameConstants.MapRevealDurationSeconds;

        public int PingsRemaining { get; private set; }
        public bool IsActive { get; private set; }
        public float ActiveUntil { get; private set; }
        public float SecondsRemaining => IsActive ? Mathf.Max(0f, ActiveUntil - Time.time) : 0f;
        public Vector3 OgrePosition { get; private set; }

        public PlayerController Player;
        public OgreAI Ogre;
        public UI.MapPingOverlay Overlay;

        private void Start()
        {
            PingsRemaining = MaxPingsPerLevel;
        }

        private void Update()
        {
            if (UnityEngine.Input.GetKeyDown(KeyCode.M)) TryActivate();
            if (Ogre != null) OgrePosition = Ogre.transform.position;

            if (IsActive && Time.time >= ActiveUntil)
            {
                EndPing();
            }
        }

        public bool TryActivate()
        {
            if (IsActive || PingsRemaining <= 0) return false;
            PingsRemaining--;
            IsActive = true;
            ActiveUntil = Time.time + FreezeDuration;
            if (Player != null) Player.Frozen = true;
            if (Ogre != null) Ogre.Frozen = true;
            GameManager.Instance?.SetState(GameState.MapReveal);
            Overlay?.Show();
            return true;
        }

        private void EndPing()
        {
            IsActive = false;
            if (Player != null) Player.Frozen = false;
            if (Ogre != null) Ogre.Frozen = false;
            GameManager.Instance?.SetState(GameState.Playing);
            Overlay?.Hide();
        }

        public void GrantBonusPing()
        {
            PingsRemaining = Mathf.Min(PingsRemaining + 1, MaxPingsPerLevel);
        }
    }
}
