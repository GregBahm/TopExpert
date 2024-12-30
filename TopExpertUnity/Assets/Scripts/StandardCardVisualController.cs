using Investigation.Model;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class StandardCardVisualController : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI actionCostLabel;
    [SerializeField]
    private GameObject actionCostIcon;
    [SerializeField]
    private TextMeshProUGUI insightsCostLabel;
    [SerializeField]
    private GameObject insightsCostIcon;

    public void DrawState(CardUiState state, float progression)
    {
        StandardPlayerCard card = (StandardPlayerCard)(state.StartElementState ?? state.EndElementState);
        int actionCost = card.ActionCost;
        int insightsCost = card.InsightCost;
        actionCostLabel.text = actionCost.ToString();
        insightsCostLabel.text = insightsCost.ToString();

        actionCostLabel.gameObject.SetActive(actionCost != 0);
        actionCostIcon.gameObject.SetActive(actionCost != 0);
        insightsCostLabel.gameObject.SetActive(insightsCost != 0);
        insightsCostIcon.gameObject.SetActive(insightsCost != 0);
    }
}
