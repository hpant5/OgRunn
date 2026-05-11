using System.Collections.Generic;
using UnityEngine;
using Runn.Level;
using Runn.Ogre;
using Runn.Player;
using Runn.UI;

namespace Runn.Systems
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        public int CurrentLevelIndex = 1;
        public int MaxLevels = 5;

        public LevelData ActiveLevel { get; private set; }
        public PlayerController Player;
        public OgreAI Ogre;
        public HUD Hud;
        public MainMenuScreen MainMenu;
        public PauseScreen PauseScreen;
        public DeathScreen DeathScreen;
        public LevelCompleteScreen LevelCompleteScreen;
        public MapPingSystem MapPing;

        public List<Bench> Benches = new List<Bench>();
        public ExitDoor ExitDoor;

        public GameState State { get; private set; } = GameState.MainMenu;
        public bool IsPlayerHidden { get; private set; }
        public bool RunOver { get; private set; }
        public Bench HidingBench { get; private set; }
        public float HideStartedAt { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        public void RegisterLevel(LevelData level)
        {
            ActiveLevel = level;
            RunOver = false;
            IsPlayerHidden = false;
            HidingBench = null;
            ReturnToMainMenu();
        }

        public void SetState(GameState state)
        {
            State = state;
        }

        public void StartPlaying()
        {
            RunOver = false;
            SetState(GameState.Playing);
            MainMenu?.Hide();
            PauseScreen?.Hide();
            if (Player != null) Player.Frozen = false;
            if (Ogre != null) Ogre.Frozen = false;
        }

        public void ReturnToMainMenu()
        {
            RunOver = false;
            SetState(GameState.MainMenu);
            if (Player != null) Player.Frozen = true;
            if (Ogre != null) Ogre.Frozen = true;
            PauseScreen?.Hide();
            DeathScreen?.gameObject.SetActive(false);
            LevelCompleteScreen?.gameObject.SetActive(false);
            MainMenu?.Show();
            Hud?.ShowMessage("OgreRunn ready");
        }

        public void TogglePause()
        {
            if (State == GameState.Playing)
            {
                SetState(GameState.Paused);
                if (Player != null) Player.Frozen = true;
                if (Ogre != null) Ogre.Frozen = true;
                PauseScreen?.Show();
                Hud?.ShowMessage("Paused");
            }
            else if (State == GameState.Paused)
            {
                StartPlaying();
                Hud?.ShowMessage("Resume");
            }
        }

        public Bench NearestBenchInRange()
        {
            if (Player == null) return null;
            Bench best = null;
            float bestDist = float.MaxValue;
            foreach (var b in Benches)
            {
                if (b == null) continue;
                if (!b.PlayerInRange(Player.transform.position)) continue;
                float d = (b.transform.position - Player.transform.position).sqrMagnitude;
                if (d < bestDist) { bestDist = d; best = b; }
            }
            return best;
        }

        public bool TryToggleHide()
        {
            if (RunOver || State != GameState.Playing) return false;
            var bench = NearestBenchInRange();
            Hud?.ShowMessage(bench == null ? "Stand under a bench to hide" : "Hidden under bench");
            return bench != null;
        }

        private void Update()
        {
            if (Player == null) return;
            if (UnityEngine.Input.GetKeyDown(KeyCode.P)) TogglePause();
            if (UnityEngine.Input.GetKeyDown(KeyCode.R)) Restart();

            if (State == GameState.Playing)
            {
                UpdateHidingState();
            }

            if (ExitDoor != null && !RunOver && State == GameState.Playing && ExitDoor.CanExit(Player.transform.position, true))
            {
                CompleteLevel();
            }
        }

        private void UpdateHidingState()
        {
            var bench = NearestBenchInRange();
            if (bench == null)
            {
                if (IsPlayerHidden) Hud?.ShowMessage("Visible");
                IsPlayerHidden = false;
                HidingBench = null;
                return;
            }

            if (IsPlayerHidden && HidingBench == bench) return;

            bool ogreSawIt = Ogre != null && Ogre.Senses != null && Ogre.Senses.CanSee(Player.transform, false);
            IsPlayerHidden = true;
            HidingBench = bench;
            HideStartedAt = Time.time;
            Hud?.ShowMessage("Hidden");

            if (ogreSawIt && Ogre != null)
            {
                Ogre.OnSawHide(bench.transform.position);
            }
        }

        public void OnPlayerCaught(bool sawHiding)
        {
            if (RunOver) return;
            RunOver = true;
            SetState(GameState.GameOver);
            if (Player != null) Player.Frozen = true;
            if (Ogre != null) Ogre.Frozen = true;
            DeathScreen?.Show(sawHiding);
        }

        public void CompleteLevel()
        {
            if (RunOver) return;
            RunOver = true;
            SetState(GameState.LevelComplete);
            if (Player != null) Player.Frozen = true;
            if (Ogre != null) Ogre.Frozen = true;
            LevelCompleteScreen?.Show(CurrentLevelIndex);
        }

        public void Restart()
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex);
        }

        public void NextLevel()
        {
            CurrentLevelIndex = Mathf.Clamp(CurrentLevelIndex + 1, 1, MaxLevels);
            UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex);
        }
    }
}
