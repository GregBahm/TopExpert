using Combat.Model;

namespace Combat.Cards
{
    public abstract class CardTemplate : ICard
    {
        public abstract string Name { get; }

        public virtual bool Consumeable => false;

        public virtual bool Holds => false;

        public virtual bool Exhausts => false;

        public abstract BattleState Apply(BattleState state);

        public abstract CardPlayability GetPlayability(BattleState state);
    }
}