using System.Collections.Generic;
using System.Linq;

namespace Investigation.Model
{
    public record HyperfocusCard(ElementIdentifier Identifier) 
        : StandardPlayerCard(Identifier)
    {
        public override int ActionCost => 2;
        public override bool DissolvesOnPlay => true;

        protected override EncounterState GetModifiedState(EncounterState state)
        {
            List<PersistantEffector> unappliedEffectors = state.UnappliedEffectors.ToList();
            HyperfocusEffector hyperfocus = new HyperfocusEffector(new ElementIdentifier());
            unappliedEffectors.Add(hyperfocus);
            return state with { UnappliedEffectors = unappliedEffectors, Draws = state.Draws - 1 };
        }
    }
}