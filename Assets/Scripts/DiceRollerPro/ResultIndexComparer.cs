using System.Collections.Generic;

namespace DiceRollerPro
{
    public class ResultIndexComparer : IComparer<IResult>
    {
        public static ResultIndexComparer Instance = new ResultIndexComparer(false);
        public static ResultIndexComparer InstanceDesc = new ResultIndexComparer(true);

        private readonly bool m_descending;

        private ResultIndexComparer(bool descending)
        {
            m_descending = descending;
        }

        public int Compare(IResult x, IResult y)
        {
            if (m_descending)
            {
                return -x.Index.CompareTo(y.Index);
            }
            return x.Index.CompareTo(y.Index);
        }
    }
}