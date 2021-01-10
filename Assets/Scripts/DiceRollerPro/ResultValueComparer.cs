using System.Collections.Generic;

namespace DiceRollerPro
{
    public class ResultValueComparer : IComparer<IResult>
    {
        public static ResultValueComparer Instance = new ResultValueComparer(false);
        public static ResultValueComparer InstanceDesc = new ResultValueComparer(true);

        private readonly bool m_descending;

        private ResultValueComparer(bool descending)
        {
            m_descending = descending;
        }

        public int Compare(IResult x, IResult y)
        {
            if (m_descending)
            {
                return -x.Value.CompareTo(y.Value);
            }
            return x.Value.CompareTo(y.Value);
        }
    }
}