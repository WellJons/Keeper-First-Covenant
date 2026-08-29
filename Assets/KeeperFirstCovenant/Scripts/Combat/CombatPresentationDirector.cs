using System.Collections;
using KeeperFirstCovenant.World;
using UnityEngine;

namespace KeeperFirstCovenant.Combat
{
    public sealed class CombatPresentationDirector : MonoBehaviour
    {
        [SerializeField]
        private Camera worldCamera;

        [SerializeField]
        private AudioSource audioSource;

        private CameraImpactShake _shake;

        private void Awake()
        {
            if (worldCamera == null)
                worldCamera = Camera.main;

            EnsureCameraShake();

            if (audioSource == null)
                audioSource = GetComponent<AudioSource>();

            if (audioSource == null)
                audioSource = gameObject.AddComponent<AudioSource>();

            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 0f;
        }

        private void OnEnable()
        {
            CombatActionExecutor.ActionPresentationRequested +=
                OnPresentationRequested;
        }

        private void OnDisable()
        {
            CombatActionExecutor.ActionPresentationRequested -=
                OnPresentationRequested;
        }

        private void OnPresentationRequested(
            CombatPresentationRequest request)
        {
            CombatPresentationProfile profile =
                request.Action != null
                    ? request.Action.presentationProfile
                    : null;

            if (profile == null)
                return;

            EnsureCameraShake();

            if (_shake != null)
            {
                _shake.AddImpulse(
                    profile.cameraShakeAmplitude,
                    profile.cameraShakeDuration,
                    profile.cameraShakeFrequency);
            }

            if (profile.hitStopSeconds > 0f)
            {
                StartCoroutine(
                    HitStopRoutine(profile));
            }

            if (profile.impactLightIntensity > 0f &&
                profile.impactLightDuration > 0f)
            {
                StartCoroutine(
                    ImpactLightRoutine(
                        profile,
                        request.ImpactPoint));
            }

            ApplyEnvironmentImpulse(
                profile,
                request.ImpactPoint);

            if (profile.worldNoiseRadius > 0f)
            {
                WorldNoiseSystem.Emit(
                    request.ImpactPoint,
                    profile.worldNoiseRadius,
                    request.Actor != null
                        ? request.Actor.gameObject
                        : gameObject,
                    profile.worldNoiseIntensity);
            }

            SpawnAssetHooks(
                profile,
                request);

            PlayImpactAudio(profile);
        }

        private void EnsureCameraShake()
        {
            if (worldCamera == null)
                worldCamera = Camera.main;

            if (worldCamera == null)
                return;

            _shake =
                worldCamera.GetComponent<
                    CameraImpactShake>();

            if (_shake == null)
            {
                _shake =
                    worldCamera.gameObject
                        .AddComponent<
                            CameraImpactShake>();
            }
        }

        private IEnumerator HitStopRoutine(
            CombatPresentationProfile profile)
        {
            if (Time.timeScale <= 0f)
                yield break;

            float previousScale =
                Time.timeScale;

            float impactScale =
                Mathf.Clamp(
                    profile.hitStopTimeScale,
                    0.01f,
                    1f);

            Time.timeScale =
                previousScale * impactScale;

            float elapsed = 0f;

            while (elapsed <
                   profile.hitStopSeconds)
            {
                elapsed +=
                    Time.unscaledDeltaTime;

                yield return null;
            }

            // Avoid overriding another system that paused the game.
            if (Time.timeScale > 0f)
                Time.timeScale = previousScale;
        }

        private IEnumerator ImpactLightRoutine(
            CombatPresentationProfile profile,
            Vector3 point)
        {
            GameObject lightObject =
                new GameObject(
                    "CombatImpactLight");

            lightObject.transform.position =
                point + Vector3.up * 0.35f;

            Light light =
                lightObject.AddComponent<Light>();

            light.type = LightType.Point;
            light.color =
                profile.impactLightColor;

            light.range =
                profile.impactLightRange;

            light.intensity =
                profile.impactLightIntensity;

            float duration =
                Mathf.Max(
                    0.01f,
                    profile.impactLightDuration);

            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed +=
                    Time.unscaledDeltaTime;

                float normalized =
                    Mathf.Clamp01(
                        1f - elapsed / duration);

                light.intensity =
                    profile.impactLightIntensity *
                    normalized *
                    normalized;

                yield return null;
            }

            Destroy(lightObject);
        }

        private static void ApplyEnvironmentImpulse(
            CombatPresentationProfile profile,
            Vector3 point)
        {
            if (profile.environmentImpulseRadius <= 0f ||
                profile.environmentImpulseForce <= 0f)
            {
                return;
            }

            Collider[] colliders =
                Physics.OverlapSphere(
                    point,
                    profile.environmentImpulseRadius,
                    ~0,
                    QueryTriggerInteraction.Ignore);

            foreach (Collider collider in colliders)
            {
                Rigidbody body =
                    collider.attachedRigidbody;

                if (body == null ||
                    body.isKinematic)
                {
                    continue;
                }

                body.AddExplosionForce(
                    profile.environmentImpulseForce,
                    point,
                    profile.environmentImpulseRadius,
                    0.15f,
                    ForceMode.Impulse);
            }
        }

        private void SpawnAssetHooks(
            CombatPresentationProfile profile,
            CombatPresentationRequest request)
        {
            if (profile.castVfxPrefab != null)
            {
                GameObject cast =
                    Instantiate(
                        profile.castVfxPrefab,
                        request.Origin,
                        Quaternion.identity);

                Destroy(
                    cast,
                    profile.spawnedVfxLifetime);
            }

            if (profile.impactVfxPrefab != null)
            {
                GameObject impact =
                    Instantiate(
                        profile.impactVfxPrefab,
                        request.ImpactPoint,
                        Quaternion.identity);

                Destroy(
                    impact,
                    profile.spawnedVfxLifetime);
            }

            if (profile.groundDecalPrefab != null)
            {
                GameObject decal =
                    Instantiate(
                        profile.groundDecalPrefab,
                        request.ImpactPoint +
                        Vector3.up * 0.02f,
                        Quaternion.identity);

                Destroy(
                    decal,
                    profile.decalLifetime);
            }
        }

        private void PlayImpactAudio(
            CombatPresentationProfile profile)
        {
            if (audioSource == null ||
                profile.impactSound == null)
            {
                return;
            }

            audioSource.PlayOneShot(
                profile.impactSound);
        }
    }
}
