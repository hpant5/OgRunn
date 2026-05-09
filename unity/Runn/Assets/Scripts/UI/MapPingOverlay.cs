using UnityEngine;
using UnityEngine.UI;
using Runn.Level;
using Runn.Systems;

namespace Runn.UI
{
    public class MapPingOverlay : MonoBehaviour
    {
        public RawImage Canvas;
        public RectTransform Frame;
        public Text CountdownText;
        public GameManager Game;
        public MapPingSystem MapPing;

        public Color WallColor = new Color(0.15f, 0.15f, 0.18f, 1f);
        public Color FloorColor = new Color(0.4f, 0.4f, 0.45f, 1f);
        public Color BenchColor = new Color(0.95f, 0.85f, 0.3f, 1f);
        public Color ExitColor = new Color(0.3f, 0.95f, 0.4f, 1f);
        public Color PlayerColor = Color.white;
        public Color OgreColor = new Color(0.95f, 0.3f, 0.3f, 1f);

        private Texture2D _tex;

        private void Awake()
        {
            if (gameObject.activeSelf) gameObject.SetActive(false);
        }

        public void Show()
        {
            gameObject.SetActive(true);
            BuildTexture();
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }

        private void Update()
        {
            if (!gameObject.activeSelf) return;
            if (CountdownText != null && MapPing != null)
                CountdownText.text = Mathf.CeilToInt(MapPing.SecondsRemaining).ToString();
            BuildTexture();
        }

        private void BuildTexture()
        {
            if (Game == null || Game.ActiveLevel == null) return;
            var lvl = Game.ActiveLevel;

            int w = lvl.Width;
            int h = lvl.Height;
            if (_tex == null || _tex.width != w || _tex.height != h)
            {
                _tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
                _tex.filterMode = FilterMode.Point;
                if (Canvas != null) Canvas.texture = _tex;
            }

            for (int x = 0; x < w; x++)
                for (int y = 0; y < h; y++)
                    _tex.SetPixel(x, y, lvl.Walkable[x, y] ? FloorColor : WallColor);

            foreach (var b in Game.Benches)
                if (b != null) _tex.SetPixel(b.GridPos.x, b.GridPos.y, BenchColor);

            _tex.SetPixel(lvl.Exit.x, lvl.Exit.y, ExitColor);

            if (Game.Player != null)
            {
                var pg = lvl.WorldToGrid(Game.Player.transform.position);
                _tex.SetPixel(pg.x, pg.y, PlayerColor);
            }

            if (MapPing != null)
            {
                var og = lvl.WorldToGrid(MapPing.OgrePosition);
                _tex.SetPixel(og.x, og.y, OgreColor);
            }

            _tex.Apply();
        }
    }
}
