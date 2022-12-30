namespace TopExpert.Combat
{
    public class BasicResearch : ICard
    {
        public string Name => "Basic Research";

        public int ActionPointCost => 1;
        public int ResearchPower => 5;

        public bool Consumeable => false;

        public bool Hold => false;

        public CardPlayability GetPlayability(BattleState state)
        {
            CardPlayability ret = new CardPlayability();
            ret.IsPlayable = state.RemainingActionPoints >= ActionPointCost;
            return ret;
        }

        public BattleState Apply(BattleState state, EntityId potentialTarget)
        {
            BattleStateBuilder builder = new BattleStateBuilder(state);
            builder.RemainingResearchPoints -= ResearchPower;
            return builder.ToState();
        }
    }
}