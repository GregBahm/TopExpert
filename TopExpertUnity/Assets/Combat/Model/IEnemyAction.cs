namespace Combat.Model
{
    public interface IEnemyAction
    {
        BattleState TakeAction(BattleState state);
    }
}