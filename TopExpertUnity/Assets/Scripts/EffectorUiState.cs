using Investigation.Model;


namespace Investigation.Behaviors
{
    public record EffectorUiState : ElementUIState<PersistantEffector>
    {
        public EffectorExistenceLocation StartLocation { get; init; }
        public int StartOrder { get; init; }

        public EffectorExistenceLocation EndLocation { get; init; }
        public int EndOrder { get; init; }
    }
}