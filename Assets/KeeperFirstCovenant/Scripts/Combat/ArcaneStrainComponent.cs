using System;
using UnityEngine;

namespace KeeperFirstCovenant.Combat
{
    [RequireComponent(typeof(CombatantRuntime))]
    public sealed class ArcaneStrainComponent : MonoBehaviour
    {
        [SerializeField, Min(1)]
        private int maxStrain = 100;

        [SerializeField, Min(0)]
        private int recoveryPerTurn = 8;

        [SerializeField, Range(0, 100)]
        private int strainedThreshold = 50;

        [SerializeField, Range(0, 100)]
        private int severeThreshold = 75;

        [SerializeField, Range(0, 100)]
        private int criticalThreshold = 90;

        private int _current;

        public int Current => _current;
        public int Max => maxStrain;

        public float Normalized =>
            maxStrain > 0
                ? _current / (float)maxStrain
                : 0f;

        public bool IsStrained =>
            _current >= strainedThreshold;

        public bool IsSevere =>
            _current >= severeThreshold;

        public bool IsCritical =>
            _current >= criticalThreshold;

        public event Action<ArcaneStrainComponent>
            Changed;

        public bool CanAccept(int amount)
        {
            if (amount <= 0)
                return true;

            return _current + amount <= maxStrain;
        }

        public bool TryAdd(int amount)
        {
            if (amount <= 0)
                return true;

            if (!CanAccept(amount))
                return false;

            _current =
                Mathf.Clamp(
                    _current + amount,
                    0,
                    maxStrain);

            Changed?.Invoke(this);
            return true;
        }

        public void Recover(int amount)
        {
            if (amount <= 0 ||
                _current <= 0)
            {
                return;
            }

            _current =
                Mathf.Max(
                    0,
                    _current - amount);

            Changed?.Invoke(this);
        }

        public void Clear()
        {
            if (_current == 0)
                return;

            _current = 0;
            Changed?.Invoke(this);
        }

        public void OnOwnerTurnStarted()
        {
            Recover(recoveryPerTurn);
        }

        public float GetMovementMultiplier()
        {
            if (IsCritical)
                return 0.55f;

            if (IsSevere)
                return 0.70f;

            if (IsStrained)
                return 0.90f;

            return 1f;
        }

        public int GetActionPointPenalty()
        {
            return IsSevere ? 1 : 0;
        }

        public int GetHitChancePenalty()
        {
            if (IsCritical)
                return 15;

            if (IsSevere)
                return 7;

            return 0;
        }

        public bool BlocksReactions()
        {
            return IsSevere;
        }
    }
}
