using System;
using System.Collections;
using UnityEngine;

namespace KeeperFirstCovenant.Visual
{
    public enum PaperDollMotionState
    {
        Idle, Walk, Run, Guard, AttackLight, AttackHeavy, Cast, Interact, Hurt, Dead
    }

    [DisallowMultipleComponent]
    public sealed class PaperDollMotionAnimator : MonoBehaviour
    {
        [Header("Rig")]
        [SerializeField] private Transform visualRoot;
        [SerializeField] private Transform torso;
        [SerializeField] private Transform head;
        [SerializeField] private SpriteRenderer eyesRenderer;
        [SerializeField] private Transform upperArmLeft;
        [SerializeField] private Transform upperArmRight;
        [SerializeField] private Transform forearmLeft;
        [SerializeField] private Transform forearmRight;
        [SerializeField] private Transform thighLeft;
        [SerializeField] private Transform thighRight;
        [SerializeField] private Transform shinLeft;
        [SerializeField] private Transform shinRight;
        [SerializeField] private Transform cloakLeft;
        [SerializeField] private Transform cloakCenter;
        [SerializeField] private Transform cloakRight;
        [SerializeField] private Transform weaponSocket;

        [Header("Idle")]
        [SerializeField] private float breathingFrequency = 1.45f;
        [SerializeField] private float breathingAmount = 0.018f;
        [SerializeField] private float headIdleDegrees = 0.8f;
        [SerializeField] private Vector2 blinkInterval = new Vector2(2.2f, 5.4f);
        [SerializeField] private float blinkDuration = 0.095f;

        [Header("Locomotion")]
        [SerializeField] private float walkFrequency = 7.2f;
        [SerializeField] private float runFrequency = 10.2f;
        [SerializeField] private float walkLegSwing = 16f;
        [SerializeField] private float runLegSwing = 25f;
        [SerializeField] private float walkArmSwing = 11f;
        [SerializeField] private float runArmSwing = 18f;
        [SerializeField] private float walkBob = 0.035f;
        [SerializeField] private float runBob = 0.055f;

        [Header("Secondary motion")]
        [SerializeField] private float cloakIdleSway = 1.8f;
        [SerializeField] private float cloakWalkSway = 6f;
        [SerializeField] private float cloakRunSway = 11f;

        private PaperDollMotionState _state = PaperDollMotionState.Idle;
        private float _speed01;
        private Coroutine _actionRoutine;
        private float _nextBlinkAt;
        private float _blinkUntil;
        private bool _dead;
        private PoseSnapshot _basePose;

        public PaperDollMotionState State => _state;
        public bool IsBusy => _actionRoutine != null;
        public bool IsDead => _dead;

        public event Action AttackImpact;
        public event Action ActionFinished;

        private struct PoseSnapshot
        {
            public Vector3 rootPos, torsoPos, headPos;
            public Quaternion rootRot, torsoRot, headRot;
            public Quaternion upperArmLeftRot, upperArmRightRot;
            public Quaternion forearmLeftRot, forearmRightRot;
            public Quaternion thighLeftRot, thighRightRot;
            public Quaternion shinLeftRot, shinRightRot;
            public Quaternion cloakLeftRot, cloakCenterRot, cloakRightRot;
            public Quaternion weaponRot;
        }

        private void Awake()
        {
            if (visualRoot == null)
                visualRoot = transform;
            CaptureBasePose();
            ScheduleNextBlink();
        }

        public void Configure(
            Transform configuredVisualRoot,
            Transform configuredTorso,
            Transform configuredHead,
            SpriteRenderer configuredEyes,
            Transform configuredUpperArmLeft,
            Transform configuredUpperArmRight,
            Transform configuredForearmLeft,
            Transform configuredForearmRight,
            Transform configuredThighLeft,
            Transform configuredThighRight,
            Transform configuredShinLeft,
            Transform configuredShinRight,
            Transform configuredCloakLeft,
            Transform configuredCloakCenter,
            Transform configuredCloakRight,
            Transform configuredWeaponSocket)
        {
            visualRoot = configuredVisualRoot != null ? configuredVisualRoot : transform;
            torso = configuredTorso;
            head = configuredHead;
            eyesRenderer = configuredEyes;
            upperArmLeft = configuredUpperArmLeft;
            upperArmRight = configuredUpperArmRight;
            forearmLeft = configuredForearmLeft;
            forearmRight = configuredForearmRight;
            thighLeft = configuredThighLeft;
            thighRight = configuredThighRight;
            shinLeft = configuredShinLeft;
            shinRight = configuredShinRight;
            cloakLeft = configuredCloakLeft;
            cloakCenter = configuredCloakCenter;
            cloakRight = configuredCloakRight;
            weaponSocket = configuredWeaponSocket;
            CaptureBasePose();
        }

