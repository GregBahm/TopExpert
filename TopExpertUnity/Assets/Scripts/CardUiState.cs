using Investigation.Behaviors;
using Investigation.Model;

public record CardUiState : ElementUIState<PlayerCard>
{
    public bool WasPlayed { get; init; }

    public CardUiLocation StartLocation { get; init; }
    public int StartOrder { get; init; }
    
    public CardUiLocation EndLocation { get; init; }
    public int EndOrder { get; init; }
}
