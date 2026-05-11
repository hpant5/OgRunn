using UnityEngine;
using UnityEngine.UI;
using Runn.Level;
using Runn.Ogre;
using Runn.Player;
using Runn.UI;

namespace Runn.Systems
{
    public class GameBootstrap : MonoBehaviour
    {
        public int LevelIndex = 1;
        public Material WallMaterial;
        public Material FloorMaterial;
        public Material PlayerMaterial;
        public Material OgreMaterial;
        public Material BenchMaterial;
        public Material KeyMaterial;
        public Material DoorMaterial;

        private const int IgnoreRaycastLayer = 2;
        private const int DefaultLayer = 0;

        private void Start()
        {
            int idx = Mathf.Clamp(LevelIndex, 1, 5);
            var level = LevelLoader.LoadFromResources(idx);
            if (level == null)
            {
                Debug.LogError("[Runn] Failed to load level.");
                return;
            }

            EnsureMaterials();
            EnsureLighting();
            EnsureEventSystem();

            int wallLayerMask = 1 << DefaultLayer;

            var mazeGo = new GameObject("MazeRoot");
            var maze = mazeGo.AddComponent<MazeBuilder>();
            maze.WallLayer = wallLayerMask;
            maze.Build(level, WallMaterial, FloorMaterial);

            var player = SpawnPlayer(level);
            var ogre = SpawnOgre(level);
            SpawnBenches(level, out var benches);
            var door = SpawnDoor(level, false);

            var ui = BuildUI(out var hud, out var mainMenu, out var pauseScreen, out var deathScreen, out var levelComplete, out var mapOverlay, out var touchInput);

            var managerGo = new GameObject("GameManager");
            var gm = managerGo.AddComponent<GameManager>();
            var mapPing = managerGo.AddComponent<MapPingSystem>();
            var camera = SetupCamera(player.transform);

            var pc = player.GetComponent<PlayerController>();
            pc.CameraRig = camera.transform;
            pc.Input = touchInput;
            var pv = player.GetComponent<PlayerVision>();
            pv.WallMask = wallLayerMask;
            pv.ViewDistance = GameConstants.PlayerVisibilityRadius;

            var senses = ogre.GetComponentInChildren<OgreSenses>();
            senses.WallMask = wallLayerMask;
            senses.ViewDistance = GameConstants.OgreDetectionRadius;
            var ai = ogre.GetComponent<OgreAI>();
            ai.BaseSpeed = GameConstants.OgreSpeed;
            var speech = ogre.GetComponent<OgreSpeech>();
            speech.Hud = hud;
            ai.Init(level, player.transform, senses, speech);

            mapPing.Player = pc;
            mapPing.Ogre = ai;
            mapPing.Overlay = mapOverlay;

            hud.MapPing = mapPing;
            hud.Game = gm;
            mainMenu.Game = gm;
            pauseScreen.Game = gm;
            mapOverlay.Game = gm;
            mapOverlay.MapPing = mapPing;
            deathScreen.Game = gm;
            levelComplete.Game = gm;

            gm.CurrentLevelIndex = idx;
            gm.MaxLevels = 5;
            gm.Player = pc;
            gm.Ogre = ai;
            gm.Hud = hud;
            gm.MainMenu = mainMenu;
            gm.PauseScreen = pauseScreen;
            gm.DeathScreen = deathScreen;
            gm.LevelCompleteScreen = levelComplete;
            gm.MapPing = mapPing;
            gm.Benches = benches;
            gm.ExitDoor = door;
            gm.RegisterLevel(level);
        }

        private void EnsureMaterials()
        {
            if (WallMaterial == null) WallMaterial = MakeMat(new Color(0.38f, 0.42f, 0.52f));
            if (FloorMaterial == null) FloorMaterial = MakeMat(new Color(0.16f, 0.17f, 0.22f));
            if (PlayerMaterial == null) PlayerMaterial = MakeMat(new Color(0.92f, 0.94f, 1f));
            if (OgreMaterial == null) OgreMaterial = MakeMat(new Color(0.75f, 0.18f, 0.16f));
            if (BenchMaterial == null) BenchMaterial = MakeMat(new Color(0.78f, 0.55f, 0.28f));
            if (KeyMaterial == null) KeyMaterial = MakeMat(new Color(0.3f, 0.85f, 0.95f));
            if (DoorMaterial == null) DoorMaterial = MakeMat(new Color(0.3f, 0.85f, 0.4f));
        }

