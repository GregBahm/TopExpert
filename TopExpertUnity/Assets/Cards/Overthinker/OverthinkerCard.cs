using System.Linq;

namespace Investigation.Model
{
    public record OverthinkerCard(ElementIdentifier Identifier) 
        : StandardPlayerCard(Identifier)
    {
        public override int ActionCost => 0;

        protected override EncounterState GetModifiedState(EncounterState state)
        {
            int count = 0;
            foreach (var card in state.Hand
                .Concat(state.DrawDeck)
                .Concat(state.DiscardDeck))
            {
                if (card.GetType() == typeof(OverthinkerCard))
                {
                    count++;
                }
            }
            return state with { Insights = state.Insights + count };
        }
    }
}