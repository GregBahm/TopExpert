namespace Encounter.Behaviors
{
    public enum CardVisualState
    {
        InHand,             // In the hand or being dragged by the user
        Drawing,            // The card is being drawn from the deck
        ApplyingEffect,     // The card has been applied, and is playing its effect
        Discarding,         // The card is going to the discard pile
        Dissolve,          // The card is going to the dissolve pile
    }
}