        private Material MakeMat(Color c)
        {
            var shader = Shader.Find("Universal Render Pipeline/Lit")
                ?? Shader.Find("Universal Render Pipeline/Unlit")
                ?? Shader.Find("Standard")
                ?? Shader.Find("Unlit/Color")
                ?? Shader.Find("Sprites/Default")
                ?? Shader.Find("Legacy Shaders/Diffuse")
                ?? Shader.Find("Mobile/Diffuse");
            if (shader == null)
            {
                Debug.LogError("[Runn] No compatible built-in shader found. Using Unity primitive defaults.");
                return null;
            }

            var m = new Material(shader);
            if (m.HasProperty("_BaseColor")) m.SetColor("_BaseColor", c);
            if (m.HasProperty("_Color")) m.SetColor("_Color", c);
            return m;
        }

        private void EnsureLighting()
        {
            var existing = FindObjectOfType<Light>();
            if (existing != null) return;
            var lightGo = new GameObject("DirectionalLight");
            var l = lightGo.AddComponent<Light>();
            l.type = LightType.Directional;
            l.color = new Color(0.95f, 0.9f, 1f);
            l.intensity = 1.05f;
            lightGo.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
            RenderSettings.ambientLight = new Color(0.18f, 0.18f, 0.23f);
        }

        private void EnsureEventSystem()
        {
            if (FindObjectOfType<UnityEngine.EventSystems.EventSystem>() != null) return;
            var es = new GameObject("EventSystem");
            es.AddComponent<UnityEngine.EventSystems.EventSystem>();
            es.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
        }

        private GameObject SpawnPlayer(LevelData level)
        {
            var go = new GameObject("Player");
            go.layer = IgnoreRaycastLayer;
            var body = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            body.transform.SetParent(go.transform, false);
            body.transform.localPosition = new Vector3(0f, 1f, 0f);
            if (PlayerMaterial != null) body.GetComponent<Renderer>().sharedMaterial = PlayerMaterial;
            Destroy(body.GetComponent<Collider>());
            body.layer = IgnoreRaycastLayer;

            var nose = GameObject.CreatePrimitive(PrimitiveType.Cube);
            nose.transform.SetParent(go.transform, false);
            nose.transform.localPosition = new Vector3(0f, 1.4f, 0.45f);
            nose.transform.localScale = new Vector3(0.2f, 0.2f, 0.4f);
            if (PlayerMaterial != null) nose.GetComponent<Renderer>().sharedMaterial = PlayerMaterial;
            Destroy(nose.GetComponent<Collider>());
            nose.layer = IgnoreRaycastLayer;

            var cc = go.AddComponent<CharacterController>();
            cc.height = 1.8f;
            cc.center = new Vector3(0f, 1f, 0f);
            cc.radius = 0.35f;

            go.AddComponent<PlayerController>();
            go.AddComponent<PlayerVision>();
            go.transform.position = level.GridToWorld(level.PlayerSpawn, 0f);
            return go;
        }

