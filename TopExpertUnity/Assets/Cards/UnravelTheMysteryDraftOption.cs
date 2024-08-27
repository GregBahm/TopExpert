namespace Investigation.Model
{
    public record UnravelTheMysteryDraftOption (ElementIdentifier Identifier) 
        : StandardDraftOption(Identifier)
    {
        public override int DraftCost => 4;

        public override PlayerCard Card => new UnravelTheMysteryCard(new ElementIdentifier());
    }
}