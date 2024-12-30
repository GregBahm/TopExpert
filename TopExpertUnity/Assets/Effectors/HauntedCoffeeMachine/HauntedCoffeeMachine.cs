namespace Investigation.Model
{
    public record HauntedCoffeeMachine(ElementIdentifier Identifier) 
        : PersistantEffector(Identifier)
    {
        public override bool IsEnemyEffect => true;
        public int TurnsBetweenEffect => 1;
        public int DangerGrowth => 1;

        public int Danger { get; init; } = 0;
        public int TurnsTillEffect { get; init; } = 1;

        protected override EncounterState GetEffectedState(EncounterState state)
        {
            if(TurnsTillEffect == 0)
            {
                return state with { Advantage = state.Advantage - Danger };
            }
            return state;
        }

        protected override PersistantEffector GetSelfAfterEffect(EncounterState state)
        {
            int turnsTillEffect = TurnsBetweenEffect;

            turnsTillEffect--;
            if (turnsTillEffect < 0)
            {
                turnsTillEffect = TurnsBetweenEffect;
            }

            int newDanger = Danger + DangerGrowth;
            return this with { TurnsTillEffect = turnsTillEffect, Danger = newDanger };
        }
    }
}