        private GameObject SpawnOgre(LevelData level)
        {
            var go = new GameObject("Ogre");
            go.layer = IgnoreRaycastLayer;
            var body = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            body.transform.SetParent(go.transform, false);
            body.transform.localPosition = new Vector3(0f, 1.2f, 0f);
            body.transform.localScale = new Vector3(1.2f, 1.2f, 1.2f);
            if (OgreMaterial != null) body.GetComponent<Renderer>().sharedMaterial = OgreMaterial;
            Destroy(body.GetComponent<Collider>());
            body.layer = IgnoreRaycastLayer;

            var eye = GameObject.CreatePrimitive(PrimitiveType.Cube);
            eye.transform.SetParent(go.transform, false);
            eye.transform.localPosition = new Vector3(0f, 1.9f, 0.45f);
            eye.transform.localScale = new Vector3(0.25f, 0.25f, 0.4f);
            if (OgreMaterial != null) eye.GetComponent<Renderer>().sharedMaterial = OgreMaterial;
            Destroy(eye.GetComponent<Collider>());
            eye.layer = IgnoreRaycastLayer;

            var cc = go.AddComponent<CharacterController>();
            cc.height = 2.2f;
            cc.center = new Vector3(0f, 1.1f, 0f);
            cc.radius = 0.5f;

            go.AddComponent<OgreSenses>();
            go.AddComponent<OgreAI>();
            go.AddComponent<OgreSpeech>();

            go.transform.position = level.GridToWorld(level.OgreSpawn, 0f);
            return go;
        }

        private void SpawnBenches(LevelData level, out System.Collections.Generic.List<Bench> benches)
        {
            benches = new System.Collections.Generic.List<Bench>();
            foreach (var b in level.Benches)
            {
                var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
                go.name = $"Bench_{b.x}_{b.y}";
                go.transform.position = level.GridToWorld(b, 0.4f);
                go.transform.localScale = new Vector3(LevelData.TileSize * 0.85f, 0.8f, LevelData.TileSize * 0.45f);
                if (BenchMaterial != null) go.GetComponent<Renderer>().sharedMaterial = BenchMaterial;
                Destroy(go.GetComponent<Collider>());
                var bench = go.AddComponent<Bench>();
                bench.GridPos = b;
                benches.Add(bench);
            }
        }

        private ExitDoor SpawnDoor(LevelData level, bool requireKey)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = "ExitDoor";
            go.transform.position = level.GridToWorld(level.Exit, 1.5f);
            go.transform.localScale = new Vector3(LevelData.TileSize * 0.6f, 3f, LevelData.TileSize * 0.6f);
            if (DoorMaterial != null) go.GetComponent<Renderer>().sharedMaterial = DoorMaterial;
            Destroy(go.GetComponent<Collider>());
            var door = go.AddComponent<ExitDoor>();
            door.RequiresKey = requireKey;
            return door;
        }

        private Camera SetupCamera(Transform player)
        {
            var camGo = new GameObject("MainCamera");
            camGo.tag = "MainCamera";
            camGo.transform.position = player.position + new Vector3(0f, 15f, -13f);
            camGo.transform.LookAt(player.position + new Vector3(0f, 0.6f, 4f), Vector3.up);
            var cam = camGo.AddComponent<Camera>();
            cam.orthographic = false;
            cam.fieldOfView = 64f;
            cam.nearClipPlane = 0.05f;
            cam.farClipPlane = 80f;
            cam.backgroundColor = new Color(0.055f, 0.058f, 0.075f);
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.depth = 0f;
            camGo.AddComponent<AudioListener>();
            var follow = camGo.AddComponent<FollowCamera>();
            follow.Target = player;
            follow.Offset = new Vector3(0f, 15f, -13f);
            follow.LookOffset = new Vector3(0f, 0.6f, 4f);
            return cam;
        }

        private GameObject BuildUI(out HUD hud, out MainMenuScreen mainMenu, out PauseScreen pause, out DeathScreen death, out LevelCompleteScreen done, out MapPingOverlay overlay, out MobileTouchInput touch)
        {
            var canvasGo = new GameObject("UICanvas");
            var canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;
            var scaler = canvasGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 1920);
            scaler.matchWidthOrHeight = 0.5f;
            canvasGo.AddComponent<GraphicRaycaster>();

            var lookArea = MakeUIChild(canvasGo.transform, "LookArea");
            var lookImg = lookArea.AddComponent<Image>();
            lookImg.color = new Color(0, 0, 0, 0.001f);
            var lookRT = lookArea.GetComponent<RectTransform>();
            lookRT.anchorMin = new Vector2(0.45f, 0f);
            lookRT.anchorMax = new Vector2(1f, 1f);
            lookRT.offsetMin = Vector2.zero;
            lookRT.offsetMax = Vector2.zero;
            touch = lookArea.AddComponent<MobileTouchInput>();

