using UnityEngine;
using Investigation.Model;
using System.Runtime.InteropServices.WindowsRuntime;
using System;
using UnityEngine.UI;
using System.Reflection;
using Unity.VisualScripting;

namespace Investigation.Behaviors
{
    public class CardVisualController : ElementVisualController<CardUiState>
    {
        [SerializeField]
        private GameObject cardBack;

        [SerializeField]
        private GameObject cardFront;

        [SerializeField]
        private CanvasGroup fullCard;

        private float hoveredness;

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

        [SerializeField]
        private RawImage fader;

        private void Start()
        {
            fullCard.alpha = 0;
        }

        private void Update()
        {

            cardLocation = GetLocation();
            int index = GetSiblingIndex();

            gameObject.transform.position = cardLocation;
            gameObject.transform.SetSiblingIndex(index);

            button.enabled = state.StartLocation == CardExistenceLocation.Hand;

            float hoverTarget = IsHovered ? 1 : 0;
            hoveredness = Mathf.Lerp(hoveredness, hoverTarget, Time.deltaTime * 25);
            transform.localScale = GetCardScale();
            transform.position = cardLocation + new Vector3(0, Mothership.CardHoveredOffset * hoveredness, 0);
            transform.localRotation = GetCardRotation();
            if (IsHovered)
            {
                int cards = transform.parent.childCount;
                gameObject.transform.SetSiblingIndex(cards);
            }

            bool cardIsUp = GetCardIsUp();
            fullCard.alpha = GetCardVisiblity();
            cardBack.SetActive(!cardIsUp);
            cardFront.SetActive(cardIsUp);
            fader.color = GetFaderColor();
        }

        private Color GetFaderColor()
        {
            Color startColor = GetFaderColor(state.StartLocation);
            Color endColor = GetFaderColor(state.EndLocation);
            Color color = Color.Lerp(startColor, endColor, Mothership.SubTurnDisplay);
            return Color.Lerp(color, Color.clear, hoveredness);
        }

        private Color GetFaderColor(CardExistenceLocation startLocation)
        {
            switch (startLocation)
            {
                case CardExistenceLocation.Hand:
                    return Mothership.CardHandTint;
                case CardExistenceLocation.Discard:
                    return Mothership.CardDiscardTint;
                case CardExistenceLocation.DrawDeck:
                case CardExistenceLocation.Dissolve:
                default:
                    return Color.clear;
            }
        }

        private float GetCardVisiblity()
        {
            float startAlpha = GetVisibility(state.StartLocation);
            float endAlpha = GetVisibility(state.EndLocation);
            return Mathf.Lerp(startAlpha, endAlpha, Mothership.SubTurnDisplay);
        }

        private float GetVisibility(CardExistenceLocation location)
        {
            return (location == CardExistenceLocation.Inexistant || location == CardExistenceLocation.Dissolve) ? 0 : 1;
        }

        private bool GetCardIsUp()
        {
            if (Mothership.SubTurnDisplay > 0)
            {
                return state.EndLocation != CardExistenceLocation.DrawDeck;
            }
            return state.StartLocation != CardExistenceLocation.DrawDeck;
        }

        public void OnClick()
        {
            EncounterInteractionManager interaction = EncounterInteractionManager.Instance;
            bool isPlayable = interaction.GetIsPlayable(identifier);
            if (isPlayable)
                interaction.PlayCard(identifier);
        }

        private Quaternion GetCardRotation()
        {
            Quaternion startRotation= GetCardRotation(state.StartLocation);
            Quaternion endRotation = GetCardRotation(state.EndLocation);
            return Quaternion.Lerp(startRotation, endRotation, Mothership.SubTurnDisplay);
        }

        private Quaternion GetCardRotation(CardExistenceLocation location)
        {
            if(location != CardExistenceLocation.Hand)
            {
                return Quaternion.identity;
            }

            float leftPos = Mothership.HandLeftPoint.position.x;
            float rightPos = Mothership.HandRightPoint.position.x;
            float currentPos = transform.position.x;
            float param = (currentPos - leftPos) / (rightPos - leftPos);

            param = param * 2 - 1f;
            param *= 1 - hoveredness;
            float rotation = Mothership.MaxCardHandRotation * param;
            return Quaternion.Euler(0, 0, rotation);
        }

