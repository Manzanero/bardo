using System;

namespace DiceRollerPro.Models
{
    public class FateDice : BaseDice
    {
        public FateDice(int count, int size, Modifiers modifiers) 
            : base(count, size, modifiers)
        {
        }

        public override bool GetValue(Random random, out int value)
        {
            value = random.Next(3) - 1; // (0..2)-1 => (-1..1)
            return false;
        }
    }
}
