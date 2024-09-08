using Investigation.Model;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DistractionCardVisualController : VisualController
{
    [SerializeField]
    private TextMeshProUGUI actionCost;

    public override void DrawState(CardUiState state, float progression)
    {
        DistractionCard card = (DistractionCard)state.StartCardState;
        actionCost.text = card.ActionCost.ToString();
    }
}
