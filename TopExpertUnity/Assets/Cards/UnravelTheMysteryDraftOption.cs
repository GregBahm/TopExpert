namespace Investigation.Model
{
    public record UnravelTheMysteryDraftOption : StandardDraftOption
    {
        public override int DraftCost => 4;

        public override PlayerCard Card => new UnravelTheMysteryCard(new CardIdentifier());
    }
}