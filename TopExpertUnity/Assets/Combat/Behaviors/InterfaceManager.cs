using Combat.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Combat.Behaviors
{
    [RequireComponent(typeof(EncounterManager))]
    public class InterfaceManager : MonoBehaviour
    {
        private EncounterManager core;

        [SerializeField]
        private GameObject cardPrefab;

        [SerializeField]
        private HandBehavior hand;

        [SerializeField]
        private TextMeshProUGUI energyLabel;

        private PointerEventData pointerData;
        List<RaycastResult> hoveredElements;

        private Vector3 mouseDragStartPos;
        private Vector3 cardDragStartPos;

        private void Start()
        {
            pointerData = new PointerEventData(EventSystem.current);
            hoveredElements = new List<RaycastResult>();
            core = GetComponent<EncounterManager>();
            foreach (ICard item in core.Encounter.CurrentState.Hand)
            {
                CreateCardUi(item);
            }
        }

        private void Update()
        {
            pointerData.position = Input.mousePosition;
            EventSystem.current.RaycastAll(pointerData, hoveredElements);
            energyLabel.text = GetEnergyLabel();
            PlaceCards();
        }

        private string GetEnergyLabel()
        {
            int actionPoints = core.Encounter.CurrentState.RemainingActionPoints;
            int maxActionPoints = core.Encounter.CurrentState.MaxActionPoints;
            return "Energy\n" + actionPoints + " / " + maxActionPoints;
        }

        private void PlaceCards()
        {
            HandlePotentialCardPlay();
            if (Input.GetMouseButton(0))
            {
                if (hand.HoveredCard != null && hand.SelectedCard == null)
                {
                    hand.SelectedCard = hand.HoveredCard;
                    mouseDragStartPos = Input.mousePosition;
                    cardDragStartPos = hand.HoveredCard.transform.position;
                }
                else if (hand.SelectedCard != null)
                {
                    Vector3 delta = Input.mousePosition - mouseDragStartPos;
                    hand.SelectedCard.transform.position = cardDragStartPos + delta;
                }
            }
            else
            {
                hand.SelectedCard = null;
                hand.HoveredCard = null;
                UpdateHoveredCard();
            }
        }

        private void HandlePotentialCardPlay()
        {
            bool potentialCardPlay = GetIfDroppedCard();
            if (potentialCardPlay)
            {
                ICard card = hand.SelectedCard.Model;
                CardPlayability playability = GetCardPlayability(card);
                if(playability.IsPlayable)
                {
                    if(playability.NeedsTarget)
                    {
                        // Did they drop the card on a valid target?
                        Debug.Log("Need target for " + hand.SelectedCard.Model.Name);
                    }
                    else
                    {
                        // Play the card
                        hand.SelectedCard.PlayCard();
                    }
                }
            }
        }

        private CardPlayability GetCardPlayability(ICard card)
        {
            BattleState state = core.Encounter.CurrentState;
            return card.GetPlayability(state);
        }

        private bool GetIfDroppedCard()
        {
            if (Input.GetMouseButtonUp(0) && hand.SelectedCard != null)
            {
                float yDelta = Input.mousePosition.y - mouseDragStartPos.y;
                if(yDelta > 200)
                {
                    return true;
                }
            }
            return false;
        }

        private void UpdateHoveredCard()
        {
            if (hoveredElements.Any())
            {
                CardBehavior card = hoveredElements[0].gameObject.GetComponent<CardBehavior>();
                if (card != null && card.State == CardVisualState.Available)
                    hand.HoveredCard = card;
            }
        }

        private void CreateCardUi(ICard card)
        {
            GameObject cardObj = Instantiate(cardPrefab);
            CardBehavior behavior = cardObj.GetComponent<CardBehavior>();
            behavior.Initialize(hand, card);
            hand.AddCard(behavior);
        }
    }
}
