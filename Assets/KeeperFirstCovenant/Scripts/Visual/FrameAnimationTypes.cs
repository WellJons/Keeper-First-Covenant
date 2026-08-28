using System;
using UnityEngine;

namespace KeeperFirstCovenant.Visual
{
    public enum SpriteFacing8
    {
        North,
        NorthEast,
        East,
        SouthEast,
        South,
        SouthWest,
        West,
        NorthWest
    }

    public enum CharacterFrameState
    {
        Idle,
        Walk,
        Run,
        CombatIdle,
        Guard,
        AttackLight,
        AttackHeavy,
        Cast,
        Interact,
        Hit,
        CriticalHit,
        Knockdown,
        Death
    }

    public enum VisualEquipmentSlot
    {
        Armor,
        Cloak,
        Weapon,
        Headgear,
        Accessory
    }

    [Serializable]
    public sealed class DirectionalFrameStrip8
    {
        [Header("Authored strips")]
        public Sprite[] north;
        public Sprite[] northEast;
        public Sprite[] east;
        public Sprite[] southEast;
        public Sprite[] south;

        [Header("Optional unique west strips")]
        public Sprite[] southWest;
        public Sprite[] west;
        public Sprite[] northWest;

        [Tooltip("Mirror east-facing strips when west-facing art is not authored yet.")]
        public bool mirrorMissingWest = true;

        public Sprite[] Get(SpriteFacing8 facing, out bool flipX)
        {
            flipX = false;

            switch (facing)
            {
                case SpriteFacing8.North:
                    return north;
                case SpriteFacing8.NorthEast:
                    return northEast;
                case SpriteFacing8.East:
                    return east;
                case SpriteFacing8.SouthEast:
                    return southEast;
                case SpriteFacing8.South:
                    return south;
                case SpriteFacing8.SouthWest:
                    if (HasFrames(southWest))
                        return southWest;
                    flipX = mirrorMissingWest;
                    return southEast;
                case SpriteFacing8.West:
                    if (HasFrames(west))
                        return west;
                    flipX = mirrorMissingWest;
                    return east;
                case SpriteFacing8.NorthWest:
                    if (HasFrames(northWest))
                        return northWest;
                    flipX = mirrorMissingWest;
                    return northEast;
                default:
                    return south;
            }
        }

        public int ResolvedFrameCount(SpriteFacing8 facing)
        {
            Sprite[] frames = Get(facing, out _);
            return frames != null ? frames.Length : 0;
        }

        public bool HasResolvableFrames(SpriteFacing8 facing)
        {
            Sprite[] frames = Get(facing, out _);
            return HasFrames(frames);
        }

        public static bool HasFrames(Sprite[] frames)
        {
            return frames != null && frames.Length > 0;
        }
    }

    [Serializable]
    public sealed class FrameAnimationClip8
    {
        public CharacterFrameState state;

        [Min(1f)]
        public float framesPerSecond = 8f;

        public bool loop = true;

        [Tooltip("Frame that emits Impact on attacks/casts. Use -1 for none.")]
        public int impactFrame = -1;

        public DirectionalFrameStrip8 frames = new DirectionalFrameStrip8();

        public int GetFrameCount(SpriteFacing8 facing)
        {
            return frames != null ? frames.ResolvedFrameCount(facing) : 0;
        }

        public Sprite GetFrame(SpriteFacing8 facing, int frameIndex, out bool flipX)
        {
            flipX = false;
            if (frames == null)
                return null;

            Sprite[] strip = frames.Get(facing, out flipX);
            if (strip == null || strip.Length == 0)
                return null;

            int index = Mathf.Clamp(frameIndex, 0, strip.Length - 1);
            return strip[index];
        }
    }

    [CreateAssetMenu(
        menuName = "Keeper First Covenant/Visual/Frame Animation Library",
        fileName = "FrameAnimationLibrary")]
    public sealed class FrameAnimationLibrary : ScriptableObject
    {
        public string libraryId;
        public string displayName;
        public FrameAnimationClip8[] clips;

        public FrameAnimationClip8 Find(CharacterFrameState state)
        {
            if (clips == null)
                return null;

            for (int i = 0; i < clips.Length; i++)
            {
                FrameAnimationClip8 clip = clips[i];
                if (clip != null && clip.state == state)
                    return clip;
            }

            return null;
        }
    }

    [CreateAssetMenu(
        menuName = "Keeper First Covenant/Visual/Frame Equipment Layer",
        fileName = "FrameEquipmentLayer")]
    public sealed class FrameEquipmentLayerDefinition : ScriptableObject
    {
        public string visualId;
        public string displayName;
        public VisualEquipmentSlot slot;
        public bool hideBaseWeapon;
        public FrameAnimationLibrary animationLibrary;
    }

    public static class SpriteFacing8Utility
    {
        public static SpriteFacing8 FromWorldDirection(Vector3 direction)
        {
            Vector2 planar = new Vector2(direction.x, direction.z);
            if (planar.sqrMagnitude < 0.0001f)
                return SpriteFacing8.South;

            planar.Normalize();
            float angle = Mathf.Atan2(planar.x, planar.y) * Mathf.Rad2Deg;
            if (angle < 0f)
                angle += 360f;

            int octant = Mathf.RoundToInt(angle / 45f) & 7;
            switch (octant)
            {
                case 0: return SpriteFacing8.North;
                case 1: return SpriteFacing8.NorthEast;
                case 2: return SpriteFacing8.East;
                case 3: return SpriteFacing8.SouthEast;
                case 4: return SpriteFacing8.South;
                case 5: return SpriteFacing8.SouthWest;
                case 6: return SpriteFacing8.West;
                case 7: return SpriteFacing8.NorthWest;
                default: return SpriteFacing8.South;
            }
        }
    }
}
