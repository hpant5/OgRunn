using System.Collections.Generic;
using UnityEngine;
using Runn.Common;
using Runn.Level;
using Runn.Player;
using Runn.Systems;

namespace Runn.Ogre
{
    public enum OgreState { Patrol, Chase, Search, Lost }

    [RequireComponent(typeof(CharacterController))]
    public class OgreAI : MonoBehaviour
    {
        public Transform PlayerTransform;
        public OgreSenses Senses;
        public OgreSpeech Speech;

        public float BaseSpeed = GameConstants.OgreSpeed;
        public float TurnSpeed = 240f;
        public float ReachTileDistance = 0.35f;
        public float ChaseLossSeconds = 3f;
        public float SearchDuration = 6f;
        public float CatchDistance = 1.0f;

        public OgreState State { get; private set; } = OgreState.Patrol;
        public bool Frozen;

        private CharacterController _cc;
        private LevelData _level;
        private System.Random _rng;
        private List<Vector2Int> _path = new List<Vector2Int>();
        private int _pathIdx;
        private Vector2Int _patrolGoal;
        private Vector3 _lastSeenPos;
        private float _lastSeenTime = -999f;
        private float _searchEndAt;
        private bool _hasLastSeen;

        private void Awake()
        {
            _cc = GetComponent<CharacterController>();
            _rng = new System.Random(System.DateTime.Now.Millisecond);
        }

        public void Init(LevelData level, Transform player, OgreSenses senses, OgreSpeech speech)
        {
            _level = level;
            PlayerTransform = player;
            Senses = senses;
            Speech = speech;
            PickNewPatrolGoal();
        }

        private void Update()
        {
            if (_level == null || PlayerTransform == null) return;
            if (Frozen || (GameManager.Instance != null && GameManager.Instance.State != GameState.Playing)) return;

            bool playerHidden = GameManager.Instance != null && GameManager.Instance.IsPlayerHidden;
            bool sees = Senses != null && Senses.CanSee(PlayerTransform, playerHidden);

            switch (State)
            {
                case OgreState.Patrol:
                    if (sees) EnterChase();
                    break;
                case OgreState.Chase:
                    if (sees)
                    {
                        _hasLastSeen = true;
                        _lastSeenPos = PlayerTransform.position;
                        _lastSeenTime = Time.time;
                        RepathTo(_level.WorldToGrid(_lastSeenPos));
                    }
                    else if (Time.time - _lastSeenTime > ChaseLossSeconds)
                    {
                        EnterSearch();
                    }
                    break;
                case OgreState.Search:
                    if (sees) EnterChase();
                    else if (Time.time > _searchEndAt) EnterLost();
                    break;
                case OgreState.Lost:
                    if (sees) EnterChase();
                    else EnterPatrol();
                    break;
            }

            FollowPath();

            if (Vector3.Distance(transform.position, PlayerTransform.position) < CatchDistance && !playerHidden)
            {
                GameManager.Instance?.OnPlayerCaught(false);
            }
        }

        public void OnSawHide(Vector3 benchWorldPos)
        {
            if (_level == null) return;
            Speech?.Say("I SAW YOU HIDING");
            var goal = _level.WorldToGrid(benchWorldPos);
            RepathTo(goal);
            State = OgreState.Chase;
            _hasLastSeen = true;
            _lastSeenPos = benchWorldPos;
            _lastSeenTime = Time.time;
        }

        private void EnterPatrol()
        {
            State = OgreState.Patrol;
            PickNewPatrolGoal();
        }

        private void EnterChase()
        {
            State = OgreState.Chase;
            _hasLastSeen = true;
            _lastSeenPos = PlayerTransform.position;
            _lastSeenTime = Time.time;
            Speech?.Say("THERE YOU ARE");
            RepathTo(_level.WorldToGrid(_lastSeenPos));
        }

        private void EnterSearch()
        {
            State = OgreState.Search;
            _searchEndAt = Time.time + SearchDuration;
            Speech?.Say("WHERE DID YOU GO");
            if (_hasLastSeen) RepathTo(_level.WorldToGrid(_lastSeenPos));
        }

        private void EnterLost()
        {
            State = OgreState.Lost;
            Speech?.Say("LOST");
        }

        private void PickNewPatrolGoal()
        {
            _patrolGoal = GridPathfinder.RandomWalkable(_level.Walkable, _rng);
            RepathTo(_patrolGoal);
        }

        private void RepathTo(Vector2Int goal)
        {
            var start = _level.WorldToGrid(transform.position);
            var path = GridPathfinder.FindPath(_level.Walkable, start, goal);
            _path = path ?? new List<Vector2Int>();
            _pathIdx = 0;
        }

        private void FollowPath()
        {
            if (_path == null || _path.Count == 0)
            {
                if (State == OgreState.Patrol) PickNewPatrolGoal();
                return;
            }

            if (_pathIdx >= _path.Count)
            {
                if (State == OgreState.Patrol) PickNewPatrolGoal();
                return;
            }

            Vector3 target = _level.GridToWorld(_path[_pathIdx], 0f);
            Vector3 to = target - transform.position;
            to.y = 0f;
            float dist = to.magnitude;

            if (dist < ReachTileDistance)
            {
                _pathIdx++;
                return;
            }

            Vector3 dir = to / Mathf.Max(dist, 0.0001f);
            Quaternion want = Quaternion.LookRotation(dir, Vector3.up);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, want, TurnSpeed * Time.deltaTime);

            float speed = BaseSpeed;
            if (State == OgreState.Patrol) speed *= 0.7f;

            Vector3 move = dir * speed * Time.deltaTime;
            move.y = -9.81f * Time.deltaTime;
            _cc.Move(move);
        }
    }
}
