using DiceRollerPro.Models;

namespace DiceRollerPro
{
    public class CompositeResult : IResult
    {
        public IResult[] Results { get; }
        public Operator Operator { get; }
        public int Index { get; set; }
        public bool Taken { get; set; }

        private int? m_cachedValue;

        public CompositeResult(IResult[] results, Operator @operator = Operator.Addition)
        {
            Results = results;
            Operator = @operator;
        }

        public int Value
        {
            get
            {
                if (m_cachedValue == null)
                {
                    var totalValue = 0;
                    int i;
                    for (i = 0; i < Results.Length; i++)
                    {
                        var result = Results[i];
                        if (!result.Taken)
                        {
                            continue;
                        }

                        totalValue = result.Value;
                        break;
                    }
                    for (i++; i < Results.Length; i++)
                    {
                        var result = Results[i];
                        if (!result.Taken)
                        {
                            continue;
                        }

                        totalValue = AccumulateResult(totalValue, result.Value);
                    }
                    m_cachedValue = totalValue;
                }
                return m_cachedValue.Value;
            }
        }

        

        private int AccumulateResult(int total, int newValue)
        {
            switch (Operator)
            {
                case Operator.Addition:
                    return total + newValue;
                case Operator.Subtraction:
                    return total - newValue;
                case Operator.Multiplication:
                    return total * newValue;
                case Operator.Division:
                    return total / newValue;
            }

            return total;
        }

        
    }
}
