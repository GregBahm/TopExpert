using Investigation.Model;
using UnityEngine;

namespace Investigation.Behaviors
{
    public class EncounterVisualsManager : MonoBehaviour
    {
        private CardVisualsManager cardVisualsManager;

        public static EncounterVisualsManager Instance { get; private set; }

        [SerializeField]
        private Canvas canvas;

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

        [Range(0, 1)]
        [SerializeField]
        float progression;

        public string debug;

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
            float fullTime = (encounter.Turns - 1) * progression;
            int mainTime = Mathf.FloorToInt(fullTime);
            float subTime = fullTime % 1;
            if(mainTime == encounter.Turns - 1)
            {
                mainTime = encounter.Turns - 2;
                subTime = 1;
            }
            debug = "Turn " + (mainTime + 1) + " of " + encounter.Turns + " turns, " + subTime * 100;
            cardVisualsManager.DrawEncounter(mainTime, subTime);
        }


        public CardBehavior InstantiateCardUi(PlayerCard card)
        {
            GameObject cardPrefab = CardVisualBindings.Instance.GetPrefabFor(card);
            GameObject obj = GameObject.Instantiate(cardPrefab);
            obj.transform.SetParent(canvas.transform, false);
            CardBehavior cardViewModel = obj.GetComponent<CardBehavior>();
            return cardViewModel;
        }
    }
}