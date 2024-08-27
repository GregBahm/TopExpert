using Investigation.Model;

public record CardUiState
{
    public ElementIdentifier Identifier { get; init; }

    public EncounterState StartState { get; init; }
    public PlayerCard StartCardState { get; init; }
    public EncounterState EndState { get; init; }
    public PlayerCard EndCardState { get; init; }

    public bool WasPlayed { get; init; }

    public CardUiLocation StartLocation { get; init; }
    public int StartOrder { get; init; }
    
    public CardUiLocation EndLocation { get; init; }
    public int EndOrder { get; init; }
}
