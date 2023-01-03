using System;
using System.Collections.Generic;
using UnityEngine;

namespace Combat.Behaviors
{
    public class HandBehavior : MonoBehaviour
    {
        [Range(0f, 1f)]
        [SerializeField]
        private float span;
        [SerializeField]
        [Range(0f, 90f)]
        private float angle;
        [SerializeField]
        private Transform handCircleCenter;

        private List<CardBehavior> cards;

        public CardBehavior HoveredCard { get; set; }
        public CardBehavior SelectedCard { get; set; }

        private void Start()
        {
            cards = new List<CardBehavior>();
        }

        public void AddCard(CardBehavior card)
        {
            cards.Add(card);
            card.gameObject.transform.SetParent(transform, false);
        }

        public void RemoveCard(CardBehavior card)
        {
            cards.Remove(card);
        }

        private void Update()
        {
            for (int i = 0; i < cards.Count; i++)
            {
                cards[i].transform.SetSiblingIndex(i);
            }
            if (HoveredCard == null)
            {
                for (int i = 0; i < cards.Count; i++)
                {
                    SetHandPositionForCard(cards[i], i, cards.Count);
                }
            }
            else
            {
                int hoveredIndex = cards.IndexOf(HoveredCard);
                for (int i = 0; i < cards.Count; i++)
                {
                    if (i > hoveredIndex)
                    {
                        SetHandPositionForCard(cards[i], i + 1, cards.Count + 1);
                    }
                    else if (i < hoveredIndex)
                    {
                        SetHandPositionForCard(cards[i], i, cards.Count + 1);
                    }
                    else
                    {
                        SetHandPositionForCard(cards[i], i, cards.Count);
                    }
                }
                HoveredCard.transform.SetSiblingIndex(cards.Count - 1);
            }
        }

        private Vector3 GetBasePosition(int i, int count)
        {
            float param = (float)i / (count - 1);
            float offset = Screen.width * (param - .5f) * span;
            return new Vector3(offset, 0, 0);
        }

        private void SetHandPositionForCard(CardBehavior cardBehavior, int i, int count)
        {
            Vector3 basePos = GetBasePosition(i, count);
            cardBehavior.HandHoverdPosition = basePos + transform.position + new Vector3(0, 200, 0);
            Vector3 toHandCenter = (handCircleCenter.position - (basePos + transform.position)).normalized;
            Quaternion angle = Quaternion.FromToRotation(Vector3.up, -toHandCenter);
            toHandCenter *= -handCircleCenter.localPosition.y;
            cardBehavior.HandRestPosition = handCircleCenter.position - toHandCenter;
            cardBehavior.HandRestRotation = angle;
        }
    }
}