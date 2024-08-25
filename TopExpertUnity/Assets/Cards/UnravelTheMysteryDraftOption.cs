namespace Investigation.Model
{
    public record UnravelTheMysteryDraftOption (DraftOptionIdentifier Identifier) 
        : StandardDraftOption(Identifier)
    {
        public override int DraftCost => 4;

        public override PlayerCard Card => new UnravelTheMysteryCard(new CardIdentifier());
    }
}