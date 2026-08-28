using UnityEngine;

namespace KeeperFirstCovenant.Visual
{
    [CreateAssetMenu(
        menuName = "Keeper First Covenant/Visual/Runtime World Art",
        fileName = "RuntimeWorldArt")]
    public sealed class RuntimeWorldArtDefinition : ScriptableObject
    {
        public Sprite rockMonolith;
        public Sprite ruinedArch;
        public Sprite stoneFloor;
        public Sprite shrineAltar;

        public Sprite covenantRuneCircle;
        public Sprite rockCluster;
        public Sprite boulder;
        public Sprite campfire;

        public Sprite brazier;
        public Sprite wagon;
        public Sprite brokenWall;
        public Sprite covenantCrystal;

        public Sprite Get(int index)
        {
            switch (index)
            {
                case 0: return rockMonolith;
                case 1: return ruinedArch;
                case 2: return stoneFloor;
                case 3: return shrineAltar;
                case 4: return covenantRuneCircle;
                case 5: return rockCluster;
                case 6: return boulder;
                case 7: return campfire;
                case 8: return brazier;
                case 9: return wagon;
                case 10: return brokenWall;
                case 11: return covenantCrystal;
                default: return null;
            }
        }
    }
}
