namespace Combat.Behaviors
{
    public enum CardVisualState
    {
        Available, // In the hand or being dragged by the user
        Drawing, // The card is being drawn fro the deck
        ApplyingEffect, // The card has been applied, and is playing its effect
        Discarding, // The card is going to the discard pile
        Consuming, // The card is going to the consume pile
        Disintegrating, // The card (probably a token) is being removed from the game
    }
}