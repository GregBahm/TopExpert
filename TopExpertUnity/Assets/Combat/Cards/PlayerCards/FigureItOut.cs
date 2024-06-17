using Combat.Cards;
using Combat.Model;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class FigureItOut : CardTemplate
{
    public override string Name => "Figure It Out";

    public int ResearchPower { get; } = 3;

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