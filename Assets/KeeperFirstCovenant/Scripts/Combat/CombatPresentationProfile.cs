using UnityEngine;

namespace KeeperFirstCovenant.Combat
{
    public enum ImpactTier
    {
        Subtle,
        Light,
        Heavy,
        Devastating,
        Mythic
    }

    [CreateAssetMenu(
        menuName = "Keeper First Covenant/Combat Presentation Profile",
        fileName = "CombatPresentation")]
    public sealed class CombatPresentationProfile : ScriptableObject
    {
        [Header("Identity")]
        public ImpactTier impactTier = ImpactTier.Light;

        [Header("Camera")]
        [Min(0f)]
        public float cameraShakeAmplitude = 0.15f;

        [Min(0f)]
        public float cameraShakeDuration = 0.12f;

        [Min(0.1f)]
        public float cameraShakeFrequency = 24f;

        [Header("Impact pause")]
        [Min(0f)]
        public float hitStopSeconds = 0.025f;

        [Range(0.01f, 1f)]
        public float hitStopTimeScale = 0.08f;

        [Header("Impact light")]
        public Color impactLightColor = Color.white;

        [Min(0f)]
        public float impactLightIntensity = 4f;

        [Min(0f)]
        public float impactLightRange = 6f;

        [Min(0f)]
        public float impactLightDuration = 0.18f;

        [Header("World awareness")]
        [Min(0f)]
        public float worldNoiseRadius = 6f;

        [Min(0f)]
        public float worldNoiseIntensity = 1f;

        [Header("World impulse")]
        [Min(0f)]
        public float environmentImpulseRadius = 3f;

        [Min(0f)]
        public float environmentImpulseForce = 2.5f;

        [Header("Asset hooks")]
        public GameObject castVfxPrefab;
        public GameObject impactVfxPrefab;
        public GameObject groundDecalPrefab;
        public AudioClip castSound;
        public AudioClip impactSound;

        [Header("Lifetime")]
        [Min(0.1f)]
        public float spawnedVfxLifetime = 5f;

        [Min(0.1f)]
        public float decalLifetime = 12f;
    }

    public readonly struct CombatPresentationRequest
    {
        public readonly CombatActionDefinition Action;
        public readonly CombatantRuntime Actor;
        public readonly CombatantRuntime PrimaryTarget;
        public readonly Vector3 Origin;
        public readonly Vector3 ImpactPoint;
        public readonly CombatActionResult Result;

        public CombatPresentationRequest(
            CombatActionDefinition action,
            CombatantRuntime actor,
            CombatantRuntime primaryTarget,
            Vector3 origin,
            Vector3 impactPoint,
            CombatActionResult result)
        {
            Action = action;
            Actor = actor;
            PrimaryTarget = primaryTarget;
            Origin = origin;
            ImpactPoint = impactPoint;
            Result = result;
        }
    }
}
