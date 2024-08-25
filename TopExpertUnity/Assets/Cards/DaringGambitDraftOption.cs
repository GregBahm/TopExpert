namespace Investigation.Model
{
    public record DaringGambitDraftOption(DraftOptionIdentifier Identifier) 
        : StandardDraftOption(Identifier)
    {
        public override int DraftCost => 4;

        public override PlayerCard Card => new DaringGambitCard(new CardIdentifier());
    }
}