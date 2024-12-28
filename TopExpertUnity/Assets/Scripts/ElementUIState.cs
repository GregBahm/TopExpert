using Investigation.Model;


namespace Investigation.Behaviors
{
    public record ElementUIState<T>
    {
        public ElementIdentifier Identifier { get; init; }

        public EncounterState StartState { get; init; }
        public T StartElementState { get; init; }
        public EncounterState EndState { get; init; }
        public T EndElementState { get; init; }
    }
}