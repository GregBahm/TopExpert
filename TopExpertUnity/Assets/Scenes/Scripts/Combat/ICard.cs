namespace TopExpert.Combat
{
    public interface ICard
    {
        string Name { get; }
        bool Consumeable { get; }
        bool Hold { get; }
        CardPlayability GetPlayability(BattleState state);
        BattleState Apply(BattleState state, EntityId potentialTarget);
    }
}