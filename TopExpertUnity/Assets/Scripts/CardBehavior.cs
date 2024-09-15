using UnityEngine;
using Investigation.Model;
using System.Runtime.InteropServices.WindowsRuntime;
using System;
using UnityEngine.UI;
using System.Reflection;

namespace Investigation.Behaviors
{
    public class CardBehavior : MonoBehaviour
    {
        [SerializeField]
        private VisualController[] visualControllers;

        private float hoveredness;

        private ElementIdentifier identifier;

        [SerializeField]
        private Button button;
        private Vector3 cardLocation;

        public bool IsHovered
        {
            get
            {
                return EncounterInteractionManager.Instance.HoveredCard == this;
            }
        }

        public void OnClick()
        {
            EncounterInteractionManager interaction = EncounterInteractionManager.Instance;
            bool isPlayable = interaction.GetIsPlayable(identifier);
            if(isPlayable)
                interaction.PlayCard(identifier);
        }

        private void Update()
        {
            float hoverTarget = IsHovered ? 1 : 0;
            hoveredness = Mathf.Lerp(hoveredness, hoverTarget, Time.deltaTime * 25);
            float scale = .5f + hoveredness * .25f;
            transform.localScale = new Vector3(scale, scale, scale);
            transform.position = cardLocation + new Vector3(0, 50 * hoveredness, 0);
            if(IsHovered)
            {
                int cards = transform.parent.childCount;
                gameObject.transform.SetSiblingIndex(cards);
            }
        }

        public void DrawState(CardUiState state, float progression)
        {
            if(state.StartLocation == CardUiLocation.Inexistant && state.EndLocation == CardUiLocation.Inexistant)
            {
                throw new System.Exception("Trying to draw a card that beings and ends inexistant");
            }
            cardLocation = GetLocation(state, progression);
            int index = GetSiblingIndex(state, progression);
            
            gameObject.transform.position = cardLocation;
            gameObject.transform.SetSiblingIndex(index);

            foreach (var visualController in visualControllers)
            {
                visualController.DrawState(state, progression);
            }
            button.enabled = state.StartLocation == CardUiLocation.Hand;
        }

        private Vector3 GetLocation(CardUiState state, float progression)
        {
            Vector3 startLocation = GetStartLocation(state);
            Vector3 endLocation = GetEndLocation(state);
            return Vector3.Lerp(startLocation, endLocation, progression);
        }

        private int GetSiblingIndex(CardUiState state, float progression)
        {
            if(progression < .5f)
            {
                return GetSiblingIndex(state.StartLocation, state.StartState, state.StartOrder);
            }
            return GetSiblingIndex(state.EndLocation, state.EndState, state.EndOrder);
        }

        private int GetSiblingIndex(CardUiLocation location, EncounterState state, int order)
        {
            int drawCount = state.DrawDeck.Count;
            int discardCount = state.DiscardDeck.Count;
            int dissolveCount = state.DissolveDeck.Count;
            int handCount = state.Hand.Count;
            switch (location)
            {
                case CardUiLocation.DrawDeck:
                    return order;
                case CardUiLocation.Discard:
                    return drawCount + order;
                case CardUiLocation.Dissolve:
                    return drawCount + discardCount + order;
                case CardUiLocation.Hand:
                    return drawCount + discardCount + dissolveCount + (handCount - order);
                case CardUiLocation.Inexistant:
                default:
                    return 0;
            }
        }

        public void Initialize(ElementIdentifier identifier)
        {
            this.identifier = identifier;
        }

        private Vector3 GetLocation(CardUiLocation location, EncounterState state, int order)
        {
            switch (location)
            {
                case CardUiLocation.Hand:
                    return GetHandPosition(state, order);
                case CardUiLocation.DrawDeck:
                    return GetDrawDeckPosition(state, order);
                case CardUiLocation.Discard:
                    return GetDiscardPosition(state, order);
                case CardUiLocation.Dissolve:
                default:
                    return GetDissovleDeckPosition(state, order);
            }
        }

        private Vector3 GetStartLocation(CardUiState state)
        {
            if(state.StartLocation == CardUiLocation.Inexistant)
            {
                return GetEndLocation(state);
            }
            return GetLocation(state.StartLocation, state.StartState, state.StartOrder);
        }

        private Vector3 GetEndLocation(CardUiState state)
        {
            if (state.EndLocation == CardUiLocation.Inexistant)
            {
                return GetStartLocation(state);
            }
            return GetLocation(state.EndLocation, state.EndState, state.EndOrder);
        }

        private Vector3 GetHandPosition(EncounterState state, int order)
        {
            EncounterVisualsManager encounterVisuals = EncounterVisualsManager.Instance;
            int cardsInHand = state.Hand.Count;
            float param = cardsInHand == 1 ? .5f : (float)order / (cardsInHand - 1);
            Vector3 leftPos = encounterVisuals.HandLeftPoint.position;
            Vector3 rightPos = encounterVisuals.HandRightPoint.position;
            float span = rightPos.x - leftPos.x;
            float maxSpan = cardsInHand * EncounterVisualsManager.Instance.MaxCardSpread;
            if(span > maxSpan)
            {
                float midPoint = (leftPos.x + rightPos.x) / 2;
                float offset = maxSpan * .5f;
                float newLeft = midPoint - offset;
                float newRight = midPoint + offset;
                leftPos = new Vector3(newLeft, leftPos.y, leftPos.z);
                rightPos = new Vector3(newRight, rightPos.y, rightPos.z);
            }
            Vector3 pos = Vector3.Lerp(rightPos, leftPos, param);
            return pos;
        }

        public Vector3 GetDrawDeckPosition(EncounterState state, int order)
        {
            float deckOffset = EncounterVisualsManager.Instance.DeckStackingOffset;
            Vector3 offset = new Vector3(-deckOffset * order, deckOffset * order, 0);
            return EncounterVisualsManager.Instance.DrawDeckPoint.position + offset;
        }

        public Vector3 GetDissovleDeckPosition(EncounterState state, int order)
        {
            return EncounterVisualsManager.Instance.DissolvePoint.position;
        }

        public Vector3 GetDiscardPosition(EncounterState state, int order)
        {
            float deckOffset = EncounterVisualsManager.Instance.DeckStackingOffset;
            Vector3 offset = new Vector3(deckOffset * order, deckOffset * order, 0);
            return EncounterVisualsManager.Instance.DiscardPoint.position + offset;
        }
    }
}