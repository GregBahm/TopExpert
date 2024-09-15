﻿using Investigation.Model;
using UnityEngine;

namespace Investigation.Behaviors
{

    [RequireComponent(typeof(HudVisualManager))]
    public class EncounterVisualsManager : MonoBehaviour
    {
        private CardVisualsManager cardVisualsManager;
        private HudVisualManager hudVisualsManager;

        public static EncounterVisualsManager Instance { get; private set; }

        [SerializeField]
        private RectTransform handLeftPoint;
        public RectTransform HandLeftPoint => handLeftPoint;
        [SerializeField]
        private RectTransform handRightPoint;
        public RectTransform HandRightPoint => handRightPoint;
        [SerializeField]
        private RectTransform drawDeckPoint;
        public RectTransform DrawDeckPoint => drawDeckPoint;
        [SerializeField]
        private RectTransform dissolvePoint;
        public RectTransform DissolvePoint => dissolvePoint;
        [SerializeField]
        private RectTransform discardPoint;
        public RectTransform DiscardPoint => discardPoint;

        [SerializeField]
        private RectTransform cardsParent;

        [Range(0, 1)]
        [SerializeField]
        float progression;
        [SerializeField]
        private float deckStackingOfset;
        public float DeckStackingOffset => deckStackingOfset;

        [SerializeField]
        private float maxCardSpread;
        public float MaxCardSpread => maxCardSpread;

        [SerializeField]
        private float maxCardHandRotation;
        public float MaxCardHandRotation => maxCardHandRotation;

        private int turnToDisplay;
        public int TurnToDisplay => turnToDisplay;

        private float subTurnDisplay;
        public float SubTurnDisplay => subTurnDisplay;

        private void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            hudVisualsManager = GetComponent<HudVisualManager>();
            cardVisualsManager = new CardVisualsManager();
        }

        private void Update()
        {
            Model.Encounter encounter = EncounterManager.Instance.Encounter;
            float fullTime = (encounter.Steps - 1) * progression;
            turnToDisplay = Mathf.FloorToInt(fullTime);
            subTurnDisplay = fullTime % 1;
            if(turnToDisplay == encounter.Steps - 1)
            {
                turnToDisplay = encounter.Steps - 2;
                subTurnDisplay = 1;
            }
            EncounterStep previousStep = encounter.GetStep(turnToDisplay);
            EncounterStep nextStep = encounter.GetStep(turnToDisplay + 1);
            cardVisualsManager.VisualizeEncounter(previousStep.State, nextStep.State);
            hudVisualsManager.VisualizeEncounter(previousStep.State, nextStep.State);
        }


        public CardBehavior InstantiateCardUi(PlayerCard card)
        {
            GameObject cardPrefab = CardVisualBindings.Instance.GetPrefabFor(card);
            GameObject obj = GameObject.Instantiate(cardPrefab);
            obj.transform.SetParent(cardsParent.transform, false);
            CardBehavior cardViewModel = obj.GetComponent<CardBehavior>();
            cardViewModel.Initialize(card.Identifier);
            return cardViewModel;
        }
    }
}