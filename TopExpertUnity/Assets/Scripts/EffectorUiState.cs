using Investigation.Model;


namespace Investigation.Behaviors
{
    public record EffectorUiState : ElementUIState<PersistantEffector>
    {
        public EffectorExistenceLocation StartLocation { get; init; }

        public EffectorExistenceLocation EndLocation { get; init; }
    }
}