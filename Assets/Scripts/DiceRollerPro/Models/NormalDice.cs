using System;

namespace DiceRollerPro.Models
{
    public class NormalDice : BaseDice
    {
        private int m_nextRollModifier;
        private bool m_additionalRoll;

        public NormalDice(int count, int size, Modifiers modifiers) 
            : base(count, size, modifiers)
        {
        }

        public override void Reset()
        {
            m_nextRollModifier = 0;
            m_additionalRoll = false;
        }

        public override bool GetValue(Random random, out int value)
        {
            value = 1 + random.Next(Size) + m_nextRollModifier;

            m_nextRollModifier = 0;

            if (Modifiers.Explode && value == Size)
            {
                if (Modifiers.CompoundExplode)
                {
                    value += 1 + random.Next(Size);
                }
                else if (!m_additionalRoll)
                {
                    if (Modifiers.PenetratingExplode)
                    {
                        m_nextRollModifier = -1;
                    }

                    m_additionalRoll = true;
                    return true;
                }
            }

            m_additionalRoll = false;
            return false;
        }
    }
}
