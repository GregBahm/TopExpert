using Investigation.Model;
using TMPro;
using UnityEngine;

namespace Investigation.Behaviors
{
    public class EncounterVisualsManager : MonoBehaviour
    {
        private CardVisualsManager cardVisualsManager;

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
        private TextMeshProUGUI currentActions;
        [SerializeField]
        private TextMeshProUGUI totalActions;
        [SerializeField]
        private TextMeshProUGUI currentInsights;

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
            cardVisualsManager.VisualizeEncounter(mainTime, subTime);
        }


        public CardBehavior InstantiateCardUi(PlayerCard card)
        {
            GameObject cardPrefab = CardVisualBindings.Instance.GetPrefabFor(card);
            GameObject obj = GameObject.Instantiate(cardPrefab);
            obj.transform.SetParent(cardsParent.transform, false);
            CardBehavior cardViewModel = obj.GetComponent<CardBehavior>();
            return cardViewModel;
        }
    }
}