using System;
using System.Threading;

namespace DiceRollerPro.Models
{
    public class Group : BaseRoll
    {
        public BaseRoll[] Rolls;

        public Modifiers Modifiers;

        private static IResult[] s_values;

        public Group(BaseRoll[] rolls, Modifiers modifiers)
        {
            Rolls = rolls;
            Modifiers = modifiers;
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

                int take;
                if (Rolls.Length == 1)
                {
                    if (values[0] is CompositeResult && Modifiers != null)
                    {
                        var subResults = ((CompositeResult) values[0]).Results;
                        take = subResults.Length;
                        if (Modifiers.KeepHighest > 0)
                        {
                            Array.Sort(subResults, ResultValueComparer.InstanceDesc);
                            take = Modifiers.KeepHighest;
                        }
                        else if (Modifiers.KeepLowest > 0)
                        {
                            Array.Sort(subResults, ResultValueComparer.Instance);
                            take = Modifiers.KeepLowest;
                        }
                        else if (Modifiers.DropHighest > 0)
                        {
                            Array.Sort(subResults, ResultValueComparer.Instance);
                            take = subResults.Length - Modifiers.DropHighest;
                        }
                        else if (Modifiers.DropLowest > 0)
                        {
                            Array.Sort(subResults, ResultValueComparer.InstanceDesc);
                            take = subResults.Length - Modifiers.DropLowest;
                        }

                        for (int i = 0; i < take; i++)
                        {
                            subResults[i].Taken = true;
                        }

                        for (int i = take; i < subResults.Length; i++)
                        {
                            subResults[i].Taken = false;
                        }

                        Array.Sort(subResults, ResultIndexComparer.Instance);
                        return new CompositeResult(subResults);
                    }

                    return values[0];
                }

                take = values.Length;
                if (Modifiers.KeepHighest > 0)
                {
                    Array.Sort(values, ResultValueComparer.InstanceDesc);
                    take = Modifiers.KeepHighest;
                }
                else if (Modifiers.KeepLowest > 0)
                {
                    Array.Sort(values, ResultValueComparer.Instance);
                    take = Modifiers.KeepLowest;
                }
                else if (Modifiers.DropHighest > 0)
                {
                    Array.Sort(values, ResultValueComparer.Instance);
                    take = values.Length - Modifiers.DropHighest;
                }
                else if (Modifiers.DropLowest > 0)
                {
                    Array.Sort(values, ResultValueComparer.InstanceDesc);
                    take = values.Length - Modifiers.DropLowest;
                }

                for (int i = 0; i < take; i++)
                {
                    values[i].Taken = true;
                }

                for (int i = take; i < values.Length; i++)
                {
                    values[i].Taken = false;
                }

                Array.Sort(values, ResultIndexComparer.Instance);
                return new CompositeResult(values);
            }
            finally
            {
                s_values = values;
            }
        }
    }
}
