using Investigation.Model;
using UnityEngine;
using UnityEngine.UI;

namespace Investigation.Behaviors
{

    [RequireComponent(typeof(HudVisualsManager))]
    public class EncounterVisualsManager : MonoBehaviour
    {
        private CardVisualsManager cardManager;
        private EffectorVisualsManager effectorManager;
        private HudVisualsManager hudVisualsManager;
        private DraftingVisualsManager draftingVisualsManager;

        public static EncounterVisualsManager Instance { get; private set; }

        [SerializeField]
        private RectTransform cardsParent;
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
        private float cardEntranceOffset;
        public float CardEntranceOffset => cardEntranceOffset;

        [SerializeField]
        private RectTransform effectorParent;
        [SerializeField]
        private RectTransform effectorLeftPoint;
        public RectTransform EffectorLeftPoint => effectorLeftPoint;
        [SerializeField]
        private RectTransform effectorRightPoint;
        public RectTransform EffectorRightPoint => effectorRightPoint;
        [SerializeField]
        private float effectorEntranceOffset;
        public float EffectorEntranceOffset => effectorEntranceOffset;

        [SerializeField]
        private RectTransform draftParent;
        [SerializeField]
        private RectTransform draftLeftPoint;
        public RectTransform DraftLeftPoint => draftLeftPoint;
        [SerializeField]
        private RectTransform draftRightPoint;
        public RectTransform DraftRightPoint => draftRightPoint;
        [SerializeField]
        private float draftEntranceOffset;
        public float DraftEntranceOffset => draftEntranceOffset;
        [SerializeField]
        private RectTransform draftDrawPile;
        public RectTransform DraftDrawPile => draftDrawPile;

        [SerializeField]
        private Color unappliedEffectorColor;
        public Color UnappliedEffectorColor => unappliedEffectorColor;
        [SerializeField]
        private Color appliedEffectorColor;
        public Color AppliedEffectorColor => appliedEffectorColor;

        [Range(0, 1)]
        [SerializeField]
        float progression;
        public float Progression { get => progression; set => progression = value; }

        [SerializeField]
        private float deckStackingOffset;
        public float DeckStackingOffset => deckStackingOffset;

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
        [SerializeField]
        private Color playableCardColor;
        public Color PlayableCardColor => playableCardColor;
        [SerializeField]
        private Color unplayableCardColor;
        public Color UnplayableCardColor => unplayableCardColor;

        private void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            hudVisualsManager = GetComponent<HudVisualsManager>();
            cardManager = new CardVisualsManager();
            effectorManager = new EffectorVisualsManager();
            draftingVisualsManager = new DraftingVisualsManager();
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
            draftingVisualsManager.VisualizeEncounter(previousStep.State, nextStep.State);
        }

        public CardVisualController InstantiateCardUi(PlayerCard card)
        {
            GameObject cardPrefab = VisualBinding.Instance.GetPrefabFor(card);
            GameObject obj = GameObject.Instantiate(cardPrefab);
            obj.transform.SetParent(cardsParent, false);
            CardVisualController cardViewModel = obj.GetComponent<CardVisualController>();
            cardViewModel.Initialize(card.Identifier);
            return cardViewModel;
        }

        public EffectorVisualController InstantiateEffectorUi(PersistantEffector effector)
        {
            GameObject effectorPrefab = VisualBinding.Instance.GetPrefabFor(effector);
            GameObject obj = GameObject.Instantiate(effectorPrefab);
            obj.transform.SetParent(effectorParent, false);
            EffectorVisualController viewModel = obj.GetComponent<EffectorVisualController>();
            viewModel.Initialize(effector.Identifier);
            return viewModel;
        }

        public DraftOptionController InstantiateDraftUi(DraftOption draftOption)
        {
            GameObject draftPrefab = VisualBinding.Instance.GetPrefabFor(draftOption);
            GameObject obj = GameObject.Instantiate(draftPrefab);
            obj.transform.SetParent(draftParent, false);
            DraftOptionController viewModel = obj.GetComponent<DraftOptionController>();
            viewModel.Initialize(draftOption.Identifier);
            return viewModel;
        }
    }
}