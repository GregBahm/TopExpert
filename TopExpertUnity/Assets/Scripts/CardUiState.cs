using Investigation.Behaviors;
using Investigation.Model;

public record CardUiState : ElementUIState<PlayerCard>
{
    public bool WasPlayed { get; init; }

    public CardExistenceLocation StartLocation { get; init; }
    public int StartOrder { get; init; }
    
    public CardExistenceLocation EndLocation { get; init; }
    public int EndOrder { get; init; }
}