        public void SetLocomotion(bool moving, bool running, float normalizedSpeed)
        {
            if (_dead || _actionRoutine != null || _state == PaperDollMotionState.Guard)
                return;

            _speed01 = Mathf.Clamp01(normalizedSpeed);
            _state = !moving ? PaperDollMotionState.Idle
                : running ? PaperDollMotionState.Run
                : PaperDollMotionState.Walk;
        }

        public void SetGuarding(bool guarding)
        {
            if (_dead || _actionRoutine != null)
                return;

            _state = guarding ? PaperDollMotionState.Guard : PaperDollMotionState.Idle;
            ResetPose();
        }

        private void LateUpdate()
        {
            UpdateBlink();

            if (_dead || _actionRoutine != null)
                return;

            switch (_state)
            {
                case PaperDollMotionState.Walk: ApplyLocomotion(false); break;
                case PaperDollMotionState.Run: ApplyLocomotion(true); break;
                case PaperDollMotionState.Guard: ApplyGuardPose(); break;
                default: ApplyIdle(); break;
            }
        }

        private void ApplyIdle()
        {
            ResetRotationsOnly();

            float phase = Time.time * breathingFrequency;
            float breath = (Mathf.Sin(phase) * 0.5f + 0.5f) * breathingAmount;

            SetPosition(visualRoot, _basePose.rootPos);
            SetPosition(torso, _basePose.torsoPos + Vector3.up * breath);
            SetPosition(head, _basePose.headPos + Vector3.up * breath * 0.42f);
            SetZ(head, Mathf.Sin(Time.time * 0.42f) * headIdleDegrees, _basePose.headRot);

            SetZ(cloakLeft, Mathf.Sin(Time.time * 0.87f + 0.4f) * cloakIdleSway, _basePose.cloakLeftRot);
            SetZ(cloakCenter, Mathf.Sin(Time.time * 0.76f) * cloakIdleSway * 0.65f, _basePose.cloakCenterRot);
            SetZ(cloakRight, Mathf.Sin(Time.time * 0.91f - 0.55f) * cloakIdleSway, _basePose.cloakRightRot);
        }

        private void ApplyLocomotion(bool running)
        {
            float frequency = running ? runFrequency : walkFrequency;
            float legSwing = running ? runLegSwing : walkLegSwing;
            float armSwing = running ? runArmSwing : walkArmSwing;
            float bob = running ? runBob : walkBob;
            float cloak = running ? cloakRunSway : cloakWalkSway;

            float phase = Time.time * frequency * Mathf.Lerp(0.75f, 1.1f, Mathf.Max(0.15f, _speed01));
            float s = Mathf.Sin(phase);
            float step = Mathf.Abs(s);

            SetZ(thighLeft, s * legSwing, _basePose.thighLeftRot);
            SetZ(thighRight, -s * legSwing, _basePose.thighRightRot);
            SetZ(shinLeft, Mathf.Max(0f, -s) * legSwing * 0.34f, _basePose.shinLeftRot);
            SetZ(shinRight, Mathf.Max(0f, s) * legSwing * 0.34f, _basePose.shinRightRot);
            SetZ(upperArmLeft, -s * armSwing, _basePose.upperArmLeftRot);
            SetZ(upperArmRight, s * armSwing, _basePose.upperArmRightRot);

            SetPosition(visualRoot, _basePose.rootPos + Vector3.up * (step * bob));
            SetPosition(torso, _basePose.torsoPos + Vector3.up * (step * bob * 0.28f));
            SetPosition(head, _basePose.headPos + Vector3.up * (step * bob * 0.12f));

            SetZ(cloakLeft, Mathf.Sin(phase - 0.8f) * cloak, _basePose.cloakLeftRot);
            SetZ(cloakCenter, Mathf.Sin(phase - 0.55f) * cloak * 0.75f, _basePose.cloakCenterRot);
            SetZ(cloakRight, Mathf.Sin(phase - 0.25f) * cloak, _basePose.cloakRightRot);
        }

