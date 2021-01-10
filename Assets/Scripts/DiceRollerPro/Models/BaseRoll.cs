using UnityEngine;
using Random = System.Random;

namespace DiceRollerPro.Models
{
    public abstract class BaseRoll : ScriptableObject
    {
        public abstract IResult GenerateValue(Random random);
    }

}
