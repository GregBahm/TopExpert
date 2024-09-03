using Investigation.Model;
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
        public RectTransform HandsRightPoint => handRightPoint;
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
            int mainTime = Mathf.FloorToInt(fullTime);
            float subTime = fullTime % 1;
            if(mainTime == encounter.Steps - 1)
            {
                mainTime = encounter.Steps - 2;
                subTime = 1;
            }
            EncounterStep previousStep = EncounterManager.Instance.Encounter.GetStep(mainTime);
            EncounterStep nextStep = EncounterManager.Instance.Encounter.GetStep(mainTime + 1);
            cardVisualsManager.VisualizeEncounter(previousStep.State, nextStep.State, subTime);
            hudVisualsManager.VisualizeEncounter(previousStep.State, nextStep.State, subTime);
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