            var stickGo = MakeUIChild(canvasGo.transform, "MoveStick");
            var stickRT = stickGo.GetComponent<RectTransform>();
            stickRT.anchorMin = new Vector2(0f, 0f);
            stickRT.anchorMax = new Vector2(0f, 0f);
            stickRT.pivot = new Vector2(0.5f, 0.5f);
            stickRT.anchoredPosition = new Vector2(220, 220);
            stickRT.sizeDelta = new Vector2(280, 280);
            var stickBg = stickGo.AddComponent<Image>();
            stickBg.color = new Color(1, 1, 1, 0.12f);
            var joy = stickGo.AddComponent<Joystick>();
            joy.Background = stickRT;

            var knobGo = MakeUIChild(stickGo.transform, "Knob");
            var knobRT = knobGo.GetComponent<RectTransform>();
            knobRT.sizeDelta = new Vector2(120, 120);
            knobRT.anchoredPosition = Vector2.zero;
            var knobImg = knobGo.AddComponent<Image>();
            knobImg.color = new Color(1, 1, 1, 0.35f);
            joy.Knob = knobRT;
            touch.MoveStick = joy;

            var hudGo = MakeUIChild(canvasGo.transform, "HUDPanel");
            var hudRT = hudGo.GetComponent<RectTransform>();
            hudRT.anchorMin = new Vector2(0f, 1f);
            hudRT.anchorMax = new Vector2(1f, 1f);
            hudRT.pivot = new Vector2(0.5f, 1f);
            hudRT.anchoredPosition = Vector2.zero;
            hudRT.sizeDelta = new Vector2(0, 160);
            hud = hudGo.AddComponent<HUD>();

            hud.LevelText = MakeText(hudGo.transform, "LevelText", "LEVEL 1", new Vector2(0f, 1f), new Vector2(40, -90), TextAnchor.UpperLeft, 56);
            hud.PingsText = MakeText(hudGo.transform, "PingsText", "MAP x2", new Vector2(1f, 1f), new Vector2(-40, -90), TextAnchor.UpperRight, 56);
            hud.MessageText = MakeText(canvasGo.transform, "MessageText", "", new Vector2(0.5f, 0.85f), Vector2.zero, TextAnchor.MiddleCenter, 64);
            hud.OgreLineText = MakeText(canvasGo.transform, "OgreLine", "", new Vector2(0.5f, 0.7f), Vector2.zero, TextAnchor.MiddleCenter, 80);
            hud.OgreLineText.color = new Color(0.95f, 0.25f, 0.25f);

            hud.MapButton = MakeButton(canvasGo.transform, "MapButton", "MAP", new Vector2(1f, 0f), new Vector2(-180, 380), new Vector2(220, 120));
            hud.HideButton = MakeButton(canvasGo.transform, "HideButton", "HIDE", new Vector2(1f, 0f), new Vector2(-180, 220), new Vector2(220, 120));
            hud.PauseButton = MakeButton(canvasGo.transform, "PauseButton", "PAUSE", new Vector2(1f, 1f), new Vector2(-180, -220), new Vector2(220, 110));

            var menuGo = MakeUIChild(canvasGo.transform, "MainMenu");
            var menuRT = menuGo.GetComponent<RectTransform>();
            menuRT.anchorMin = Vector2.zero;
            menuRT.anchorMax = Vector2.one;
            menuRT.offsetMin = Vector2.zero;
            menuRT.offsetMax = Vector2.zero;
            var menuDim = menuGo.AddComponent<Image>();
            menuDim.color = new Color(0, 0, 0, 0.82f);
            mainMenu = menuGo.AddComponent<MainMenuScreen>();
            var title = MakeText(menuGo.transform, "Title", "OgreRunn", new Vector2(0.5f, 0.72f), Vector2.zero, TextAnchor.MiddleCenter, 96);
            title.color = new Color(1f, 0.92f, 0.7f);
            mainMenu.PlaceholderText = MakeText(menuGo.transform, "Placeholder", "", new Vector2(0.5f, 0.32f), Vector2.zero, TextAnchor.MiddleCenter, 42);
            mainMenu.PlayButton = MakeButton(menuGo.transform, "PlayBtn", "PLAY", new Vector2(0.5f, 0.55f), Vector2.zero, new Vector2(420, 140));
            mainMenu.LevelSelectButton = MakeButton(menuGo.transform, "LevelSelectBtn", "LEVEL SELECT", new Vector2(0.5f, 0.43f), Vector2.zero, new Vector2(420, 120));
            mainMenu.SettingsButton = MakeButton(menuGo.transform, "SettingsBtn", "SETTINGS", new Vector2(0.5f, 0.22f), Vector2.zero, new Vector2(420, 120));

