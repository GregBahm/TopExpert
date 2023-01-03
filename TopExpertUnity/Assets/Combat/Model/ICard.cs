namespace Combat.Model
{
    public interface ICard
    {
        string Name { get; }
        bool Consumeable { get; }
        bool Holds { get; }
        bool Exhausts { get; }
        CardPlayability GetPlayability(BattleState state);
        BattleState Apply(BattleState state, EntityId potentialTarget);
    }
}