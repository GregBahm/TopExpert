﻿namespace Investigation.Model
{
    public record InvestigateCard(CardIdentifier Identifier) 
        : StandardPlayerCard(Identifier)
    {
        public override int ActionCost => 1;

        protected override EncounterState GetModifiedState(EncounterState state)
        {
            return state with { Insights = state.Insights + 1 };
        }
    }
}