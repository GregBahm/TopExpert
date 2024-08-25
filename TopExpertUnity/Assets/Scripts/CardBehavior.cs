using Investigation.Model;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Investigation.Behaviors
{
    /*
    public class CardBehavior : MonoBehaviour
    {
        private HandBehavior hand;
        private CardVisuals cardVisuals;

        public PlayerCard Model { get; private set; }

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

        public Vector3 HandHoverdPosition { get; set; }
        public Vector3 HandRestPosition { get; set; }
        public Quaternion HandRestRotation { get; set; }

        private float playabilityIndicatorMargin;
        private Color playabilityIndicatorColor;

        public bool IsHovered
        {
            get { return hand.HoveredCard == this; }
        }

        private bool wasPressed;
        public bool IsPressed
        {
            get
            {
                return IsHovered && Input.GetMouseButton(0);
            }
        }

        private void Start()
        {
            playabilityIndicatorImage = playabilityIndicator.gameObject.GetComponent<Image>();
        }

        private void Update()
        {
            gameObject.name = cardVisuals.Name + " [" + State.ToString() + "]";
            UpdateState();
            UpdatePosition();
            UpdatePlayabilityIndicator();
            UpdateIsPlayed();
            wasPressed = IsPressed;
        }

        private void UpdatePlayabilityIndicator()
        {
            playabilityIndicatorImage.color = GetPlayabilityIndicatorColor();
        }

        private Color GetPlayabilityIndicatorColor()
        {
            if (IsHovered)
            {
                bool playability = Model.CanPlay(EncounterManager.Instance.Encounter.CurrentState);
                if (!playability)
                    return Color.gray;
                if (IsPressed)
                {
                    return Color.yellow;
                }
                return Color.green;
            }
            return Color.gray;
        }

        private void UpdateIsPlayed()
        {
            if (wasPressed && IsHovered && Input.GetMouseButtonUp(0))
            {
                PlayCard();
            }
        }

        private void UpdatePosition()
        {
            if (State == CardVisualState.Drawing)
            {
                float boost = Screen.height * .25f;
                Vector3 drawPosition = InterfaceManager.Instance.DrawPosition;
                EnterOrExitCard(drawPosition, HandRestPosition, .5f, 1, 2, boost);
            }
            if (State == CardVisualState.Discarding)
            {
                float boost = Screen.height * .25f;
                Vector3 discardPosition = InterfaceManager.Instance.DiscardPosition;
                EnterOrExitCard(outroStartPosition, discardPosition, outroStartScale, .5f, 2, boost);
            }
            if (State == CardVisualState.InHand
                || State == CardVisualState.ApplyingEffect)
            {
                Vector3 targetPosition = GetTargetPosition();
                transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * 25);

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
            if (State == CardVisualState.ApplyingEffect)
            {
                return HandHoverdPosition + new Vector3(0, 50, 0);
            }
            return IsHovered ? HandHoverdPosition : HandRestPosition;
        }

        private Quaternion GetTargetRotation()
        {
            return (IsHovered || State == CardVisualState.ApplyingEffect) ? Quaternion.identity : HandRestRotation;
        }

        private float GetInHandScaleTarget()
        {
            if (State == CardVisualState.ApplyingEffect)
                return 1.1f;
            if (IsPressed)
                return .9f;
            if (IsHovered)
                return 1f;
            return .5f;
        }

        public void Initialize(HandBehavior hand, PlayerCard model, float drawDelay = 0)
        {
            this.hand = hand;
            Model = model;
            State = CardVisualState.Drawing;
            currentTransitionTime = -drawDelay;
            cardVisuals = CardVisualBindings.Instance.GetVisualsFor(model);
            label.text = cardVisuals.Name;
            cardArt.sprite = cardVisuals.Image;
        }

        public void PlayCard()
        {
            EncounterManager.Instance.Encounter.PlayCard(Model);
            hand.RemoveCard(this);
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
            if (Model.DissolvesOnPlay)
                return CardVisualState.Dissolve;
            return CardVisualState.Discarding;
        }
    }
    */
}