            var pauseGo = MakeUIChild(canvasGo.transform, "PauseScreen");
            var pauseRT = pauseGo.GetComponent<RectTransform>();
            pauseRT.anchorMin = Vector2.zero;
            pauseRT.anchorMax = Vector2.one;
            pauseRT.offsetMin = Vector2.zero;
            pauseRT.offsetMax = Vector2.zero;
            var pauseDim = pauseGo.AddComponent<Image>();
            pauseDim.color = new Color(0, 0, 0, 0.72f);
            pause = pauseGo.AddComponent<PauseScreen>();
            MakeText(pauseGo.transform, "PauseTitle", "PAUSED", new Vector2(0.5f, 0.66f), Vector2.zero, TextAnchor.MiddleCenter, 88);
            pause.ResumeButton = MakeButton(pauseGo.transform, "ResumeBtn", "RESUME", new Vector2(0.5f, 0.48f), Vector2.zero, new Vector2(420, 130));
            pause.RestartButton = MakeButton(pauseGo.transform, "PauseRestartBtn", "RESTART", new Vector2(0.5f, 0.34f), Vector2.zero, new Vector2(420, 130));
            pause.MainMenuButton = MakeButton(pauseGo.transform, "PauseMenuBtn", "MAIN MENU", new Vector2(0.5f, 0.2f), Vector2.zero, new Vector2(420, 130));
            pauseGo.SetActive(false);

            var overlayGo = MakeUIChild(canvasGo.transform, "MapPingOverlay");
            var overlayRT = overlayGo.GetComponent<RectTransform>();
            overlayRT.anchorMin = Vector2.zero;
            overlayRT.anchorMax = Vector2.one;
            overlayRT.offsetMin = Vector2.zero;
            overlayRT.offsetMax = Vector2.zero;
            var dim = overlayGo.AddComponent<Image>();
            dim.color = new Color(0, 0, 0, 0.85f);
            var raw = MakeUIChild(overlayGo.transform, "MapImage");
            var rawRT = raw.GetComponent<RectTransform>();
            rawRT.anchorMin = new Vector2(0.1f, 0.15f);
            rawRT.anchorMax = new Vector2(0.9f, 0.85f);
            rawRT.offsetMin = Vector2.zero;
            rawRT.offsetMax = Vector2.zero;
            var rawImg = raw.AddComponent<RawImage>();
            overlay = overlayGo.AddComponent<MapPingOverlay>();
            overlay.Canvas = rawImg;
            overlay.Frame = rawRT;
            overlay.CountdownText = MakeText(overlayGo.transform, "Countdown", "5", new Vector2(0.5f, 0.92f), Vector2.zero, TextAnchor.MiddleCenter, 72);
            overlayGo.SetActive(false);

