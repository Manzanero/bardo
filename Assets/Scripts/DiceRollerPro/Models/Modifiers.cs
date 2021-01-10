using System;

namespace DiceRollerPro.Models
{
    [Serializable]
    public class Modifiers
    {
        public int KeepHighest;
        public int KeepLowest;
        public int DropHighest;
        public int DropLowest;
        public bool Explode;
        public bool CompoundExplode;
        public bool PenetratingExplode;

        public Modifiers()
        {
        }

        public Modifiers(int keepHighest, int keepLowest, int dropHighest, int dropLowest, bool explode, bool compoundExplode, bool penetratingExplode)
        {
            KeepHighest = keepHighest;
            KeepLowest = keepLowest;
            DropHighest = dropHighest;
            DropLowest = dropLowest;
            Explode = explode;
            CompoundExplode = compoundExplode;
            PenetratingExplode = penetratingExplode;
        }
    }
}
