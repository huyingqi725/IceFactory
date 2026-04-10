using System;
using IceFactory.Thermal.Core;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;

namespace IceFactory.Gameplay.Enemy
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(NavMeshAgent))]
    public sealed class EnemyLureController : MonoBehaviour, IThermalInteractable, IShatterableWhenFrozen
    {
        [Header("Movement")]
        [SerializeField] private EnemyPatrolRoute patrolRoute;
        [SerializeField] [Min(0.05f)] private float patrolArriveDistance = 0.5f;

        [Header("Player Detection")]
        [SerializeField] private Transform playerTarget;
        [SerializeField] [Min(0.1f)] private float detectRadius = 7f;
        [SerializeField] [Min(0.2f)] private float loseRadius = 10f;
        [SerializeField] [Min(0.02f)] private float chaseRefreshInterval = 0.15f;

        [Header("Frozen")]
        [SerializeField] private string frozenWeakPointTag = "VulnerableFrozen";
        [SerializeField] private UnityEvent onFrozen = new UnityEvent();
        [SerializeField] private UnityEvent onShattered = new UnityEvent();

        public event Action<EnemyMoveState> EnemyMoveStateChanged;
        public bool IsFrozen => _state == EnemyMoveState.Frozen;

        private NavMeshAgent _agent;
        private EnemyMoveState _state = EnemyMoveState.Patrolling;
        private Transform _currentPatrolTarget;
        private float _nextChaseRefreshTime;
        private string _originalTag;

        private void Awake()
        {
            _agent = GetComponent<NavMeshAgent>();
            _originalTag = gameObject.tag;

            if (loseRadius < detectRadius)
            {
                loseRadius = detectRadius + 0.5f;
            }
        }

        private void Start()
        {
            if (playerTarget == null)
            {
                var player = GameObject.FindGameObjectWithTag("Player");
                if (player != null)
                {
                    playerTarget = player.transform;
                }
            }

            SelectInitialPatrolTarget();
            EnterState(EnemyMoveState.Patrolling);
        }

        private void Update()
        {
            if (_state == EnemyMoveState.Frozen)
            {
                return;
            }

            UpdateStateTransitionByDistance();

            if (_state == EnemyMoveState.Patrolling)
            {
                TickPatrolling();
                return;
            }

            TickChasing();
        }

        public bool CanReceiveTemperature(TemperatureType type)
        {
            return !IsFrozen && type == TemperatureType.Cold;
        }

        public void OnReceiveTemperature(TemperaturePayload payload)
        {
            if (payload.Type != TemperatureType.Cold || IsFrozen)
            {
                return;
            }

            Freeze();
        }

        public void TryShatter(ShatterSourceType sourceType)
        {
            if (!IsFrozen)
            {
                return;
            }

            onShattered.Invoke();
            Destroy(gameObject);
        }

        private void UpdateStateTransitionByDistance()
        {
            if (playerTarget == null)
            {
                if (_state == EnemyMoveState.Chasing)
                {
                    EnterState(EnemyMoveState.Patrolling);
                }

                return;
            }

            var distance = Vector3.Distance(transform.position, playerTarget.position);
            if (_state == EnemyMoveState.Patrolling && distance <= detectRadius)
            {
                EnterState(EnemyMoveState.Chasing);
            }
            else if (_state == EnemyMoveState.Chasing && distance >= loseRadius)
            {
                EnterState(EnemyMoveState.Patrolling);
            }
        }

        private void TickPatrolling()
        {
            if (patrolRoute == null || !patrolRoute.IsValid || _currentPatrolTarget == null)
            {
                _agent.isStopped = true;
                return;
            }

            if (_agent.enabled && _agent.isOnNavMesh)
            {
                _agent.isStopped = false;
                _agent.SetDestination(_currentPatrolTarget.position);
            }

            var sqrDistance = (transform.position - _currentPatrolTarget.position).sqrMagnitude;
            if (sqrDistance <= patrolArriveDistance * patrolArriveDistance)
            {
                _currentPatrolTarget = _currentPatrolTarget == patrolRoute.PointA ? patrolRoute.PointB : patrolRoute.PointA;
            }
        }

        private void TickChasing()
        {
            if (playerTarget == null)
            {
                EnterState(EnemyMoveState.Patrolling);
                return;
            }

            if (Time.time < _nextChaseRefreshTime)
            {
                return;
            }

            _nextChaseRefreshTime = Time.time + chaseRefreshInterval;
            if (_agent.enabled && _agent.isOnNavMesh)
            {
                _agent.isStopped = false;
                _agent.SetDestination(playerTarget.position);
            }
        }

        private void Freeze()
        {
            EnterState(EnemyMoveState.Frozen);

            if (_agent.enabled)
            {
                _agent.isStopped = true;
                _agent.ResetPath();
                _agent.enabled = false;
            }

            if (!string.IsNullOrWhiteSpace(frozenWeakPointTag))
            {
                gameObject.tag = frozenWeakPointTag;
            }

            onFrozen.Invoke();
        }

        private void EnterState(EnemyMoveState nextState)
        {
            if (_state == nextState)
            {
                return;
            }

            _state = nextState;
            EnemyMoveStateChanged?.Invoke(_state);

            if (_state == EnemyMoveState.Patrolling)
            {
                if (string.Equals(gameObject.tag, frozenWeakPointTag, StringComparison.Ordinal))
                {
                    gameObject.tag = _originalTag;
                }

                SelectInitialPatrolTarget();
            }
        }

        private void SelectInitialPatrolTarget()
        {
            if (patrolRoute == null || !patrolRoute.IsValid)
            {
                _currentPatrolTarget = null;
                return;
            }

            var toA = (transform.position - patrolRoute.PointA.position).sqrMagnitude;
            var toB = (transform.position - patrolRoute.PointB.position).sqrMagnitude;
            _currentPatrolTarget = toA <= toB ? patrolRoute.PointA : patrolRoute.PointB;
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, detectRadius);
            Gizmos.color = new Color(1f, 0.5f, 0f, 1f);
            Gizmos.DrawWireSphere(transform.position, loseRadius);
        }
    }
}
