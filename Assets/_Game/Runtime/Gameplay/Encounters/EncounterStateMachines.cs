using System;

namespace Gravivore.Gameplay.Encounters
{
    public enum MagnetarGuardState
    {
        Waiting = 0,
        Approach = 1,
        Telegraphing = 2,
        Recovery = 3,
        Dead = 4
    }

    public readonly struct MagnetarGuardDecision
    {
        public MagnetarGuardDecision(
            MagnetarGuardState state,
            bool shouldApproach,
            bool telegraphBegan,
            bool resolveShockwave)
        {
            State = state;
            ShouldApproach = shouldApproach;
            TelegraphBegan = telegraphBegan;
            ResolveShockwave = resolveShockwave;
        }

        public MagnetarGuardState State { get; }
        public bool ShouldApproach { get; }
        public bool TelegraphBegan { get; }
        public bool ResolveShockwave { get; }
    }

    public sealed class MagnetarGuardStateMachine
    {
        private MagnetarGuardConfiguration _configuration;
        private float _remaining;
        private bool _configured;

        public MagnetarGuardState State { get; private set; } = MagnetarGuardState.Waiting;

        public void Configure(in MagnetarGuardConfiguration configuration)
        {
            _configuration = configuration;
            _configured = true;
            Reset();
        }

