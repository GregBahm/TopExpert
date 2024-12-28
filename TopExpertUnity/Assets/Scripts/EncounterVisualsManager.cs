using Investigation.Model;
using UnityEngine;

namespace Investigation.Behaviors
{

    [RequireComponent(typeof(HudVisualManager))]
    public class EncounterVisualsManager : MonoBehaviour
    {
        private CardVisualsManager cardManager;
        private EffectorVisualManager effectorManager;
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
        private RectTransform effectorLeftPoint;
        public RectTransform EffectorLeftPoint => effectorLeftPoint;
        [SerializeField]
        private RectTransform effectorRightPoint;
        public RectTransform EffectorRightPoint => effectorRightPoint;

        [SerializeField]
        private RectTransform cardsParent;
        [SerializeField]
        private RectTransform effectorParent;

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

        [SerializeField]
        private float cardHoveredScale;
        public float CardHoveredScale => cardHoveredScale;

        [SerializeField]
        private float cardHoveredOffset;
        public float CardHoveredOffset => cardHoveredOffset;

        [SerializeField]
        private float cardDeckScale;
        public float CardDeckScale => cardDeckScale;

        [SerializeField]
        private Color cardHandTint;
        public Color CardHandTint => cardHandTint;

        [SerializeField]
        private Color cardDiscardTint;
        public Color CardDiscardTint => cardDiscardTint;

        private void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            hudVisualsManager = GetComponent<HudVisualManager>();
            cardManager = new CardVisualsManager();
            effectorManager = new EffectorVisualManager();
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
            cardManager.VisualizeEncounter(previousStep.State, nextStep.State);
            effectorManager.VisualizeEncounter(previousStep.State, nextStep.State);
            hudVisualsManager.VisualizeEncounter(previousStep.State, nextStep.State);
        }


        public CardVisualController InstantiateCardUi(PlayerCard card)
        {
            GameObject cardPrefab = VisualBinding.Instance.GetPrefabFor(card);
            GameObject obj = GameObject.Instantiate(cardPrefab);
            obj.transform.SetParent(cardsParent.transform, false);
            CardVisualController cardViewModel = obj.GetComponent<CardVisualController>();
            cardViewModel.Initialize(card.Identifier);
            return cardViewModel;
        }

        public EffectorVisualController InstantiateEffectorUi(PersistantEffector effector)
        {
            GameObject effectorPrefab = VisualBinding.Instance.GetPrefabFor(effector);
            GameObject obj = GameObject.Instantiate(effectorPrefab);
            obj.transform.SetParent(effectorParent.transform, false);
            EffectorVisualController viewModel = obj.GetComponent<EffectorVisualController>();
            viewModel.Initialize(effector.Identifier);
            return viewModel;
        }
    }
}