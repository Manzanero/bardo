using System;

namespace DiceRollerPro.Models
{
    public class Number : BaseRoll
    {
        public int Value;

        public Number(int value)
        {
            Value = value;
        }

        public override IResult GenerateValue(Random random)
        {
            return new Result(Value, 0, true);
        }
    }
}
