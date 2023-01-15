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

        [SerializeField]
        private Transform effectApplyingPosition;

        [SerializeField]
        private GameObject cardDropZone;

        [SerializeField]
        private Transform drawPile;
        public Vector3 DrawPosition { get { return drawPile.position; } }

        [SerializeField]
        private Transform discardPile;
        public Vector3 DiscardPosition { get { return discardPile.position; } }

        public Vector3 EffectApplyingPosition { get { return effectApplyingPosition.position; } }

        public static InterfaceManager Instance { get; private set; }

        private void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            pointerData = new PointerEventData(EventSystem.current);
            hoveredElements = new List<RaycastResult>();
            core = GetComponent<EncounterManager>();
            for (int i = 0; i < core.Encounter.CurrentState.Hand.Count; i++)
            {
                ICard card = core.Encounter.CurrentState.Hand[i];
                CreateCardUi(card, i * .05f);
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
                    CardBehavior.StartDrag(hand.HoveredCard);
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
            if(hand.SelectedCard != null && Input.GetMouseButtonUp(0))
            {
                bool potentialCardPlay = GetIfCardIsOverDropZone();
                if (potentialCardPlay)
                {
                    ICard card = hand.SelectedCard.Model;
                    CardPlayability playability = GetCardPlayability(card);
                    if (playability.IsPlayable)
                    {
                        if (playability.NeedsTarget)
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
        }

        public CardPlayability GetCardPlayability(ICard card)
        {
            BattleState state = core.Encounter.CurrentState;
            return card.GetPlayability(state);
        }

        public bool GetIfCardIsOverDropZone()
        {
            return hoveredElements.Any(item => item.gameObject == cardDropZone);
        }

        private void UpdateHoveredCard()
        {
            if (hoveredElements.Any())
            {
                CardBehavior card = hoveredElements[0].gameObject.GetComponent<CardBehavior>();
                if (card != null && card.State == CardVisualState.InHand)
                    hand.HoveredCard = card;
            }
        }

        private void CreateCardUi(ICard card, float drawDelay = 0)
        {
            GameObject cardObj = Instantiate(cardPrefab);
            CardBehavior behavior = cardObj.GetComponent<CardBehavior>();
            behavior.Initialize(hand, card, drawDelay);
            hand.AddCard(behavior);
        }
    }
}
