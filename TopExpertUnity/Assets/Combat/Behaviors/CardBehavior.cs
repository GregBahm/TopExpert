using Combat.Model;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Combat.Behaviors
{
    public class CardBehavior : MonoBehaviour
    {
        private HandBehavior hand;
        private float selectedness;

        public ICard Model { get; private set; }

        [SerializeField]
        private TextMeshProUGUI label;

        [SerializeField]
        private Image cardArt;

        public Vector3 HandHoverdPosition { get; set; }
        public Vector3 HandRestPosition { get; set; }
        public Quaternion HandRestRotation { get; set; }

        public CardVisualState State { get; set; }

        private void Update()
        {
            gameObject.name = Model.Name + (hand.HoveredCard == this ? "*" : "");
            UpdatePosition();
            UpdateScale();
        }

        private void UpdatePosition()
        {
            if (State == CardVisualState.Available)
            {
                if (hand.SelectedCard != this)
                {
                    Vector3 targetPosition = hand.HoveredCard == this ? HandHoverdPosition : HandRestPosition;
                    transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * 25);
                }
                Quaternion targetRotation = hand.HoveredCard != this ? HandRestRotation : Quaternion.identity;
                transform.localRotation = Quaternion.Lerp(transform.localRotation, targetRotation, Time.deltaTime * 25);
            }
        }

        private void UpdateScale()
        {
            bool isHovered = hand.HoveredCard == this;
            float selectednessTarget = isHovered ? 1 : 0;
            selectedness = Mathf.Lerp(selectedness, selectednessTarget, Time.deltaTime * 25);
            float scale = Mathf.Lerp(1, 1.5f, selectedness);
            transform.localScale = new Vector3(scale, scale, scale);
        }

        public void Initialize(HandBehavior hand, ICard model)
        {
            this.hand = hand;
            this.Model = model;
            label.text = model.Name;
            cardArt.sprite = CardVisualBindings.Instance.GetVisualFor(model);
        }

        public void PlayCard()
        {
            EncounterManager.Instance.Encounter.PlayCard(Model, null);
            hand.RemoveCard(hand.SelectedCard);
            //if(Model.Consumeable) //TODO: Finish this
        }
    }
}