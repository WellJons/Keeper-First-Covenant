using UnityEngine;

namespace KeeperFirstCovenant.Core
{
    public sealed class GameAudioService : MonoBehaviour
    {
        private static GameAudioService instance;

        private AudioSource musicSource;
        private AudioSource uiSource;
        private AudioClip fallbackAmbience;
        private AudioClip clickClip;
        private AudioClip hoverClip;

        public static GameAudioService Instance
        {
            get
            {
                EnsureExists();
                return instance;
            }
        }

        public static void EnsureExists()
        {
            if (instance != null)
                return;

            instance = FindFirstObjectByType<GameAudioService>();
            if (instance != null)
                return;

            var root = new GameObject("Keeper_Audio");
            instance = root.AddComponent<GameAudioService>();
        }

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }

            instance = this;
            DontDestroyOnLoad(gameObject);

            musicSource = gameObject.AddComponent<AudioSource>();
            musicSource.loop = true;
            musicSource.playOnAwake = false;
            musicSource.spatialBlend = 0f;

            uiSource = gameObject.AddComponent<AudioSource>();
            uiSource.loop = false;
            uiSource.playOnAwake = false;
            uiSource.spatialBlend = 0f;

            clickClip = BuildTone("Keeper_UI_Click", 360f, 0.055f, 0.22f);
            hoverClip = BuildTone("Keeper_UI_Hover", 520f, 0.035f, 0.10f);

            SettingsService.SettingsChanged += OnSettingsChanged;
            ApplyVolumes(SettingsService.Load());
        }

        private void OnDestroy()
        {
            if (instance == this)
            {
                SettingsService.SettingsChanged -= OnSettingsChanged;
                instance = null;
            }
        }

        public void PlayMenuAmbience(AudioClip authoredClip = null)
        {
            AudioClip clip = authoredClip != null ? authoredClip : GetFallbackAmbience();
            if (clip == null)
                return;

            if (musicSource.clip == clip && musicSource.isPlaying)
                return;

            musicSource.clip = clip;
            musicSource.loop = true;
            musicSource.Play();
        }

        public void StopMenuAmbience()
        {
            if (musicSource != null && musicSource.isPlaying)
                musicSource.Stop();
        }

        public void PlayClick()
        {
            if (clickClip != null)
                uiSource.PlayOneShot(clickClip);
        }

        public void PlayHover()
        {
            if (hoverClip != null)
                uiSource.PlayOneShot(hoverClip);
        }

        private void OnSettingsChanged(GameSettingsData settings)
        {
            ApplyVolumes(settings);
        }

        private void ApplyVolumes(GameSettingsData settings)
        {
            if (settings == null)
                return;

            if (musicSource != null)
                musicSource.volume = settings.musicVolume * 0.24f;

            if (uiSource != null)
                uiSource.volume = settings.sfxVolume;
        }

        private AudioClip GetFallbackAmbience()
        {
            if (fallbackAmbience != null)
                return fallbackAmbience;

            const int sampleRate = 22050;
            const float duration = 12f;
            int samples = Mathf.RoundToInt(sampleRate * duration);
            float[] data = new float[samples];

            for (int i = 0; i < samples; i++)
            {
                float t = i / (float)sampleRate;
                float fadeIn = Mathf.Clamp01(t / 1.8f);
                float fadeOut = Mathf.Clamp01((duration - t) / 1.8f);
                float envelope = Mathf.Min(fadeIn, fadeOut);

                float low = Mathf.Sin(t * Mathf.PI * 2f * 55f) * 0.020f;
                float fifth = Mathf.Sin(t * Mathf.PI * 2f * 82.5f) * 0.012f;
                float shimmerFrequency = 164f + Mathf.Sin(t * 0.23f) * 5f;
                float shimmer = Mathf.Sin(t * Mathf.PI * 2f * shimmerFrequency) * 0.004f;
                float breath = 0.70f + Mathf.Sin(t * 0.41f) * 0.30f;

                data[i] = (low + fifth + shimmer) * breath * envelope;
            }

            fallbackAmbience = AudioClip.Create(
                "Keeper_Menu_Ambience_Fallback",
                samples,
                1,
                sampleRate,
                false);

            fallbackAmbience.SetData(data, 0);
            return fallbackAmbience;
        }

        private static AudioClip BuildTone(string name, float frequency, float duration, float amplitude)
        {
            const int sampleRate = 22050;
            int samples = Mathf.Max(32, Mathf.RoundToInt(sampleRate * duration));
            float[] data = new float[samples];

            for (int i = 0; i < samples; i++)
            {
                float t = i / (float)sampleRate;
                float normalized = i / (float)(samples - 1);
                float envelope = 1f - normalized;
                data[i] = Mathf.Sin(t * Mathf.PI * 2f * frequency) * amplitude * envelope * envelope;
            }

            AudioClip clip = AudioClip.Create(name, samples, 1, sampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }
    }
}
