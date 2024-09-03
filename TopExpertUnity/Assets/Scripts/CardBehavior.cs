using UnityEngine;
using Investigation.Model;
using System.Runtime.InteropServices.WindowsRuntime;

namespace Investigation.Behaviors
{
    public class CardBehavior : MonoBehaviour
    {
        [SerializeField]
        private CardVisualController visualController;

        private ElementIdentifier identifier;

        public void OnClick()
        {
            EncounterInteractionManager.Instance.PlayCard(identifier);
        }

        public void DrawState(CardUiState state, float progression)
        {
            if(state.StartLocation == CardUiLocation.Inexistant && state.EndLocation == CardUiLocation.Inexistant)
            {
                throw new System.Exception("Trying to draw a card that beings and ends inexistant");
            }
            Vector3 cardLocation = GetLocation(state, progression);
            gameObject.transform.position = cardLocation;
            visualController.DrawState(state, progression);
        }

        private Vector3 GetLocation(CardUiState state, float progression)
        {
            Vector3 startLocation = GetStartLocation(state);
            Vector3 endLocation = GetEndLocation(state);
            return Vector3.Lerp(startLocation, endLocation, progression);
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
            Vector3 pos = Vector3.Lerp(encounterVisuals.HandLeftPoint.position, encounterVisuals.HandsRightPoint.position, param);
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