        public MagnetarGuardDecision Tick(float deltaTime, float distanceToPlayer)
        {
            if (!_configured) throw new InvalidOperationException("Magnetar state machine is not configured.");
            ValidateNonNegative(deltaTime, nameof(deltaTime));
            ValidateNonNegative(distanceToPlayer, nameof(distanceToPlayer));
            switch (State)
            {
                case MagnetarGuardState.Waiting:
                    if (distanceToPlayer > _configuration.AggroRadius) return Decision();
                    State = MagnetarGuardState.Approach;
                    return EvaluateApproach(distanceToPlayer);
                case MagnetarGuardState.Approach:
                    if (distanceToPlayer > _configuration.AggroRadius)
                    {
                        State = MagnetarGuardState.Waiting;
                        return Decision();
                    }

                    return EvaluateApproach(distanceToPlayer);
                case MagnetarGuardState.Telegraphing:
                    _remaining = Math.Max(0f, _remaining - deltaTime);
                    if (_remaining > 0f) return Decision();
                    State = MagnetarGuardState.Recovery;
                    _remaining = _configuration.RecoveryDuration;
                    return new MagnetarGuardDecision(State, false, false, true);
                case MagnetarGuardState.Recovery:
                    _remaining = Math.Max(0f, _remaining - deltaTime);
                    if (_remaining <= 0f) State = MagnetarGuardState.Approach;
                    return Decision();
                case MagnetarGuardState.Dead:
                    return Decision();
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public void MarkDead()
        {
            State = MagnetarGuardState.Dead;
            _remaining = 0f;
        }

        public void Reset()
        {
            State = MagnetarGuardState.Waiting;
            _remaining = 0f;
        }

        private MagnetarGuardDecision EvaluateApproach(float distanceToPlayer)
        {
            if (distanceToPlayer > _configuration.AttackRange)
            {
                return new MagnetarGuardDecision(State, true, false, false);
            }

            State = MagnetarGuardState.Telegraphing;
            _remaining = _configuration.TelegraphDuration;
            return new MagnetarGuardDecision(State, false, true, false);
        }

        private MagnetarGuardDecision Decision() => new MagnetarGuardDecision(State, false, false, false);

        private static void ValidateNonNegative(float value, string name)
        {
            if (float.IsNaN(value) || float.IsInfinity(value) || value < 0f) throw new ArgumentOutOfRangeException(name);
        }
    }

    public enum CustodianBossState
    {
        Dormant = 0,
        Engaging = 1,
        Telegraphing = 2,
        ExecutingAttack = 3,
        Recovery = 4,
        Resetting = 5,
        Dead = 6
    }

    public readonly struct BossStateDecision
    {
        public BossStateDecision(
            CustodianBossState state,
            BossAttackType attack,
            bool telegraphBegan,
            bool resolveAttack,
            bool phaseChanged)
        {
            State = state;
            Attack = attack;
            TelegraphBegan = telegraphBegan;
            ResolveAttack = resolveAttack;
            PhaseChanged = phaseChanged;
        }

        public CustodianBossState State { get; }
        public BossAttackType Attack { get; }
        public bool TelegraphBegan { get; }
        public bool ResolveAttack { get; }
        public bool PhaseChanged { get; }
    }

    public sealed class CustodianBossStateMachine
    {
        private readonly CustodianBossConfiguration _configuration;
        private float _remaining;
        private int _attackIndex;

        public CustodianBossStateMachine(CustodianBossConfiguration configuration)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        public CustodianBossState State { get; private set; } = CustodianBossState.Dormant;
        public BossAttackType CurrentAttack { get; private set; }
        public bool IsLowHealthPhase { get; private set; }
        public float RemainingTime => _remaining;

        public bool Engage()
        {
            if (State != CustodianBossState.Dormant) return false;
            State = CustodianBossState.Engaging;
            return true;
        }

        public BossStateDecision Tick(float deltaTime, float healthFraction)
        {
            ValidateNonNegative(deltaTime, nameof(deltaTime));
            if (float.IsNaN(healthFraction) || float.IsInfinity(healthFraction) || healthFraction < 0f || healthFraction > 1f)
            {
                throw new ArgumentOutOfRangeException(nameof(healthFraction));
            }

            var phaseChanged = false;
            if (State != CustodianBossState.Dormant && State != CustodianBossState.Dead &&
                !IsLowHealthPhase && healthFraction <= _configuration.LowHealthThreshold)
            {
                IsLowHealthPhase = true;
                phaseChanged = true;
            }

            switch (State)
            {
                case CustodianBossState.Dormant:
                case CustodianBossState.Resetting:
                case CustodianBossState.Dead:
                    return Decision(phaseChanged);
                case CustodianBossState.Engaging:
                    CurrentAttack = _configuration.GetAttackAtSequenceIndex(_attackIndex++);
                    _remaining = _configuration.GetAttack(CurrentAttack).TelegraphDuration;
                    State = CustodianBossState.Telegraphing;
                    return new BossStateDecision(State, CurrentAttack, true, false, phaseChanged);
                case CustodianBossState.Telegraphing:
                    _remaining = Math.Max(0f, _remaining - deltaTime);
                    if (_remaining > 0f) return Decision(phaseChanged);
                    State = CustodianBossState.ExecutingAttack;
                    return new BossStateDecision(State, CurrentAttack, false, true, phaseChanged);
                case CustodianBossState.ExecutingAttack:
                    State = CustodianBossState.Recovery;
                    _remaining = _configuration.RecoveryDuration *
                                 (IsLowHealthPhase ? _configuration.LowHealthCadenceMultiplier : 1f);
                    return Decision(phaseChanged);
                case CustodianBossState.Recovery:
                    _remaining = Math.Max(0f, _remaining - deltaTime);
                    if (_remaining <= 0f) State = CustodianBossState.Engaging;
                    return Decision(phaseChanged);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public bool BeginReset()
        {
            if (State == CustodianBossState.Dormant || State == CustodianBossState.Resetting || State == CustodianBossState.Dead)
            {
                return false;
            }

            State = CustodianBossState.Resetting;
            _remaining = 0f;
            CurrentAttack = default;
            IsLowHealthPhase = false;
            _attackIndex = 0;
            return true;
        }

        public void CompleteReset()
        {
            if (State != CustodianBossState.Resetting) return;
            State = CustodianBossState.Dormant;
        }

        public bool MarkDead()
        {
            if (State == CustodianBossState.Dead) return false;
            State = CustodianBossState.Dead;
            _remaining = 0f;
            return true;
        }

        private BossStateDecision Decision(bool phaseChanged)
        {
            return new BossStateDecision(State, CurrentAttack, false, false, phaseChanged);
        }

        private static void ValidateNonNegative(float value, string name)
        {
            if (float.IsNaN(value) || float.IsInfinity(value) || value < 0f) throw new ArgumentOutOfRangeException(name);
        }
    }
}