        private Vector3 GetCardScale()
        {
            float startScale = GetCardScale(state.StartLocation);
            float endScale = GetCardScale(state.EndLocation);
            float scale = Mathf.Lerp(startScale, endScale, Mothership.SubTurnDisplay);
            scale *= 1 + hoveredness * Mothership.CardHoveredScale; 
            return new Vector3(scale, scale, scale);
        }

        private float GetCardScale(CardExistenceLocation location)
        {
            if(location == CardExistenceLocation.DrawDeck ||
                location == CardExistenceLocation.Discard)
            {
                return Mothership.CardDeckScale;
            }
            return 1;
        }

        private Vector3 GetLocation()
        {
            Vector3 startLocation = GetStartLocation(state);
            Vector3 endLocation = GetEndLocation(state);
            return Vector3.Lerp(startLocation, endLocation, Mothership.SubTurnDisplay);
        }

        private int GetSiblingIndex()
        {
            if(Mothership.SubTurnDisplay < .5f)
            {
                return GetSiblingIndex(state.StartLocation, state.StartState, state.StartOrder);
            }
            return GetSiblingIndex(state.EndLocation, state.EndState, state.EndOrder);
        }

        private int GetSiblingIndex(CardExistenceLocation location, EncounterState state, int order)
        {
            int drawCount = state.DrawDeck.Count;
            int discardCount = state.DiscardDeck.Count;
            int dissolveCount = state.DissolveDeck.Count;
            int handCount = state.Hand.Count;
            switch (location)
            {
                case CardExistenceLocation.DrawDeck:
                    return order;
                case CardExistenceLocation.Discard:
                    return drawCount + order;
                case CardExistenceLocation.Dissolve:
                    return drawCount + discardCount + order;
                case CardExistenceLocation.Hand:
                    return drawCount + discardCount + dissolveCount + (handCount - order);
                case CardExistenceLocation.Inexistant:
                default:
                    return 0;
            }
        }

        private static Vector3 GetLocation(CardExistenceLocation location, EncounterState state, int order)
        {
            switch (location)
            {
                case CardExistenceLocation.Hand:
                    return GetHandPosition(state, order);
                case CardExistenceLocation.DrawDeck:
                    return GetDrawDeckPosition(state, order);
                case CardExistenceLocation.Discard:
                    return GetDiscardPosition(state, order);
                case CardExistenceLocation.Dissolve:
                default:
                    return GetDissovleDeckPosition(state, order);
            }
        }

        private static Vector3 GetStartLocation(CardUiState state)
        {
            if(state.StartLocation == CardExistenceLocation.Inexistant)
            {
                return GetEndLocation(state);
            }
            return GetLocation(state.StartLocation, state.StartState, state.StartOrder);
        }

        private static Vector3 GetEndLocation(CardUiState state)
        {
            if (state.EndLocation == CardExistenceLocation.Inexistant)
            {
                return GetStartLocation(state);
            }
            return GetLocation(state.EndLocation, state.EndState, state.EndOrder);
        }

        private static Vector3 GetHandPosition(EncounterState state, int order)
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

        public static Vector3 GetDrawDeckPosition(EncounterState state, int order)
        {
            float deckOffset = EncounterVisualsManager.Instance.DeckStackingOffset;
            Vector3 offset = new Vector3(-deckOffset * order, deckOffset * order, 0);
            return EncounterVisualsManager.Instance.DrawDeckPoint.position + offset;
        }

        public static Vector3 GetDissovleDeckPosition(EncounterState state, int order)
        {
            return EncounterVisualsManager.Instance.DissolvePoint.position;
        }

        public static Vector3 GetDiscardPosition(EncounterState state, int order)
        {
            float deckOffset = EncounterVisualsManager.Instance.DeckStackingOffset;
            Vector3 offset = new Vector3(deckOffset * order, deckOffset * order, 0);
            return EncounterVisualsManager.Instance.DiscardPoint.position + offset;
        }
    }
}