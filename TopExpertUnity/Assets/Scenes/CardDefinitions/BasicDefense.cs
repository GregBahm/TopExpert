namespace TopExpert.Combat
{
    public class BasicDefense : ICard
    {
        public string Name => "Basic Defense";

        public int ActionPointCost => 1;
        public int DefensePower => 5;

        public bool Consumeable => false;

        public bool Hold => false;

        public CardPlayability GetPlayability(BattleState state)
        {
            CardPlayability ret = new CardPlayability();
            ret.IsPlayable = state.RemainingActionPoints >= ActionPointCost;
            return ret;
        }

        public BattleState Apply(BattleState state, EntityId target)
        {
            BattleStateBuilder builder = new BattleStateBuilder(state);
            builder.Investigators.Shield += DefensePower;
            return builder.ToState();
        }
    }
}