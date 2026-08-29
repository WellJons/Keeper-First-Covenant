using System;
using KeeperFirstCovenant.Combat;
using UnityEngine;

namespace KeeperFirstCovenant.World
{
    public sealed class WorldTimeSystem : MonoBehaviour
    {
        public static WorldTimeSystem Instance
        {
            get;
            private set;
        }

        [SerializeField, Range(0f, 24f)]
        private float hour = 9f;

        [SerializeField, Min(1f)]
        private float realSecondsPerGameHour = 90f;

        [SerializeField]
        private bool advanceDuringCombat;

        private int _day = 1;

        public float Hour => hour;
        public int Day => _day;

        public bool IsNight =>
            hour < 6f ||
            hour >= 20f;

        public float VisibilityMultiplier
        {
            get
            {
                if (hour >= 8f &&
                    hour < 18f)
                {
                    return 1f;
                }

                if (hour >= 6f &&
                    hour < 8f)
                {
                    return Mathf.Lerp(
                        0.62f,
                        1f,
                        (hour - 6f) / 2f);
                }

                if (hour >= 18f &&
                    hour < 20f)
                {
                    return Mathf.Lerp(
                        1f,
                        0.62f,
                        (hour - 18f) / 2f);
                }

                return 0.62f;
            }
        }

        public event Action<float> TimeChanged;
        public event Action<int> DayChanged;

        private void Awake()
        {
            if (Instance != null &&
                Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }

        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }

        private void Update()
        {
            TurnCombatDirector director =
                TurnCombatDirector.Instance;

            if (!advanceDuringCombat &&
                director != null &&
                director.State ==
                    CombatState.Active)
            {
                return;
            }

            if (Time.timeScale <= 0f)
                return;

            float hours =
                Time.deltaTime /
                Mathf.Max(
                    1f,
                    realSecondsPerGameHour);

            AdvanceHours(hours);
        }

        public void AdvanceHours(float hours)
        {
            if (Mathf.Approximately(
                    hours,
                    0f))
            {
                return;
            }

            float value =
                hour + hours;

            while (value >= 24f)
            {
                value -= 24f;
                _day++;
                DayChanged?.Invoke(_day);
            }

            while (value < 0f)
            {
                value += 24f;
                _day =
                    Mathf.Max(
                        1,
                        _day - 1);
                DayChanged?.Invoke(_day);
            }

            hour = value;
            TimeChanged?.Invoke(hour);
        }

        public void SetTime(
            float newHour,
            int day = -1)
        {
            hour =
                Mathf.Repeat(
                    newHour,
                    24f);

            if (day > 0)
                _day = day;

            TimeChanged?.Invoke(hour);
            DayChanged?.Invoke(_day);
        }
    }
}
