using System;
using TMPro;
using UnityEngine;

namespace Investigation.Behaviors
{
    public class CardVisualController : VisualController
    {
        [SerializeField]
        private GameObject cardBack;

        [SerializeField]
        private GameObject cardFront;

        [SerializeField]
        private CanvasGroup fullCard;

        public override void DrawState(CardUiState state, float progression)
        {
            bool cardIsUp = GetCardIsUp(state, progression);
            fullCard.alpha = GetCardVisiblity(state, progression);
            cardBack.SetActive(!cardIsUp);
            cardFront.SetActive(cardIsUp);
        }

        private float GetCardVisiblity(CardUiState state, float progression)
        {
            float startAlpha = GetVisibility(state.StartLocation);
            float endAlpha = GetVisibility(state.EndLocation);
            return Mathf.Lerp(startAlpha, endAlpha, progression);
        }

        private float GetVisibility(CardUiLocation location)
        {
            return (location == CardUiLocation.Inexistant || location == CardUiLocation.Dissolve) ? 0 : 1;
        }

        private bool GetCardIsUp(CardUiState state, float progression)
        {
            if(progression > 0)
            {
                return state.EndLocation != CardUiLocation.DrawDeck;
            }
            return state.StartLocation != CardUiLocation.DrawDeck;
        }
    }
}