#if UNITY_EDITOR
using KeeperFirstCovenant.Combat;
using UnityEditor;
using UnityEngine;

namespace KeeperFirstCovenant.EditorTools
{
    public static class PrototypeCombatPresentationContent
    {
        private const string DataRoot =
            "Assets/KeeperFirstCovenant/Generated/Data";

        public static void Build()
        {
            if (!AssetDatabase.IsValidFolder(
                    "Assets/KeeperFirstCovenant/Generated"))
            {
                return;
            }

            CombatPresentationProfile light =
                GetOrCreateProfile(
                    "Impact_Light");

            Configure(
                light,
                ImpactTier.Light,
                0.07f,
                0.08f,
                28f,
                0.012f,
                0.15f,
                new Color(1f, 0.92f, 0.75f),
                2.5f,
                4f,
                0.10f,
                1.5f,
                1.0f);

            CombatPresentationProfile fireHeavy =
                GetOrCreateProfile(
                    "Impact_FireHeavy");

            Configure(
                fireHeavy,
                ImpactTier.Heavy,
                0.26f,
                0.22f,
                24f,
                0.045f,
                0.07f,
                new Color(1f, 0.26f, 0.05f),
                13f,
                8f,
                0.28f,
                4.5f,
                4.5f);

            CombatPresentationProfile lightning =
                GetOrCreateProfile(
                    "Impact_LightningHeavy");

            Configure(
                lightning,
                ImpactTier.Heavy,
                0.18f,
                0.16f,
                32f,
                0.028f,
                0.08f,
                new Color(0.18f, 0.42f, 1f),
                16f,
                7f,
                0.15f,
                3.5f,
                3f);

            CombatPresentationProfile frost =
                GetOrCreateProfile(
                    "Impact_FrostControl");

            Configure(
                frost,
                ImpactTier.Light,
                0.09f,
                0.10f,
                25f,
                0.018f,
                0.12f,
                new Color(0.55f, 0.85f, 1f),
                7f,
                6f,
                0.20f,
                2.5f,
                1.8f);

            CombatPresentationProfile support =
                GetOrCreateProfile(
                    "Impact_Support");

            Configure(
                support,
                ImpactTier.Subtle,
                0.025f,
                0.06f,
                20f,
                0f,
                1f,
                new Color(0.75f, 0.95f, 1f),
                5f,
                5f,
                0.18f,
                0f,
                0f);

            CombatPresentationProfile rift =
                GetOrCreateProfile(
                    "Impact_RiftMythic");

            Configure(
                rift,
                ImpactTier.Mythic,
                0.38f,
                0.30f,
                38f,
                0.060f,
                0.045f,
                new Color(0.86f, 0.92f, 1f),
                20f,
                11f,
                0.32f,
                5.5f,
                5.5f);

            Assign(
                "Action_SwordSlash.asset",
                light);

            Assign(
                "Action_Shove.asset",
                light);

            Assign(
                "Action_FireBurst.asset",
                fireHeavy);

            Assign(
                "Action_LightningArc.asset",
                lightning);

            Assign(
                "Action_FrostField.asset",
                frost);

            Assign(
                "Action_HealingLight.asset",
                support);

            Assign(
                "Action_SilverBarrier.asset",
                support);

            Assign(
                "Action_Rift.asset",
                rift);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private static void Configure(
            CombatPresentationProfile profile,
            ImpactTier tier,
            float shakeAmplitude,
            float shakeDuration,
            float shakeFrequency,
            float hitStopSeconds,
            float hitStopScale,
            Color lightColor,
            float lightIntensity,
            float lightRange,
            float lightDuration,
            float impulseRadius,
            float impulseForce)
        {
            profile.impactTier = tier;
            profile.cameraShakeAmplitude =
                shakeAmplitude;
            profile.cameraShakeDuration =
                shakeDuration;
            profile.cameraShakeFrequency =
                shakeFrequency;
            profile.hitStopSeconds =
                hitStopSeconds;
            profile.hitStopTimeScale =
                hitStopScale;
            profile.impactLightColor =
                lightColor;
            profile.impactLightIntensity =
                lightIntensity;
            profile.impactLightRange =
                lightRange;
            profile.impactLightDuration =
                lightDuration;
            profile.environmentImpulseRadius =
                impulseRadius;
            profile.environmentImpulseForce =
                impulseForce;

            EditorUtility.SetDirty(profile);
        }

        private static CombatPresentationProfile
            GetOrCreateProfile(
                string id)
        {
            string path =
                DataRoot +
                "/" +
                id +
                ".asset";

            CombatPresentationProfile profile =
                AssetDatabase.LoadAssetAtPath<
                    CombatPresentationProfile>(
                    path);

            if (profile != null)
                return profile;

            profile =
                ScriptableObject.CreateInstance<
                    CombatPresentationProfile>();

            AssetDatabase.CreateAsset(
                profile,
                path);

            return profile;
        }

        private static void Assign(
            string actionFile,
            CombatPresentationProfile profile)
        {
            CombatActionDefinition action =
                AssetDatabase.LoadAssetAtPath<
                    CombatActionDefinition>(
                    DataRoot + "/" + actionFile);

            if (action == null)
                return;

            action.presentationProfile =
                profile;

            EditorUtility.SetDirty(action);
        }
    }
}
#endif
