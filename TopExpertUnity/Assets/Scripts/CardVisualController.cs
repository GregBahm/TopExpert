using System;
using TMPro;
using UnityEngine;

namespace Investigation.Behaviors
{
    public class CardVisualController : MonoBehaviour
    {
        [SerializeField]
        private GameObject cardBack;

        [SerializeField]
        private GameObject cardFront;

        public void DrawState(CardUiState state, float progression)
        {
            bool cardIsUp = GetCardIsUp(state, progression);
            cardBack.SetActive(!cardIsUp);
            cardFront.SetActive(cardIsUp);
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