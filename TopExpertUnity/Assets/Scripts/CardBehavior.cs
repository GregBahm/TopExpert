using UnityEngine;
using Investigation.Model;
using System.Runtime.InteropServices.WindowsRuntime;

namespace Investigation.Behaviors
{
    public class CardBehavior : MonoBehaviour
    {
        [SerializeField]
        private CardVisualController visualController;

        public void DrawState(CardUiState state, float progression)
        {
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

        private Vector3 GetStartLocation(CardUiState state)
        {
            switch (state.StartLocation)
            {
                case CardUiLocation.Hand:
                    return GetHandPosition(state.StartState, state.StartOrder);
                case CardUiLocation.DrawDeck:
                    return GetDrawDeckPosition();
                case CardUiLocation.Discard:
                    return GetDiscardPosition();
                case CardUiLocation.Dissolve:
                    return GetDissovleDeckPosition();
                case CardUiLocation.Inexistant:
                default:
                    return GetEndLocation(state);
            }
        }

        private Vector3 GetEndLocation(CardUiState state)
        {
            switch (state.EndLocation)
            {
                case CardUiLocation.Hand:
                    return GetHandPosition(state.EndState, state.EndOrder);
                case CardUiLocation.DrawDeck:
                    return GetDrawDeckPosition();
                case CardUiLocation.Discard:
                    return GetDiscardPosition();
                case CardUiLocation.Dissolve:
                    return GetDissovleDeckPosition();
                case CardUiLocation.Inexistant:
                default:
                    return GetStartLocation(state);
            }
        }

        private Vector3 GetHandPosition(EncounterState state, int order)
        {
            EncounterVisualsManager encounterVisuals = EncounterVisualsManager.Instance;
            int cardsInHand = state.Hand.Count;
            float param = cardsInHand == 1 ? .5f : (float)order / (cardsInHand - 1);
            Vector3 pos = Vector3.Lerp(encounterVisuals.HandLeftPoint.position, encounterVisuals.HandsRightPoint.position, param);
            return pos;
        }

        public Vector3 GetDrawDeckPosition()
        {
            // Later this method will need to be expanded
            return EncounterVisualsManager.Instance.DrawDeckPoint.position;
        }

        public Vector3 GetDissovleDeckPosition()
        {
            // Later this method will need to be expanded
            return EncounterVisualsManager.Instance.DissolvePoint.position;
        }

        public Vector3 GetDiscardPosition()
        {
            // Later this method will need to be expanded
            return EncounterVisualsManager.Instance.DiscardPoint.position;
        }
    }
}