        private void ApplyGuardPose()
        {
            ResetPose();
            SetZ(upperArmRight, -28f, _basePose.upperArmRightRot);
            SetZ(forearmRight, -22f, _basePose.forearmRightRot);
            SetZ(weaponSocket, -24f, _basePose.weaponRot);
            SetZ(upperArmLeft, 16f, _basePose.upperArmLeftRot);
            SetZ(torso, -4f, _basePose.torsoRot);
            SetZ(cloakCenter, Mathf.Sin(Time.time * 1.1f) * 2.1f, _basePose.cloakCenterRot);
        }

        public void PlayLightAttack() => StartAction(PaperDollMotionState.AttackLight, LightAttackRoutine());
        public void PlayHeavyAttack() => StartAction(PaperDollMotionState.AttackHeavy, HeavyAttackRoutine());
        public void PlayCast() => StartAction(PaperDollMotionState.Cast, CastRoutine());
        public void PlayInteract() => StartAction(PaperDollMotionState.Interact, InteractRoutine());

        public void PlayHit(bool heavy = false)
        {
            if (!_dead)
                StartAction(PaperDollMotionState.Hurt, HurtRoutine(heavy));
        }

        public void PlayDeath()
        {
            if (_dead)
                return;

            _dead = true;
            if (_actionRoutine != null)
                StopCoroutine(_actionRoutine);
            _state = PaperDollMotionState.Dead;
            _actionRoutine = StartCoroutine(DeathRoutine());
        }

        public void ReviveVisual()
        {
            if (_actionRoutine != null)
                StopCoroutine(_actionRoutine);

            _dead = false;
            _actionRoutine = null;
            _state = PaperDollMotionState.Idle;
            ResetPose();
            if (eyesRenderer != null)
                eyesRenderer.enabled = true;
            ScheduleNextBlink();
        }

        private void StartAction(PaperDollMotionState state, IEnumerator routine)
        {
            if (_dead)
                return;

            if (_actionRoutine != null)
                StopCoroutine(_actionRoutine);

            _state = state;
            _actionRoutine = StartCoroutine(ActionWrapper(routine));
        }

        private IEnumerator ActionWrapper(IEnumerator routine)
        {
            ResetPose();
            yield return routine;
            ResetPose();
            _state = PaperDollMotionState.Idle;
            _actionRoutine = null;
            ActionFinished?.Invoke();
        }

        private IEnumerator LightAttackRoutine()
        {
            yield return Tween(0.11f, p =>
            {
                SetZ(torso, Mathf.Lerp(0f, 9f, p), _basePose.torsoRot);
                SetZ(upperArmRight, Mathf.Lerp(0f, 46f, p), _basePose.upperArmRightRot);
                SetZ(forearmRight, Mathf.Lerp(0f, 26f, p), _basePose.forearmRightRot);
                SetZ(weaponSocket, Mathf.Lerp(0f, 38f, p), _basePose.weaponRot);
            });

            bool impact = false;
            yield return Tween(0.085f, p =>
            {
                SetZ(torso, Mathf.Lerp(9f, -13f, p), _basePose.torsoRot);
                SetZ(upperArmRight, Mathf.Lerp(46f, -62f, p), _basePose.upperArmRightRot);
                SetZ(forearmRight, Mathf.Lerp(26f, -35f, p), _basePose.forearmRightRot);
                SetZ(weaponSocket, Mathf.Lerp(38f, -76f, p), _basePose.weaponRot);

                if (!impact && p >= 0.52f)
                {
                    impact = true;
                    AttackImpact?.Invoke();
                }
            });

            yield return Tween(0.17f, p =>
            {
                SetZ(torso, Mathf.Lerp(-13f, 0f, p), _basePose.torsoRot);
                SetZ(upperArmRight, Mathf.Lerp(-62f, 0f, p), _basePose.upperArmRightRot);
                SetZ(forearmRight, Mathf.Lerp(-35f, 0f, p), _basePose.forearmRightRot);
                SetZ(weaponSocket, Mathf.Lerp(-76f, 0f, p), _basePose.weaponRot);
            });
        }

