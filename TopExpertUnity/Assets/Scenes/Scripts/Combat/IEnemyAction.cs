namespace TopExpert.Combat
{
    public interface IEnemyAction
    {
        BattleState TakeAction(BattleState state);
    }
}