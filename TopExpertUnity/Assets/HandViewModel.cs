using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using TopExpert.Combat;
using UnityEngine;

public class HandViewModel : MonoBehaviour
{
    [Range(0f, 1f)]
    [SerializeField]
    private float span;
    [SerializeField]
    [Range(0f, 90f)]
    private float angle;
    [SerializeField]
    private Transform handCircleCenter;

    private List<CardViewModel> cards;

    public CardViewModel HoveredCard { get; set; }
    public CardViewModel SelectedCard { get; set; }

    private void Start()
    {
        cards = new List<CardViewModel>();
    }

    public void AddCard(CardViewModel card)
    {
        cards.Add(card);
        card.gameObject.transform.SetParent(transform, false);
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
                if(i > hoveredIndex)
                {
                    SetHandPositionForCard(cards[i], i + 1, cards.Count + 1);
                } 
                else if(i < hoveredIndex)
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

    private void SetHandPositionForCard(CardViewModel cardViewModel, int i, int count)
    {
        Vector3 basePos = GetBasePosition(i, count);
        Vector3 toHandCenter = (handCircleCenter.position - (basePos + transform.position)).normalized; 
        Quaternion angle = Quaternion.FromToRotation(Vector3.up, -toHandCenter);
        toHandCenter *= -handCircleCenter.localPosition.y;
        cardViewModel.HandPosition = handCircleCenter.position - toHandCenter;
        cardViewModel.HandRotation = angle;
    }
}