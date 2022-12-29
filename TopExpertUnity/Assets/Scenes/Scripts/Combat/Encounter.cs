using System.Collections.Generic;
using System.Linq;

namespace TopExpert.Combat
{
    public class Encounter
    {
        private readonly List<BattleState> battleProgression;
        public BattleState CurrentState { get { return battleProgression.Last(); } }

        public Encounter(BattleState initialState)
        {
            battleProgression = new List<BattleState>() { initialState };
        }
    }
}