namespace Investigation.Model
{
    public record SpringTheTrapDraftOption (DraftOptionIdentifier Identifier) 
        : StandardDraftOption(Identifier)
    {
        public override int DraftCost => 5;

        public override PlayerCard Card => new SpringTheTrapCard(new CardIdentifier());
    }
}