namespace DiceRollerPro
{
    public interface IResult
    {
        int Value { get; }
        int Index { get; set; }
        bool Taken { get; set; }
    }
}