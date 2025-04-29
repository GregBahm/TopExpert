using System.Collections.Generic;
using System.Linq;

namespace Investigation.Model
{
    public record HyperfocusCard() 
        : StandardPlayerCard()
    {
        public override int ActionCost => 2;
        public override bool DissolvesOnPlay => true;

        protected override EncounterState GetModifiedState(EncounterState state)
        {
            List<PersistantEffector> unappliedEffectors = state.UnappliedEffectors.ToList();
            HyperfocusEffector hyperfocus = new HyperfocusEffector();
            unappliedEffectors.Add(hyperfocus);
            return state with { UnappliedEffectors = unappliedEffectors, BaseDraws = state.BaseDraws - 1 };
        }
    }
}