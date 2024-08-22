namespace Encounter.Model
{
    public record UnravelTheMysteryDraftOption : StandardDraftOption<UnravelTheMysteryCard>
    {
        public override int DraftCost => 4;
    }
}