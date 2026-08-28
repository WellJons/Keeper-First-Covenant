using System.Collections;
using KeeperFirstCovenant.Combat;
using UnityEngine;

namespace KeeperFirstCovenant.Visual
{
    [DisallowMultipleComponent]
    public sealed class PaperDollBloodVisual : MonoBehaviour
    {
        [SerializeField] private CombatantRuntime combatant;
        [SerializeField] private PaperDollMotionAnimator motion;
        [SerializeField] private Transform emissionPoint;
        [SerializeField] private int physicalDamageToStartBleeding = 8;
        [SerializeField] private float lowHealthBleedThreshold = 0.35f;
        [SerializeField] private Color bloodColor = new Color(0.34f, 0.015f, 0.018f, 1f);

        private ParticleSystem _particles;
        private float _bleedIntensity;
        private float _nextDrip;
        private Coroutine _flashRoutine;
        private PaperDollCharacterVisual _visual;
        private Transform _bloodPool;
        private Material _bloodPoolMaterial;

        private void Awake()
        {
            if (combatant == null)
                combatant = GetComponentInParent<CombatantRuntime>();
            if (motion == null)
                motion = GetComponentInChildren<PaperDollMotionAnimator>();
            if (emissionPoint == null)
                emissionPoint = transform;

            _visual = GetComponentInChildren<PaperDollCharacterVisual>();
            BuildParticles();
            BuildBloodPool();
        }

        private void OnEnable()
        {
            if (combatant == null)
                return;

            combatant.Damaged += OnDamaged;
            combatant.Died += OnDied;
            combatant.Changed += OnChanged;
        }

        private void OnDisable()
        {
            if (combatant == null)
                return;

            combatant.Damaged -= OnDamaged;
            combatant.Died -= OnDied;
            combatant.Changed -= OnChanged;
        }

        private void Update()
        {
            if (_bleedIntensity <= 0.01f || combatant == null || !combatant.IsAlive)
                return;

            UpdateBloodPool();

            if (Time.time < _nextDrip)
                return;

            EmitBlood(Mathf.Clamp(Mathf.CeilToInt(_bleedIntensity * 2f), 1, 3), 0.45f);
            _nextDrip = Time.time + Mathf.Lerp(2f, 0.45f, Mathf.Clamp01(_bleedIntensity));
        }

        public void EmitHit(int damage, bool critical)
        {
            int count = Mathf.Clamp(2 + damage / 4 + (critical ? 3 : 0), 2, 12);
            EmitBlood(count, critical ? 1.45f : 1f);
        }

        private void OnDamaged(CombatantRuntime source, DamagePacket packet)
        {
            bool physical = packet.Type == DamageType.Physical || packet.Type == DamageType.Bleeding;
            bool heavy = packet.Critical || packet.Amount >= physicalDamageToStartBleeding * 2;

            motion?.PlayHit(heavy);
            EmitHit(packet.Amount, packet.Critical);

            if (physical && packet.Amount >= physicalDamageToStartBleeding)
                _bleedIntensity = Mathf.Clamp01(_bleedIntensity + packet.Amount / 28f);

            if (_flashRoutine != null)
                StopCoroutine(_flashRoutine);
            _flashRoutine = StartCoroutine(DamageFlash());
        }

        private void OnChanged(CombatantRuntime source)
        {
            if (source.Definition == null || source.Definition.maxHealth <= 0)
                return;

            float health01 = source.CurrentHealth / (float)source.Definition.maxHealth;
            if (health01 >= 0.98f)
            {
                _bleedIntensity = 0f;
                if (_bloodPool != null)
                    _bloodPool.localScale = Vector3.zero;
                return;
            }

            if (health01 <= lowHealthBleedThreshold)
            {
                float criticality = Mathf.InverseLerp(lowHealthBleedThreshold, 0f, health01);
                _bleedIntensity = Mathf.Max(_bleedIntensity, criticality);
            }
        }

