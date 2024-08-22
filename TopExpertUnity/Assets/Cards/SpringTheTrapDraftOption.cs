namespace Encounter.Model
{
    public record SpringTheTrapDraftOption : StandardDraftOption<SpringTheTrapCard>
    {
        public override int DraftCost => 5;
    }
}