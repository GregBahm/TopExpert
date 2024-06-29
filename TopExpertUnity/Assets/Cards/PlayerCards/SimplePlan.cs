using Combat.Cards;
using Combat.Model;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class SimplePlan : CardTemplate
{
    public override string Name => "Simple Plan";

    public override BattleState Apply(BattleState state)
    {
        BattleStateBuilder builder = new BattleStateBuilder(state);
        return builder.ToState();
    }

    public override CardPlayability GetPlayability(BattleState state)
    {
        return CardPlayability.AlwaysPlayable;
    }
}