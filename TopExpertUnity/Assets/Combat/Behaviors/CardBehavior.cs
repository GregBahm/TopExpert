using Combat.Model;
using System;
using System.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Combat.Behaviors
{
    public class CardBehavior : MonoBehaviour
    {
        private HandBehavior hand;

        public ICard Model { get; private set; }

        [SerializeField]
        private TextMeshProUGUI label;

        [SerializeField]
        private Image cardArt;

        [SerializeField]
        private RectTransform playabilityIndicator;
        private Image playabilityIndicatorImage;

        [SerializeField]
        private float playabilityIndicatorMarginMax;

        [SerializeField]
        private Color canPlayCardColor;
        [SerializeField]
        private Color cannotPlayCardColor;

        [SerializeField]
        private float stateTransitionTime;
        private float currentTransitionTime;
        private float TransitionParam
        {
            get
            {
                float ret = currentTransitionTime / stateTransitionTime;
                return Mathf.Clamp01(ret);
            }
        }

        public Vector3 HandHoverdPosition { get; set; }
        public Vector3 HandRestPosition { get; set; }
        public Quaternion HandRestRotation { get; set; }

        private CardVisualState state;
        public CardVisualState State 
        { 
            get => state; 
            set
            {
                if(value != state)
                {
                    state = value;
                    currentTransitionTime = 0;
                }
            }
        }

        private static Vector3 mouseDragStartPos;
        private static Vector3 cardDragStartPos;

        private float playabilityIndicatorMargin;
        private Color playabilityIndicatorColor;

        public static void StartDrag(CardBehavior hoveredCard)
        {
            mouseDragStartPos = Input.mousePosition;
            cardDragStartPos = hoveredCard.transform.position;
        }

        public bool IsHovered
        {
            get { return hand.HoveredCard == this; }
        }

        public bool IsSelected
        {
            get { return hand.SelectedCard == this; }
        }

        private void Start()
        {
            playabilityIndicatorImage = playabilityIndicator.gameObject.GetComponent<Image>();
        }

        private void Update()
        {
            gameObject.name = Model.Name + " [" + State.ToString() + "]";
            currentTransitionTime += Time.deltaTime;
            UpdateState();
            UpdatePosition();
            UpdatePlayabilityIndicator();
        }

        private void UpdateState()
        {
            bool transitionComplete = currentTransitionTime > stateTransitionTime;
            if (transitionComplete)
            {
                if (State == CardVisualState.Drawing)
                {
                    State = CardVisualState.InHand;
                    return;
                }
                if (State == CardVisualState.ApplyingEffect)
                {
                    StartOutro();
                    return;
                }
                if (State == CardVisualState.Discarding
                    || State == CardVisualState.Exhausts
                    || State == CardVisualState.Consuming)
                {
                    Destroy(this.gameObject);
                    return;
                }
            }
        }

        private void UpdatePlayabilityIndicator()
        {
            UpdatePlayabilityIndicatorTargets();
            ApplyPlayabilityIndicatorTargets();
        }

        private void ApplyPlayabilityIndicatorTargets()
        {
            playabilityIndicator.offsetMin = new Vector2(-playabilityIndicatorMargin, -playabilityIndicatorMargin);
            playabilityIndicator.offsetMax = new Vector2(playabilityIndicatorMargin, playabilityIndicatorMargin);
            playabilityIndicatorImage.color = playabilityIndicatorColor;
        }

        private void UpdatePlayabilityIndicatorTargets()
        {
            float marginTarget = 0;
            Color colorTarget = cannotPlayCardColor;
            if (GetIsHeldOverDropZone())
            {
                CardPlayability playability = InterfaceManager.Instance.GetCardPlayability(Model);
                if (playability.IsPlayable)
                {
                    marginTarget = playabilityIndicatorMarginMax;
                    colorTarget = canPlayCardColor;
                }
                else
                {
                    marginTarget = playabilityIndicatorMarginMax;
                }
            }
            playabilityIndicatorMargin = Mathf.Lerp(playabilityIndicatorMargin, marginTarget, Time.deltaTime * 25);
            playabilityIndicatorColor = Color.Lerp(playabilityIndicatorColor, colorTarget, Time.deltaTime * 25);
        }

        private bool GetIsHeldOverDropZone()
        {
            if (!IsSelected) return false;
            return InterfaceManager.Instance.GetIfCardIsOverDropZone();
        }

        private Vector3 outroStartPosition;
        private float outroStartScale; 

        private void UpdatePosition()
        {
            if (State == CardVisualState.Drawing)
            {
                float boost = Screen.height * .25f;
                Vector3 drawPosition = InterfaceManager.Instance.DrawPosition;
                EnterOrExitCard(drawPosition, HandRestPosition, .5f, 1, 2, boost);
            }
            if(State == CardVisualState.Discarding)
            {
                float boost = Screen.height * .25f;
                Vector3 discardPosition = InterfaceManager.Instance.DiscardPosition;
                EnterOrExitCard(outroStartPosition, discardPosition, outroStartScale, .5f, 2, boost);
            }
            if (State == CardVisualState.InHand
                || State == CardVisualState.ApplyingEffect)
            {
                Vector3 targetPosition = GetTargetPosition();
                float positionSmoothing = IsSelected ? 1.0f : Time.deltaTime * 25;
                transform.position = Vector3.Lerp(transform.position, targetPosition, positionSmoothing);

                Quaternion targetRotation = GetTargetRotation();
                transform.localRotation = Quaternion.Lerp(transform.localRotation, targetRotation, Time.deltaTime * 25);

                float scaleTarget = GetInHandScaleTarget();
                float scale = Mathf.Lerp(transform.localScale.x, scaleTarget, Time.deltaTime * 25);
                transform.localScale = new Vector3(scale, scale, scale);
            }
        }

        private void EnterOrExitCard(Vector3 introPos, Vector3 outroPos, float introScale, float outroScale, float ramp, float boost)
        {
            float rampedParam = Mathf.Pow(TransitionParam, ramp);
            Vector3 baseTarget = Vector3.Lerp(introPos, outroPos, rampedParam);
            float arcedBoost = Mathf.Sin(rampedParam * Mathf.PI) * boost;
            transform.position = baseTarget + new Vector3(0, arcedBoost, 0);

            float scale = Mathf.Lerp(introScale, outroScale, rampedParam);
            transform.localScale = new Vector3(scale, scale, scale);
        }

        private Vector3 GetTargetPosition()
        {
            if (IsSelected)
            {
                Vector3 delta = Input.mousePosition - mouseDragStartPos;
                return cardDragStartPos + delta;
            }
            if (State == CardVisualState.InHand)
                return IsHovered ? HandHoverdPosition : HandRestPosition;
            return InterfaceManager.Instance.EffectApplyingPosition;
        }

        private Quaternion GetTargetRotation()
        {
            if (State == CardVisualState.InHand && !IsHovered)
                return HandRestRotation;
            return Quaternion.identity;
        }

        private float GetInHandScaleTarget()
        {
            if (State == CardVisualState.ApplyingEffect)
                return 1f;
            if (IsSelected)
                return 1.45f;
            if (IsHovered)
                return 1.5f;
            return 1;
        }

        public void Initialize(HandBehavior hand, ICard model, float drawDelay = 0)
        {
            this.hand = hand;
            Model = model;
            State = CardVisualState.Drawing;
            currentTransitionTime = -drawDelay;
            label.text = model.Name;
            cardArt.sprite = CardVisualBindings.Instance.GetVisualFor(model);
        }

        public void PlayCard()
        {
            EncounterManager.Instance.Encounter.PlayCard(Model);
            hand.RemoveCard(hand.SelectedCard);
            State = CardVisualState.ApplyingEffect;
        }

        public void StartOutro()
        {
            State = GetStateAfterEffect();
            outroStartPosition = transform.position;
            outroStartScale = transform.localScale.x;
        }

        private CardVisualState GetStateAfterEffect()
        {
            if (Model.Exhausts)
                return CardVisualState.Exhausts;
            if (Model.Consumeable)
                return CardVisualState.Consuming;
            return CardVisualState.Discarding;
        }
    }
}