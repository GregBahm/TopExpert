using Investigation.Model;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class StandardCardVisualController : VisualController
{
    [SerializeField]
    private TextMeshProUGUI actionCost;

    public override void DrawState(CardUiState state, float progression)
    {
        StandardPlayerCard card = (StandardPlayerCard)(state.StartCardState?? state.EndCardState);
        actionCost.text = card.ActionCost.ToString();
    }
}
