using Combat.Cards;
using Combat.Model;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Precaution : CardTemplate
{
    public override string Name => "Precaution";

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