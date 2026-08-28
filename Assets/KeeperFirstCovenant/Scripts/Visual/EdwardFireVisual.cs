using UnityEngine;

namespace KeeperFirstCovenant.Visual
{
    [DisallowMultipleComponent]
    public sealed class EdwardFireVisual : MonoBehaviour
    {
        [SerializeField] private PaperDollMotionAnimator motion;
        [SerializeField] private Transform weaponSocket;
        [SerializeField] private Transform castingHand;
        [SerializeField] private Color hotColor = new Color(1f, 0.32f, 0.045f, 1f);
        [SerializeField] private Color coreColor = new Color(1f, 0.78f, 0.24f, 1f);

        private ParticleSystem _weaponEmbers;
        private ParticleSystem _handFlame;
        private Light _light;

        private void Awake()
        {
            if (motion == null)
                motion = GetComponentInChildren<PaperDollMotionAnimator>();
            if (weaponSocket == null && GetComponentInChildren<PaperDollCharacterVisual>() != null)
                weaponSocket = GetComponentInChildren<PaperDollCharacterVisual>().WeaponSocket;

            _weaponEmbers = CreateSystem("SwordFire", weaponSocket != null ? weaponSocket : transform, 0.035f, 0.18f);
            _handFlame = CreateSystem("HandFire", castingHand != null ? castingHand : transform, 0.05f, 0.26f);

            GameObject lightObject = new GameObject("FireLight");
            lightObject.transform.SetParent(transform, false);
            lightObject.transform.localPosition = new Vector3(0.3f, 1.25f, 0f);
            _light = lightObject.AddComponent<Light>();
            _light.type = LightType.Point;
            _light.range = 4.5f;
            _light.intensity = 0f;
            _light.color = new Color(1f, 0.28f, 0.06f);
        }

        public void Configure(PaperDollMotionAnimator configuredMotion, Transform configuredWeaponSocket, Transform configuredCastingHand)
        {
            motion = configuredMotion;
            weaponSocket = configuredWeaponSocket;
            castingHand = configuredCastingHand;
        }

        private void Update()
        {
            if (motion == null)
                return;

            float swordRate = 0f;
            float handRate = 0f;
            float lightIntensity = 0.12f;

            switch (motion.State)
            {
                case PaperDollMotionState.AttackLight:
                    swordRate = 42f;
                    lightIntensity = 1.7f;
                    break;
                case PaperDollMotionState.AttackHeavy:
                    swordRate = 76f;
                    lightIntensity = 2.5f;
                    break;
                case PaperDollMotionState.Cast:
                    handRate = 95f;
                    swordRate = 12f;
                    lightIntensity = 3.2f;
                    break;
                case PaperDollMotionState.Hurt:
                    swordRate = 6f;
                    lightIntensity = 0.55f;
                    break;
                case PaperDollMotionState.Dead:
                    lightIntensity = 0f;
                    break;
                default:
                    swordRate = 2f;
                    lightIntensity = 0.18f;
                    break;
            }

            SetRate(_weaponEmbers, swordRate);
            SetRate(_handFlame, handRate);
            if (_light != null)
                _light.intensity = Mathf.Lerp(_light.intensity, lightIntensity, Time.deltaTime * 10f);
        }

        private ParticleSystem CreateSystem(string name, Transform parent, float minSize, float maxSize)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent, false);
            ParticleSystem ps = go.AddComponent<ParticleSystem>();

            ParticleSystem.MainModule main = ps.main;
            main.loop = true;
            main.playOnAwake = true;
            main.startLifetime = new ParticleSystem.MinMaxCurve(0.16f, 0.42f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(0.12f, 0.8f);
            main.startSize = new ParticleSystem.MinMaxCurve(minSize, maxSize);
            main.startColor = new ParticleSystem.MinMaxGradient(coreColor, hotColor);
            main.gravityModifier = -0.18f;
            main.simulationSpace = ParticleSystemSimulationSpace.World;

            ParticleSystem.ShapeModule shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Cone;
            shape.angle = 16f;
            shape.radius = 0.035f;

            ParticleSystem.ColorOverLifetimeModule color = ps.colorOverLifetime;
            color.enabled = true;
            Gradient gradient = new Gradient();
            gradient.SetKeys(
                new[]
                {
                    new GradientColorKey(coreColor, 0f),
                    new GradientColorKey(hotColor, 0.45f),
                    new GradientColorKey(new Color(0.45f, 0.025f, 0.005f), 1f)
                },
                new[]
                {
                    new GradientAlphaKey(0f, 0f),
                    new GradientAlphaKey(1f, 0.12f),
                    new GradientAlphaKey(0f, 1f)
                });
            color.color = gradient;

            SetRate(ps, 0f);
            return ps;
        }

        private static void SetRate(ParticleSystem system, float rate)
        {
            if (system == null)
                return;
            ParticleSystem.EmissionModule emission = system.emission;
            emission.rateOverTime = rate;
        }
    }
}
