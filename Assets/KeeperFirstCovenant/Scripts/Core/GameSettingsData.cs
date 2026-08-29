using System;
using UnityEngine;

namespace KeeperFirstCovenant.Core
{
    [Serializable]
    public sealed class GameSettingsData
    {
        public float masterVolume = 0.85f;
        public float musicVolume = 0.65f;
        public float sfxVolume = 0.80f;

        public int width = 1920;
        public int height = 1080;
        public FullScreenMode fullscreenMode = FullScreenMode.FullScreenWindow;
        public int qualityLevel = -1;
        public bool vSync = true;
        public int targetFrameRate = 60;

        public bool cameraShake = true;
        public float uiScale = 1f;

        public static GameSettingsData CreateDefaults()
        {
            var result = new GameSettingsData
            {
                width = Screen.currentResolution.width > 0 ? Screen.currentResolution.width : 1920,
                height = Screen.currentResolution.height > 0 ? Screen.currentResolution.height : 1080,
                fullscreenMode = Screen.fullScreenMode,
                qualityLevel = Mathf.Max(0, QualitySettings.GetQualityLevel()),
                vSync = true,
                targetFrameRate = 60,
                masterVolume = 0.85f,
                musicVolume = 0.65f,
                sfxVolume = 0.80f,
                cameraShake = true,
                uiScale = 1f
            };

            result.Clamp();
            return result;
        }

        public GameSettingsData Clone()
        {
            return new GameSettingsData
            {
                masterVolume = masterVolume,
                musicVolume = musicVolume,
                sfxVolume = sfxVolume,
                width = width,
                height = height,
                fullscreenMode = fullscreenMode,
                qualityLevel = qualityLevel,
                vSync = vSync,
                targetFrameRate = targetFrameRate,
                cameraShake = cameraShake,
                uiScale = uiScale
            };
        }

        public void Clamp()
        {
            masterVolume = Mathf.Clamp01(masterVolume);
            musicVolume = Mathf.Clamp01(musicVolume);
            sfxVolume = Mathf.Clamp01(sfxVolume);

            width = Mathf.Max(640, width);
            height = Mathf.Max(360, height);
            qualityLevel = Mathf.Clamp(
                qualityLevel < 0 ? QualitySettings.GetQualityLevel() : qualityLevel,
                0,
                Mathf.Max(0, QualitySettings.names.Length - 1));

            targetFrameRate = targetFrameRate == 0 ? -1 : Mathf.Clamp(targetFrameRate, -1, 360);
            uiScale = Mathf.Clamp(uiScale, 0.75f, 1.35f);
        }
    }
}
