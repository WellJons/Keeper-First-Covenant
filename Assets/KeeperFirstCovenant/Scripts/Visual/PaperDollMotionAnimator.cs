using System.Collections;
using UnityEngine;

namespace KeeperFirstCovenant.Visual
{
    public sealed class PaperDollMotionAnimator : MonoBehaviour
    {
        [Header("Rig transforms")]
        [SerializeField] private Transform visualRoot;
        [SerializeField] private Transform torso;
        [SerializeField] private Transform head;
        [SerializeField] private Transform armLeft;
        [SerializeField] private Transform armRight;
        [SerializeField] private Transform legLeft;
        [SerializeField] private Transform legRight;
        [SerializeField] private Transform cloak;
        [SerializeField] private Transform weaponSocket;

        [Header("Walk")]
        [SerializeField] private float walkFrequency = 7.5f;
        [SerializeField] private float legSwingDegrees = 15f;
        [SerializeField] private float armSwingDegrees = 11f;
        [SerializeField] private float bodyBob = 0.035f;
        [SerializeField] private float cloakSwayDegrees = 5f;

        [Header("Idle")]
        [SerializeField] private float idleFrequency = 1.6f;
        [SerializeField] private float idleBreath = 0.018f;

        private bool _walking;
        private float _speed01;
        private Coroutine _actionRoutine;

        private Vector3 _rootBase;
        private Vector3 _torsoBase;
        private Vector3 _headBase;

        private void Awake()
        {
            if (visualRoot == null)
                visualRoot = transform;

            _rootBase = visualRoot.localPosition;
            _torsoBase = torso != null ? torso.localPosition : Vector3.zero;
            _headBase = head != null ? head.localPosition : Vector3.zero;
        }

        public void SetLocomotion(bool walking, float normalizedSpeed)
        {
            _walking = walking;
            _speed01 = Mathf.Clamp01(normalizedSpeed);
        }

        private void LateUpdate()
        {
            if (_actionRoutine != null)
                return;

            if (_walking && _speed01 > 0.01f)
                ApplyWalk();
            else
                ApplyIdle();
        }

        private void ApplyWalk()
        {
            float phase = Time.time * walkFrequency * Mathf.Lerp(0.7f, 1.15f, _speed01);
            float sin = Mathf.Sin(phase);
            float abs = Mathf.Abs(sin);

            SetLocalZ(legLeft, sin * legSwingDegrees);
            SetLocalZ(legRight, -sin * legSwingDegrees);
            SetLocalZ(armLeft, -sin * armSwingDegrees);
            SetLocalZ(armRight, sin * armSwingDegrees);
            SetLocalZ(cloak, Mathf.Sin(phase - 0.55f) * cloakSwayDegrees);

            if (visualRoot != null)
                visualRoot.localPosition = _rootBase + Vector3.up * (abs * bodyBob);

            if (torso != null)
                torso.localPosition = _torsoBase + Vector3.up * (abs * bodyBob * 0.35f);

            if (head != null)
                head.localPosition = _headBase + Vector3.up * (abs * bodyBob * 0.2f);
        }

        private void ApplyIdle()
        {
            float breath = (Mathf.Sin(Time.time * idleFrequency) * 0.5f + 0.5f) * idleBreath;

            ResetLimbRotations();

            if (visualRoot != null)
                visualRoot.localPosition = _rootBase;

            if (torso != null)
                torso.localPosition = _torsoBase + Vector3.up * breath;

            if (head != null)
                head.localPosition = _headBase + Vector3.up * breath * 0.45f;

            SetLocalZ(cloak, Mathf.Sin(Time.time * 0.9f) * 1.2f);
        }

        public void PlaySwordAttack()
        {
            StartAction(SwordAttackRoutine());
        }

        public void PlayCast()
        {
            StartAction(CastRoutine());
        }

        private void StartAction(IEnumerator routine)
        {
            if (_actionRoutine != null)
                StopCoroutine(_actionRoutine);

            _actionRoutine = StartCoroutine(ActionWrapper(routine));
        }

        private IEnumerator ActionWrapper(IEnumerator routine)
        {
            ResetPose();
            yield return routine;
            ResetPose();
            _actionRoutine = null;
        }

        private IEnumerator SwordAttackRoutine()
        {
            const float windup = 0.12f;
            const float strike = 0.10f;
            const float recover = 0.18f;

            float t = 0f;
            while (t < windup)
            {
                t += Time.deltaTime;
                float p = Mathf.Clamp01(t / windup);
                SetLocalZ(armRight, Mathf.Lerp(0f, 42f, p));
                SetLocalZ(weaponSocket, Mathf.Lerp(0f, 34f, p));
                yield return null;
            }

            t = 0f;
            while (t < strike)
            {
                t += Time.deltaTime;
                float p = Mathf.Clamp01(t / strike);
                SetLocalZ(armRight, Mathf.Lerp(42f, -68f, p));
                SetLocalZ(weaponSocket, Mathf.Lerp(34f, -78f, p));
                yield return null;
            }

            t = 0f;
            while (t < recover)
            {
                t += Time.deltaTime;
                float p = Mathf.Clamp01(t / recover);
                SetLocalZ(armRight, Mathf.Lerp(-68f, 0f, p));
                SetLocalZ(weaponSocket, Mathf.Lerp(-78f, 0f, p));
                yield return null;
            }
        }

        private IEnumerator CastRoutine()
        {
            const float raise = 0.18f;
            const float hold = 0.28f;
            const float recover = 0.20f;

            float t = 0f;
            while (t < raise)
            {
                t += Time.deltaTime;
                float p = Mathf.Clamp01(t / raise);
                SetLocalZ(armLeft, Mathf.Lerp(0f, -55f, p));
                yield return null;
            }

            yield return new WaitForSeconds(hold);

            t = 0f;
            while (t < recover)
            {
                t += Time.deltaTime;
                float p = Mathf.Clamp01(t / recover);
                SetLocalZ(armLeft, Mathf.Lerp(-55f, 0f, p));
                yield return null;
            }
        }

        private void ResetPose()
        {
            ResetLimbRotations();

            if (visualRoot != null)
                visualRoot.localPosition = _rootBase;

            if (torso != null)
                torso.localPosition = _torsoBase;

            if (head != null)
                head.localPosition = _headBase;
        }

        private void ResetLimbRotations()
        {
            SetLocalZ(legLeft, 0f);
            SetLocalZ(legRight, 0f);
            SetLocalZ(armLeft, 0f);
            SetLocalZ(armRight, 0f);
            SetLocalZ(cloak, 0f);
            SetLocalZ(weaponSocket, 0f);
        }

        private static void SetLocalZ(Transform target, float degrees)
        {
            if (target == null)
                return;

            target.localRotation = Quaternion.Euler(0f, 0f, degrees);
        }
    }
}
