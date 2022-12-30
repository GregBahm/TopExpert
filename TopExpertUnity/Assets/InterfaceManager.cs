using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.EventSystems;

namespace TopExpert.Combat
{
    [RequireComponent(typeof(EncounterManager))]
    public class InterfaceManager : MonoBehaviour
    {
        private EncounterManager core;

        [SerializeField]
        private GameObject cardPrefab;

        [SerializeField]
        private HandViewModel hand;

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
            if(Input.GetMouseButton(0))
            {
                if(hand.HoveredCard != null && hand.SelectedCard == null)
                {
                    hand.SelectedCard = hand.HoveredCard;
                    mouseDragStartPos = Input.mousePosition;
                    cardDragStartPos = hand.HoveredCard.transform.position;
                }
                else if(hand.SelectedCard != null)
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

        private void UpdateHoveredCard()
        {
            if (hoveredElements.Any())
            {
                hand.HoveredCard = hoveredElements[0].gameObject.GetComponent<CardViewModel>();
            }
        }

        private void CreateCardUi(ICard card)
        {
            GameObject cardObj = Instantiate(cardPrefab);
            CardViewModel viewModel = cardObj.GetComponent<CardViewModel>();
            viewModel.Initialize(hand, card);
            hand.AddCard(viewModel);
        }
    }
}