            var deathGo = MakeUIChild(canvasGo.transform, "DeathScreen");
            var deathRT = deathGo.GetComponent<RectTransform>();
            deathRT.anchorMin = Vector2.zero;
            deathRT.anchorMax = Vector2.one;
            deathRT.offsetMin = Vector2.zero;
            deathRT.offsetMax = Vector2.zero;
            var deathDim = deathGo.AddComponent<Image>();
            deathDim.color = new Color(0, 0, 0, 0.8f);
            death = deathGo.AddComponent<DeathScreen>();
            death.TitleText = MakeText(deathGo.transform, "DeathTitle", "THE OGRE GOT YOU", new Vector2(0.5f, 0.65f), Vector2.zero, TextAnchor.MiddleCenter, 80);
            death.TitleText.color = new Color(0.95f, 0.2f, 0.2f);
            death.BodyText = MakeText(deathGo.transform, "DeathBody", "Ogre laugh placeholder", new Vector2(0.5f, 0.52f), Vector2.zero, TextAnchor.MiddleCenter, 48);
            death.RestartButton = MakeButton(deathGo.transform, "RestartBtn", "RETRY", new Vector2(0.5f, 0.4f), Vector2.zero, new Vector2(420, 140));
            death.MainMenuButton = MakeButton(deathGo.transform, "MainMenuBtn", "MAIN MENU", new Vector2(0.5f, 0.25f), Vector2.zero, new Vector2(420, 140));
            deathGo.SetActive(false);

            var doneGo = MakeUIChild(canvasGo.transform, "LevelCompleteScreen");
            var doneRT = doneGo.GetComponent<RectTransform>();
            doneRT.anchorMin = Vector2.zero;
            doneRT.anchorMax = Vector2.one;
            doneRT.offsetMin = Vector2.zero;
            doneRT.offsetMax = Vector2.zero;
            var doneDim = doneGo.AddComponent<Image>();
            doneDim.color = new Color(0, 0, 0, 0.8f);
            done = doneGo.AddComponent<LevelCompleteScreen>();
            done.TitleText = MakeText(doneGo.transform, "DoneTitle", "LEVEL COMPLETE", new Vector2(0.5f, 0.6f), Vector2.zero, TextAnchor.MiddleCenter, 80);
            done.TitleText.color = new Color(0.4f, 0.95f, 0.5f);
            done.NextButton = MakeButton(doneGo.transform, "NextBtn", "NEXT LEVEL", new Vector2(0.5f, 0.35f), Vector2.zero, new Vector2(420, 140));
            done.ReplayButton = MakeButton(doneGo.transform, "ReplayBtn", "REPLAY", new Vector2(0.5f, 0.22f), Vector2.zero, new Vector2(420, 120));
            done.MainMenuButton = MakeButton(doneGo.transform, "DoneMenuBtn", "MAIN MENU", new Vector2(0.5f, 0.1f), Vector2.zero, new Vector2(420, 120));
            doneGo.SetActive(false);

            return canvasGo;
        }

        private static GameObject MakeUIChild(Transform parent, string name)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            return go;
        }

        private static Text MakeText(Transform parent, string name, string content, Vector2 anchor, Vector2 anchoredPos, TextAnchor align, int fontSize)
        {
            var go = MakeUIChild(parent, name);
            go.layer = 5;
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = rt.anchorMax = rt.pivot = anchor;
            rt.anchoredPosition = anchoredPos;
            rt.sizeDelta = new Vector2(900, 200);
            var t = go.AddComponent<Text>();
            t.text = content;
            t.alignment = align;
            t.fontSize = fontSize;
            t.color = Color.white;
            t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            return t;
        }

        private static Button MakeButton(Transform parent, string name, string label, Vector2 anchor, Vector2 anchoredPos, Vector2 size)
        {
            var go = MakeUIChild(parent, name);
            go.layer = 5;
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = rt.anchorMax = rt.pivot = anchor;
            rt.anchoredPosition = anchoredPos;
            rt.sizeDelta = size;
            var img = go.AddComponent<Image>();
            img.color = new Color(1, 1, 1, 0.18f);
            var btn = go.AddComponent<Button>();
            btn.targetGraphic = img;

            var labelGo = MakeUIChild(go.transform, "Label");
            var lrt = labelGo.GetComponent<RectTransform>();
            lrt.anchorMin = Vector2.zero;
            lrt.anchorMax = Vector2.one;
            lrt.offsetMin = Vector2.zero;
            lrt.offsetMax = Vector2.zero;
            var t = labelGo.AddComponent<Text>();
            t.text = label;
            t.alignment = TextAnchor.MiddleCenter;
            t.fontSize = 48;
            t.color = Color.white;
            t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            return btn;
        }
    }
}
