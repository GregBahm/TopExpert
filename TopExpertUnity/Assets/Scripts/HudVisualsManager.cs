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

        [SerializeField]
        private TextMeshProUGUI advantageToWin;
        [SerializeField]
        private TextMeshProUGUI advantageToLose;

        [SerializeField]
        private AdvantageMeterController advantageMeterController;

        [SerializeField]
        private CanvasGroup loseScreen;
        [SerializeField]
        private CanvasGroup winScreen;

        public void VisualizeEncounter(EncounterState previousState, EncounterState nextState)
        {
            // TODO: Better stuff here
            currentActions.text = nextState.Actions.ToString();
            totalActions.text = nextState.ActionsPerTurn.ToString();
            currentInsights.text = GetCurrentInsightsText(nextState);
            advantage.text = "Advantage: " + nextState.Advantage.ToString();
            advantageToWin.text = nextState.AdvantageToWin.ToString();
            advantageToLose.text = nextState.DangerToLose.ToString();
            advantageMeterController.SetVisual(previousState, nextState);

            UpdateWinLoseScreens(previousState, nextState);
        }

        private void UpdateWinLoseScreens(EncounterState previousState, EncounterState nextState)
        {
            float previousWin = previousState.Status == EncounterStatus.PlayersWon ? 1 : 0;
            float previousLoss = previousState.Status == EncounterStatus.PlayersLost ? 1 : 0;
            float nextWin = nextState.Status == EncounterStatus.PlayersWon ? 1 : 0;
            float nextLoss = nextState.Status == EncounterStatus.PlayersLost ? 1 : 0;

            float win = Mathf.Lerp(previousWin, nextWin, EncounterVisualsManager.Instance.SubTurnDisplay);
            float loss = Mathf.Lerp(previousLoss, nextLoss, EncounterVisualsManager.Instance.SubTurnDisplay);

            winScreen.alpha = win;
            loseScreen.alpha = loss;
            winScreen.gameObject.SetActive(win > 0);
            loseScreen.gameObject.SetActive(loss > 0);
        }

        private string GetCurrentInsightsText(EncounterState nextState)
        {
            return nextState.Insights.ToString();
        }
    }
}