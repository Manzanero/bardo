namespace DiceRollerPro
{
    public struct Result : IResult
    {
        public int Value { get; }
        public int Index { get; set; }
        public bool Taken { get; set; }

        public Result(int value, int index, bool taken)
        {
            Value = value;
            Index = index;
            Taken = taken;
        }
    }
}
