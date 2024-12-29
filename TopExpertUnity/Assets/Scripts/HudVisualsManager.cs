using Investigation.Model;
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

        public void VisualizeEncounter(EncounterState previousState, EncounterState nextState)
        {
            // TODO: Better stuff here
            currentActions.text = nextState.Actions.ToString();
            totalActions.text = nextState.ActionsPerTurn.ToString();
            currentInsights.text = nextState.Insights.ToString();
        }
    }
}