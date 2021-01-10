using System;
using System.Collections.Generic;
using System.Threading;

namespace DiceRollerPro.Models
{
    public abstract class BaseDice : BaseRoll
    {
        public int Count;
        public int Size;
        public Modifiers Modifiers;


        private static List<IResult> s_values;

        protected BaseDice(int count, int size, Modifiers modifiers)
        {
            Count = count;
            Size = size;
            Modifiers = modifiers;
        }

        public override IResult GenerateValue(Random random)
        {
            Reset();

            var values = Interlocked.Exchange(ref s_values, null);
            if (values == null)
            {
                values = new List<IResult>(Count*2);
            }
            else
            {
                values.Clear();
            }

            try
            {
                for (int i = 0; i < Count; i++)
                {
                    var hasAnotherRoll = true;

                    while (hasAnotherRoll)
                    {
                        hasAnotherRoll = GetValue(random, out int value);

                        values.Add(new Result(value, i, false));
                    }
                }

                var results = values.ToArray();

                int take = results.Length;
                if (Modifiers != null)
                {
                    if (Modifiers.KeepHighest > 0)
                    {
                        Array.Sort(results, ResultValueComparer.InstanceDesc);
                        take = Modifiers.KeepHighest;
                    }
                    else if (Modifiers.KeepLowest > 0)
                    {
                        Array.Sort(results, ResultValueComparer.Instance);
                        take = Modifiers.KeepLowest;
                    }
                    else if (Modifiers.DropHighest > 0)
                    {
                        Array.Sort(results, ResultValueComparer.Instance);
                        take = take - Modifiers.DropHighest;
                    }
                    else if (Modifiers.DropLowest > 0)
                    {
                        Array.Sort(results, ResultValueComparer.InstanceDesc);
                        take = take - Modifiers.DropLowest;
                    }
                }

                for (int i = 0; i < take; i++)
                {
                    results[i].Taken = true;
                }

                Array.Sort(results, ResultIndexComparer.Instance);
                return new CompositeResult(results);
            }
            finally
            {
                s_values = values;
            }
        }

        public virtual void Reset() { }

        public abstract bool GetValue(Random random, out int value);
    }
}
