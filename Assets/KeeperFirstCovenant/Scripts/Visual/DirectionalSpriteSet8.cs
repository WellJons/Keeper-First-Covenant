using System;
using UnityEngine;

namespace KeeperFirstCovenant.Visual
{
    public enum FacingDirection8
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

    [Serializable]
    public sealed class DirectionalSpriteSet8
    {
        [Header("Unique directions")]
        public Sprite north;
        public Sprite northEast;
        public Sprite east;
        public Sprite southEast;
        public Sprite south;

        [Header("Optional unique mirrored directions")]
        public Sprite southWest;
        public Sprite west;
        public Sprite northWest;

        [Tooltip("When a west-facing sprite is missing, mirror its east-facing partner.")]
        public bool mirrorMissingWestDirections = true;

        public Sprite Get(FacingDirection8 direction, out bool flipX)
        {
            flipX = false;

            switch (direction)
            {
                case FacingDirection8.North:
                    return north;
                case FacingDirection8.NorthEast:
                    return northEast;
                case FacingDirection8.East:
                    return east;
                case FacingDirection8.SouthEast:
                    return southEast;
                case FacingDirection8.South:
                    return south;
                case FacingDirection8.SouthWest:
                    if (southWest != null)
                        return southWest;
                    flipX = mirrorMissingWestDirections;
                    return southEast;
                case FacingDirection8.West:
                    if (west != null)
                        return west;
                    flipX = mirrorMissingWestDirections;
                    return east;
                case FacingDirection8.NorthWest:
                    if (northWest != null)
                        return northWest;
                    flipX = mirrorMissingWestDirections;
                    return northEast;
                default:
                    return south;
            }
        }

        public static FacingDirection8 FromWorldDirection(Vector3 direction)
        {
            Vector2 planar = new Vector2(direction.x, direction.z);
            if (planar.sqrMagnitude < 0.0001f)
                return FacingDirection8.South;

            planar.Normalize();
            float angle = Mathf.Atan2(planar.x, planar.y) * Mathf.Rad2Deg;
            if (angle < 0f)
                angle += 360f;

            int octant = Mathf.RoundToInt(angle / 45f) & 7;
            switch (octant)
            {
                case 0: return FacingDirection8.North;
                case 1: return FacingDirection8.NorthEast;
                case 2: return FacingDirection8.East;
                case 3: return FacingDirection8.SouthEast;
                case 4: return FacingDirection8.South;
                case 5: return FacingDirection8.SouthWest;
                case 6: return FacingDirection8.West;
                case 7: return FacingDirection8.NorthWest;
                default: return FacingDirection8.South;
            }
        }
    }
}