        private IEnumerator HeavyAttackRoutine()
        {
            yield return Tween(0.22f, p =>
            {
                SetZ(torso, Mathf.Lerp(0f, 14f, p), _basePose.torsoRot);
                SetZ(upperArmRight, Mathf.Lerp(0f, 82f, p), _basePose.upperArmRightRot);
                SetZ(forearmRight, Mathf.Lerp(0f, 42f, p), _basePose.forearmRightRot);
                SetZ(weaponSocket, Mathf.Lerp(0f, 72f, p), _basePose.weaponRot);
                SetZ(cloakLeft, Mathf.Lerp(0f, 9f, p), _basePose.cloakLeftRot);
                SetZ(cloakRight, Mathf.Lerp(0f, -8f, p), _basePose.cloakRightRot);
            });

            bool impact = false;
            yield return Tween(0.12f, p =>
            {
                SetZ(torso, Mathf.Lerp(14f, -19f, p), _basePose.torsoRot);
                SetZ(upperArmRight, Mathf.Lerp(82f, -89f, p), _basePose.upperArmRightRot);
                SetZ(forearmRight, Mathf.Lerp(42f, -48f, p), _basePose.forearmRightRot);
                SetZ(weaponSocket, Mathf.Lerp(72f, -102f, p), _basePose.weaponRot);

                if (!impact && p >= 0.47f)
                {
                    impact = true;
                    AttackImpact?.Invoke();
                }
            });

            yield return Tween(0.28f, p =>
            {
                SetZ(torso, Mathf.Lerp(-19f, 0f, p), _basePose.torsoRot);
                SetZ(upperArmRight, Mathf.Lerp(-89f, 0f, p), _basePose.upperArmRightRot);
                SetZ(forearmRight, Mathf.Lerp(-48f, 0f, p), _basePose.forearmRightRot);
                SetZ(weaponSocket, Mathf.Lerp(-102f, 0f, p), _basePose.weaponRot);
            });
        }

        private IEnumerator CastRoutine()
        {
            yield return Tween(0.18f, p =>
            {
                SetZ(upperArmLeft, Mathf.Lerp(0f, -58f, p), _basePose.upperArmLeftRot);
                SetZ(forearmLeft, Mathf.Lerp(0f, -27f, p), _basePose.forearmLeftRot);
                SetZ(head, Mathf.Lerp(0f, -4f, p), _basePose.headRot);
            });

            yield return new WaitForSeconds(0.24f);

            yield return Tween(0.19f, p =>
            {
                SetZ(upperArmLeft, Mathf.Lerp(-58f, 0f, p), _basePose.upperArmLeftRot);
                SetZ(forearmLeft, Mathf.Lerp(-27f, 0f, p), _basePose.forearmLeftRot);
                SetZ(head, Mathf.Lerp(-4f, 0f, p), _basePose.headRot);
            });
        }

        private IEnumerator InteractRoutine()
        {
            yield return Tween(0.16f, p =>
            {
                SetZ(upperArmLeft, Mathf.Lerp(0f, -34f, p), _basePose.upperArmLeftRot);
                SetZ(forearmLeft, Mathf.Lerp(0f, 21f, p), _basePose.forearmLeftRot);
            });

            yield return new WaitForSeconds(0.16f);

            yield return Tween(0.16f, p =>
            {
                SetZ(upperArmLeft, Mathf.Lerp(-34f, 0f, p), _basePose.upperArmLeftRot);
                SetZ(forearmLeft, Mathf.Lerp(21f, 0f, p), _basePose.forearmLeftRot);
            });
        }

        private IEnumerator HurtRoutine(bool heavy)
        {
            float amount = heavy ? 14f : 8f;
            float recoil = heavy ? 0.075f : 0.045f;

            yield return Tween(heavy ? 0.08f : 0.055f, p =>
            {
                SetZ(torso, Mathf.Lerp(0f, amount, p), _basePose.torsoRot);
                SetZ(head, Mathf.Lerp(0f, -amount * 0.45f, p), _basePose.headRot);
                SetPosition(visualRoot, _basePose.rootPos + Vector3.right * (-recoil * p));
            });

            yield return Tween(heavy ? 0.24f : 0.15f, p =>
            {
                SetZ(torso, Mathf.Lerp(amount, 0f, p), _basePose.torsoRot);
                SetZ(head, Mathf.Lerp(-amount * 0.45f, 0f, p), _basePose.headRot);
                SetPosition(visualRoot, Vector3.Lerp(_basePose.rootPos + Vector3.right * -recoil, _basePose.rootPos, p));
            });
        }

        private IEnumerator DeathRoutine()
        {
            ResetPose();
            if (eyesRenderer != null)
                eyesRenderer.enabled = false;

            yield return Tween(0.62f, p =>
            {
                float eased = 1f - Mathf.Pow(1f - p, 2f);
                SetZ(visualRoot, Mathf.Lerp(0f, -78f, eased), _basePose.rootRot);
                SetPosition(visualRoot, _basePose.rootPos + new Vector3(0.22f * eased, -0.20f * eased, 0f));
                SetZ(cloakLeft, Mathf.Lerp(0f, 18f, p), _basePose.cloakLeftRot);
                SetZ(cloakRight, Mathf.Lerp(0f, -12f, p), _basePose.cloakRightRot);
            });

            _actionRoutine = null;
            ActionFinished?.Invoke();
        }

