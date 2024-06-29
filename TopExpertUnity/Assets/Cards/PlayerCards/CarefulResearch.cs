using Combat.Cards;
using Combat.Model;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class CarefulResearch : CardTemplate
{
    public override string Name => "Careful Research";

    public int ResearchPower { get; } = 3;

    public override BattleState Apply(BattleState state)
    {
        BattleStateBuilder builder = new BattleStateBuilder(state);
        builder.RemainingResearchPoints -= ResearchPower;
        return builder.ToState();
    }

    public override CardPlayability GetPlayability(BattleState state)
    {
        return CardPlayability.AlwaysPlayable;
    }
}