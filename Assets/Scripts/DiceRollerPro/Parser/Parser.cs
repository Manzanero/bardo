using System.Collections.Generic;
using DiceRollerPro.Models;
using UnityEngine;

namespace DiceRollerPro.Parser
{
    public class Parser
    {

        public BaseRoll Parse(string text)
        {
            int index = 0;
            return ParseExpression(text, ref index);
        }

        private BaseRoll ParseExpression(string text, ref int index, bool insideGroup = false)
        {
            Stack<BaseRoll> tokens = new Stack<BaseRoll>();
            while (index < text.Length)
            {
                ParseWhitespace(text, ref index);

                if (index >= text.Length)
                {
                    break;
                }

                char ch = text[index];

                if (ch >= '0' && ch <= '9')
                {
                    tokens.Push(ParseDice(text, ref index));
                }
                else if (ch == '+' || ch == '-' || ch == '*' || ch == '/')
                {
                    index++;
                    var expression = ParseExpression(text, ref index);
                    var sequence2 = ScriptableObject.CreateInstance<Sequence>();
                    sequence2.Rolls = new[] {tokens.Pop(), expression};
                    sequence2.Operator = GetOperator(ch);
                    tokens.Push(sequence2);
                }
                else if (ch == '(')
                {
                    index++;
                    tokens.Push(ParseExpression(text, ref index, insideGroup: true));
                }
                else if (ch == '{')
                {
                    tokens.Push(ParseGroup(text, ref index));
                }
                else if (ch == ',')
                {
                    index++;
                    break;
                }
                else if (ch == '}' || ch == ')')
                {
                    if (!insideGroup)
                    {
                        throw new SyntaxException("Unexpected group closing character '}'");
                    }
                    break;
                }
                else
                {
                    throw new SyntaxException("Unexpected character");
                }
            }

            if (tokens.Count == 1)
            {
                return tokens.Pop();
            }

            var sequence = ScriptableObject.CreateInstance<Sequence>();
            sequence.Rolls = tokens.ToArray();
            sequence.Operator = Operator.Addition;
            return sequence;
        }

        private int ParseNumber(string text, ref int index)
        {
            int number = 0;
            while (index < text.Length && char.IsDigit(text[index]))
            {
                number = (number * 10) + (text[index] - '0');
                index++;
            }

            return number;
        }

        private void ParseWhitespace(string text, ref int index)
        {
            while (index < text.Length && char.IsWhiteSpace(text[index]))
            {
                index++;
            }
        }

        private BaseRoll ParseGroup(string text, ref int index)
        {
            List<BaseRoll> tokens = new List<BaseRoll>();

            index++;

            while (index < text.Length && text[index] != '}')
            {
                ParseWhitespace(text, ref index);

                tokens.Add(ParseExpression(text, ref index, insideGroup: true));

                ParseWhitespace(text, ref index);
            }

            if (index >= text.Length || text[index] != '}')
            {
                throw new SyntaxException("Expected group closing character '}'");
            }

            index++;

            var modifiers = ParseModifiers(text, ref index);

            var group = ScriptableObject.CreateInstance<Group>();
            group.Rolls = tokens.ToArray();
            group.Modifiers = modifiers;
            return group;
        }

        private Modifiers ParseModifiers(string text, ref int index)
        {
            int dropHighest = 0, dropLowest = 0;
            int keepHighest = 0, keepLowest = 0;
            bool explode = false, compoundExplode = false, penetratingExplode = false;

            if (index < text.Length)
            {
                if (text[index] == 'd')
                {
                    index++;
                    if (index < text.Length)
                    {
                        if (text[index] == 'h')
                        {
                            index++;
                            dropHighest = ParseNumber(text, ref index);
                        }
                        else if (text[index] == 'l')
                        {
                            index++;
                            dropLowest = ParseNumber(text, ref index);
                        }
                        else if (char.IsDigit(text[index]))
                        {
                            dropLowest = ParseNumber(text, ref index);
                        }
                    }
                }
                else if (text[index] == 'k')
                {
                    index++;
                    if (index < text.Length)
                    {
                        if (text[index] == 'h')
                        {
                            index++;
                            keepHighest = ParseNumber(text, ref index);
                        }
                        else if (text[index] == 'l')
                        {
                            index++;
                            keepLowest = ParseNumber(text, ref index);
                        }
                        else if (char.IsDigit(text[index]))
                        {
                            keepHighest = ParseNumber(text, ref index);
                        }
                    }
                }
            }
            if (index < text.Length)
            {
                if (text[index] == '!')
                {
                    index++;
                    explode = true;

                    if (index < text.Length)
                    {
                        if (text[index] == '!')
                        {
                            index++;
                            compoundExplode = true;
                        }
                        else if (text[index] == 'p')
                        {
                            index++;
                            penetratingExplode = true;
                        }
                    }
                }
            }

            return new Modifiers(keepHighest, keepLowest, dropHighest, dropLowest, explode, compoundExplode, penetratingExplode);
        }

        private BaseRoll ParseDice(string text, ref int index)
        {
            int size = 0;
            bool isFate = false;

            var count = ParseNumber(text, ref index);

            if (index >= text.Length)
            {
                var number = ScriptableObject.CreateInstance<Number>();
                number.Value = count;
                return number;
            }

            if (text[index] == 'd')
            {
                index++;

                if (index >= text.Length)
                {
                    throw new SyntaxException("Expected dice size");
                }

                if (text[index] == 'F')
                {
                    index++;
                    isFate = true;
                }
                else
                {
                    size = ParseNumber(text, ref index);
                    if (size == 0)
                    {
                        throw new SyntaxException("Expected dice size");
                    }
                }

                var modifiers = ParseModifiers(text, ref index);

                if (isFate)
                {
                    var fateDice = ScriptableObject.CreateInstance<FateDice>();
                    fateDice.Count = count;
                    fateDice.Size = size;
                    fateDice.Modifiers = modifiers;
                    return fateDice;
                }

                var normalDice = ScriptableObject.CreateInstance<NormalDice>();
                normalDice.Count = count;
                normalDice.Size = size;
                normalDice.Modifiers = modifiers;
                return normalDice;

            }

            if (text[index] != '}' && text[index] != ')' &&
                text[index] != '+' &&
                text[index] != '-' &&
                text[index] != '*' &&
                text[index] != '/')
            {
                throw new SyntaxException("Unexpected character");
            }

            var number2 = ScriptableObject.CreateInstance<Number>();
            number2.Value = count;
            return number2;
        }

        private Operator GetOperator(char ch)
        {
            switch (ch)
            {
                case '+':
                    return Operator.Addition;
                case '-':
                    return Operator.Subtraction;
                case '*':
                    return Operator.Multiplication;
                case '/':
                    return Operator.Division;
            }
            return Operator.Addition;
        }
    }
}
