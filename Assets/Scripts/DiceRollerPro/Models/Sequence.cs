using System.Collections.Generic;
using System.Threading;
using Random = System.Random;

namespace DiceRollerPro.Models
{
    public class Sequence : BaseRoll
    {
        public BaseRoll[] Rolls;

        public Operator Operator;

        private static IResult[] s_values;


        public Sequence(BaseRoll[] rolls, Operator @operator)
        {
            Rolls = rolls;
            Operator = @operator;
        }

        public override IResult GenerateValue(Random random)
        {
            var values = Interlocked.Exchange(ref s_values, null);
            if (values == null)
            {
                values = new IResult[Rolls.Length];
            }

            try
            {

                for (int i = 0; i < Rolls.Length; i++)
                {
                    BaseRoll token = Rolls[i];
                    var result = token.GenerateValue(random);
                    result.Index = i;
                    result.Taken = true;

                    values[i] = result;
                }

                return new CompositeResult(values, Operator);
            }
            finally
            {
                s_values = values;
            }
        }
    }
}
