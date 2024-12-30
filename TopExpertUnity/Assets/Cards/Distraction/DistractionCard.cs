using System.Collections.Generic;
using System.Linq;

namespace Investigation.Model
{
    public record DistractionCard(ElementIdentifier Identifier)
        : StandardPlayerCard(Identifier)
    {
        public override int ActionCost => 3;
        public override int InsightCost => 5;

        protected override EncounterState GetModifiedState(EncounterState state)
        {
            List<PersistantEffector> unappliedEffectors = state.UnappliedEffectors.ToList();
            List<PersistantEffector> appliedEffectors = state.AppliedEffectors.ToList();

            foreach (var item in state.UnappliedEffectors.Where(item => item.IsEnemyEffect))
            {
                unappliedEffectors.Remove(item);
                appliedEffectors.Add(item);
            }
            return state with { UnappliedEffectors = unappliedEffectors, AppliedEffectors = appliedEffectors };
        }
    }
}