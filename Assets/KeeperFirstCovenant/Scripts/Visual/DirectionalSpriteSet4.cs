using System;
using UnityEngine;

namespace KeeperFirstCovenant.Visual
{
    public enum FacingDirection4
    {
        SouthEast,
        SouthWest,
        NorthEast,
        NorthWest
    }

    [Serializable]
    public sealed class DirectionalSpriteSet4
    {
        [Tooltip("Unique front-facing 3/4 sprite. SouthWest can be mirrored from this when allowed.")]
        public Sprite southEast;

        [Tooltip("Optional unique SouthWest sprite. Leave empty to mirror SouthEast.")]
        public Sprite southWest;

        [Tooltip("Unique back-facing 3/4 sprite. NorthWest can be mirrored from this when allowed.")]
        public Sprite northEast;

        [Tooltip("Optional unique NorthWest sprite. Leave empty to mirror NorthEast.")]
        public Sprite northWest;

        public bool mirrorMissingDirections = true;

        public Sprite Get(FacingDirection4 direction, out bool flipX)
        {
            flipX = false;

            switch (direction)
            {
                case FacingDirection4.SouthEast:
                    return southEast;

                case FacingDirection4.SouthWest:
                    if (southWest != null)
                        return southWest;
                    flipX = mirrorMissingDirections;
                    return southEast;

                case FacingDirection4.NorthEast:
                    return northEast;

                case FacingDirection4.NorthWest:
                    if (northWest != null)
                        return northWest;
                    flipX = mirrorMissingDirections;
                    return northEast;

                default:
                    return southEast;
            }
        }
    }
}
