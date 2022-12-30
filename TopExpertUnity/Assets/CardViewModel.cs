using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using TopExpert.Combat;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CardViewModel : MonoBehaviour
{
    private HandViewModel hand;
    private float selectedness;

    private ICard model;

    [SerializeField]
    private TextMeshProUGUI label;

    [SerializeField]
    private Image cardArt;

    public Vector3 HandPosition { get; set; }
    public Quaternion HandRotation { get; set; }

    private void Update()
    {
        gameObject.name = model.Name + (hand.HoveredCard == this ? "*" : "");
        UpdatePosition();
        UpdateScale();
    }

    private void UpdatePosition()
    {
        if (hand.SelectedCard != this)
        {
            transform.position = Vector3.Lerp(transform.position, HandPosition, Time.deltaTime * 25);
        }
        Quaternion targetRotation = hand.SelectedCard != this ? HandRotation : Quaternion.identity;
        transform.localRotation = Quaternion.Lerp(transform.localRotation, targetRotation, Time.deltaTime * 25);
    }

    private void UpdateScale()
    {
        bool isHovered = hand.HoveredCard == this;
        float selectednessTarget = isHovered ? 1 : 0;
        selectedness = Mathf.Lerp(selectedness, selectednessTarget, Time.deltaTime * 25);
        float scale = Mathf.Lerp(1, 1.5f, selectedness);
        transform.localScale = new Vector3(scale, scale, scale);
    }

    public void Initialize(HandViewModel hand, ICard model)
    {
        this.hand = hand;
        this.model = model;
        label.text = model.Name;
        cardArt.sprite = CardVisualBindings.Instance.GetVisualFor(model);
    }
}
