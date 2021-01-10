using System.Text;
using System.Threading;
using DiceRollerPro.Models;

namespace DiceRollerPro
{
    public static class RollExplainer
    {
        private static StringBuilder s_reusedBuilder;

        public static string Explain(IResult result)
        {
            var compositeResult = result as CompositeResult;
            if (compositeResult != null)
            {
                // Reuse StringBuilder to avoid allocations
                StringBuilder builder = Interlocked.Exchange(ref s_reusedBuilder, null);
                if (builder == null)
                {
                    builder = new StringBuilder();
                }
                else
                {
                    builder.Clear();
                }

                builder.Append("(");
                for (int i = 0; i < compositeResult.Results.Length; i++)
                {
                    var r = compositeResult.Results[i];
                    if (i != 0)
                    {
                        builder.Append(AccumulatorExplanation(compositeResult.Operator));
                    }
                    builder.Append(Explain(r));
                    if (r is CompositeResult)
                    {
                        builder.Append(r.Taken ? "" : "D");
                    }
                }
                builder.Append(")");

                string explanation = builder.ToString();
                s_reusedBuilder = builder;
                return explanation;
            }
            else
            {
                return string.Format("{0}{1}", result.Value, result.Taken ? "K" : "D");
            }
        }

        private static string AccumulatorExplanation(Operator @operator)
        {
            switch (@operator)
            {
                case Operator.Addition:
                    return " + ";
                case Operator.Subtraction:
                    return " - ";
                case Operator.Multiplication:
                    return " * ";
                case Operator.Division:
                    return " / ";
            }

            return " + ";
        }
    }
}