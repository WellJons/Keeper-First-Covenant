using System;
using UnityEngine;

namespace KeeperFirstCovenant.World
{
    public readonly struct WorldNoiseEvent
    {
        public readonly Vector3 Position;
        public readonly float Radius;
        public readonly GameObject Source;
        public readonly float Intensity;

        public WorldNoiseEvent(
            Vector3 position,
            float radius,
            GameObject source,
            float intensity = 1f)
        {
            Position = position;
            Radius = Mathf.Max(0f, radius);
            Source = source;
            Intensity = Mathf.Max(0f, intensity);
        }
    }

    public static class WorldNoiseSystem
    {
        public static event Action<WorldNoiseEvent>
            NoiseEmitted;

        public static void Emit(
            Vector3 position,
            float radius,
            GameObject source = null,
            float intensity = 1f)
        {
            if (radius <= 0f ||
                intensity <= 0f)
            {
                return;
            }

            NoiseEmitted?.Invoke(
                new WorldNoiseEvent(
                    position,
                    radius,
                    source,
                    intensity));
        }
    }
}
