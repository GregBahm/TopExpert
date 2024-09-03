using Investigation.Model;
using TMPro;
using UnityEngine;

namespace Investigation.Behaviors
{
    public class HudVisualManager : MonoBehaviour
    {

        [SerializeField]
        private TextMeshProUGUI currentActions;
        [SerializeField]
        private TextMeshProUGUI totalActions;
        [SerializeField]
        private TextMeshProUGUI currentInsights;

        public void VisualizeEncounter(EncounterState previousState, EncounterState nextState, float progression)
        {
            //TODO: Improve this logic
            if(progression > .5f)
            {
                currentActions.text = previousState.Actions.ToString();
                totalActions.text = previousState.ActionsPerTurn.ToString();
                currentInsights.text = previousState.Insights.ToString();
            }
            else
            {
                currentActions.text = nextState.Actions.ToString();
                totalActions.text = nextState.ActionsPerTurn.ToString();
                currentInsights.text = nextState.Insights.ToString();
            }
        }
    }
}