        private IEnumerator Tween(float duration, Action<float> apply)
        {
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                apply?.Invoke(Mathf.Clamp01(elapsed / duration));
                yield return null;
            }

            apply?.Invoke(1f);
        }

        private void UpdateBlink()
        {
            if (eyesRenderer == null || _dead)
                return;

            if (Time.time >= _nextBlinkAt && _blinkUntil <= 0f)
            {
                _blinkUntil = Time.time + blinkDuration;
                eyesRenderer.enabled = false;
            }

            if (_blinkUntil > 0f && Time.time >= _blinkUntil)
            {
                _blinkUntil = 0f;
                eyesRenderer.enabled = true;
                ScheduleNextBlink();
            }
        }

        private void ScheduleNextBlink()
        {
            float min = Mathf.Max(0.4f, blinkInterval.x);
            float max = Mathf.Max(min, blinkInterval.y);
            _nextBlinkAt = Time.time + UnityEngine.Random.Range(min, max);
        }

        private void CaptureBasePose()
        {
            _basePose.rootPos = GetPosition(visualRoot);
            _basePose.torsoPos = GetPosition(torso);
            _basePose.headPos = GetPosition(head);
            _basePose.rootRot = GetRotation(visualRoot);
            _basePose.torsoRot = GetRotation(torso);
            _basePose.headRot = GetRotation(head);
            _basePose.upperArmLeftRot = GetRotation(upperArmLeft);
            _basePose.upperArmRightRot = GetRotation(upperArmRight);
            _basePose.forearmLeftRot = GetRotation(forearmLeft);
            _basePose.forearmRightRot = GetRotation(forearmRight);
            _basePose.thighLeftRot = GetRotation(thighLeft);
            _basePose.thighRightRot = GetRotation(thighRight);
            _basePose.shinLeftRot = GetRotation(shinLeft);
            _basePose.shinRightRot = GetRotation(shinRight);
            _basePose.cloakLeftRot = GetRotation(cloakLeft);
            _basePose.cloakCenterRot = GetRotation(cloakCenter);
            _basePose.cloakRightRot = GetRotation(cloakRight);
            _basePose.weaponRot = GetRotation(weaponSocket);
        }

        private void ResetPose()
        {
            SetPosition(visualRoot, _basePose.rootPos);
            SetPosition(torso, _basePose.torsoPos);
            SetPosition(head, _basePose.headPos);
            ResetRotationsOnly();
        }

        private void ResetRotationsOnly()
        {
            SetRotation(visualRoot, _basePose.rootRot);
            SetRotation(torso, _basePose.torsoRot);
            SetRotation(head, _basePose.headRot);
            SetRotation(upperArmLeft, _basePose.upperArmLeftRot);
            SetRotation(upperArmRight, _basePose.upperArmRightRot);
            SetRotation(forearmLeft, _basePose.forearmLeftRot);
            SetRotation(forearmRight, _basePose.forearmRightRot);
            SetRotation(thighLeft, _basePose.thighLeftRot);
            SetRotation(thighRight, _basePose.thighRightRot);
            SetRotation(shinLeft, _basePose.shinLeftRot);
            SetRotation(shinRight, _basePose.shinRightRot);
            SetRotation(cloakLeft, _basePose.cloakLeftRot);
            SetRotation(cloakCenter, _basePose.cloakCenterRot);
            SetRotation(cloakRight, _basePose.cloakRightRot);
            SetRotation(weaponSocket, _basePose.weaponRot);
        }

        private static Vector3 GetPosition(Transform target) => target != null ? target.localPosition : Vector3.zero;
        private static Quaternion GetRotation(Transform target) => target != null ? target.localRotation : Quaternion.identity;

        private static void SetPosition(Transform target, Vector3 position)
        {
            if (target != null)
                target.localPosition = position;
        }

        private static void SetRotation(Transform target, Quaternion rotation)
        {
            if (target != null)
                target.localRotation = rotation;
        }

        private static void SetZ(Transform target, float degrees, Quaternion baseRotation)
        {
            if (target != null)
                target.localRotation = baseRotation * Quaternion.Euler(0f, 0f, degrees);
        }
    }
}
