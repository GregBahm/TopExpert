using System.Collections.Generic;
using System.Linq;

namespace Combat.Model
{
    public class Encounter
    {
        private readonly List<BattleState> battleProgression;
        public BattleState CurrentState { get { return battleProgression.Last(); } }

        public Encounter(BattleState initialState)
        {
            battleProgression = new List<BattleState>() { initialState };
        }

        public void PlayCard(ICard card, EntityId potentialTarget)
        {
            BattleState nextState = CurrentState.PlayCard(card, potentialTarget);
            battleProgression.Add(nextState);
        }

        public void EndRound()
        {
            BattleState nextState = CurrentState.EndRound();
            battleProgression.Add(nextState);
        }
    }
}