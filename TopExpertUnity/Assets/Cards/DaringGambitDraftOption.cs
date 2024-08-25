namespace Investigation.Model
{
    public record DaringGambitDraftOption : StandardDraftOption
    {
        public override int DraftCost => 4;

        public override PlayerCard Card => new DaringGambitCard(new CardIdentifier());
    }
}