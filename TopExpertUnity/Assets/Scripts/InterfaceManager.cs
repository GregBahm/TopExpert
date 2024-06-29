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

        private PointerEventData pointerData;
        List<RaycastResult> hoveredElements;

        [SerializeField]
        private Transform drawPile;
        public Vector3 DrawPosition { get { return drawPile.position; } }

        [SerializeField]
        private Transform discardPile;
        public Vector3 DiscardPosition { get { return discardPile.position; } }

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
            UpdateHoveredCard();
        }


        public CardPlayability GetCardPlayability(ICard card)
        {
            BattleState state = core.Encounter.CurrentState;
            return card.GetPlayability(state);
        }

        private void UpdateHoveredCard()
        {
            if (hoveredElements.Any())
            {
                CardBehavior card = hoveredElements[0].gameObject.GetComponent<CardBehavior>();
                if (card != null && card.State == CardVisualState.InHand)
                    hand.HoveredCard = card;
            }
            else
            {
                hand.HoveredCard = null;
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