        private void OnDied(CombatantRuntime source)
        {
            _bleedIntensity = 1f;
            EmitBlood(18, 1.55f);
            UpdateBloodPool(true);
            motion?.PlayDeath();
        }

        private IEnumerator DamageFlash()
        {
            if (_visual == null)
                yield break;

            _visual.SetTint(new Color(1f, 0.55f, 0.55f, 1f));
            yield return new WaitForSeconds(0.07f);
            _visual.SetTint(Color.white);
            _flashRoutine = null;
        }

        private void BuildBloodPool()
        {
            GameObject pool = GameObject.CreatePrimitive(PrimitiveType.Quad);
            pool.name = "BloodPool";

            Collider collider = pool.GetComponent<Collider>();
            if (collider != null)
                Destroy(collider);

            _bloodPool = pool.transform;
            _bloodPool.SetParent(transform, false);
            _bloodPool.localPosition = new Vector3(0f, 0.012f, 0f);
            _bloodPool.localRotation = Quaternion.Euler(90f, 0f, 0f);
            _bloodPool.localScale = Vector3.zero;

            Shader shader = Shader.Find("Universal Render Pipeline/Unlit");
            if (shader == null)
                shader = Shader.Find("Unlit/Color");
            if (shader == null)
                return;

            _bloodPoolMaterial = new Material(shader);
            Color poolColor = new Color(
                bloodColor.r * 0.72f,
                bloodColor.g * 0.72f,
                bloodColor.b * 0.72f,
                0.72f);

            if (_bloodPoolMaterial.HasProperty("_BaseColor"))
                _bloodPoolMaterial.SetColor("_BaseColor", poolColor);
            else if (_bloodPoolMaterial.HasProperty("_Color"))
                _bloodPoolMaterial.SetColor("_Color", poolColor);

            MeshRenderer renderer = pool.GetComponent<MeshRenderer>();
            if (renderer != null)
                renderer.sharedMaterial = _bloodPoolMaterial;
        }

        private void UpdateBloodPool(bool forceDeathPool = false)
        {
            if (_bloodPool == null)
                return;

            float target = forceDeathPool
                ? 0.72f
                : Mathf.Lerp(0f, 0.38f, Mathf.Clamp01(_bleedIntensity));

            Vector3 desired = new Vector3(target * 1.55f, target, target);
            _bloodPool.localScale = forceDeathPool
                ? desired
                : Vector3.Lerp(_bloodPool.localScale, desired, Time.deltaTime * 1.5f);
        }

        private void BuildParticles()
        {
            GameObject go = new GameObject("BloodParticles");
            go.transform.SetParent(emissionPoint, false);
            _particles = go.AddComponent<ParticleSystem>();

            ParticleSystem.MainModule main = _particles.main;
            main.playOnAwake = false;
            main.loop = false;
            main.startLifetime = new ParticleSystem.MinMaxCurve(0.35f, 0.9f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(0.8f, 2.4f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.025f, 0.075f);
            main.startColor = bloodColor;
            main.gravityModifier = 1.15f;
            main.simulationSpace = ParticleSystemSimulationSpace.World;

            ParticleSystem.EmissionModule emission = _particles.emission;
            emission.enabled = false;

            ParticleSystem.ShapeModule shape = _particles.shape;
            shape.shapeType = ParticleSystemShapeType.Cone;
            shape.angle = 32f;
            shape.radius = 0.06f;
        }

        private void EmitBlood(int count, float speedMultiplier)
        {
            if (_particles == null)
                return;

            ParticleSystem.EmitParams emit = new ParticleSystem.EmitParams
            {
                startColor = bloodColor
            };

            for (int i = 0; i < count; i++)
            {
                emit.startSize = Random.Range(0.025f, 0.075f);
                emit.velocity = new Vector3(
                    Random.Range(-0.85f, 0.85f),
                    Random.Range(0.35f, 1.5f),
                    Random.Range(-0.85f, 0.85f)) * speedMultiplier;
                _particles.Emit(emit, 1);
            }
        }
    }
}
