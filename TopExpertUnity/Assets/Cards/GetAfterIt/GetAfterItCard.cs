﻿namespace Investigation.Model
{
    public record GetAfterItCard(ElementIdentifier Identifier) 
        : StandardPlayerCard(Identifier)
    {
        public override int ActionCost => 0;
        protected override EncounterState GetModifiedState(EncounterState state)
        {
            return state with { Actions = state.Actions + 1 };
        }
    }
}