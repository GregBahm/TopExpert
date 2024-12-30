using Investigation.Model;
using System;
using TMPro;
using UnityEngine;

namespace Investigation.Behaviors
{
    public class HudVisualsManager : MonoBehaviour
    {
        [SerializeField]
        private TextMeshProUGUI currentActions;
        [SerializeField]
        private TextMeshProUGUI totalActions;
        [SerializeField]
        private TextMeshProUGUI currentInsights;
        [SerializeField]
        private TextMeshProUGUI advantage;

        public void VisualizeEncounter(EncounterState previousState, EncounterState nextState)
        {
            // TODO: Better stuff here
            currentActions.text = nextState.Actions.ToString();
            totalActions.text = nextState.ActionsPerTurn.ToString();
            currentInsights.text = GetCurrentInsightsText(nextState);
            advantage.text = "Advantage: " + nextState.Advantage.ToString();
        }

        private string GetCurrentInsightsText(EncounterState nextState)
        {
            return nextState.Insights.ToString();
        }
    }
}