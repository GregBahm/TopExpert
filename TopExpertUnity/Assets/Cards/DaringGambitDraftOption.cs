namespace Encounter.Model
{
    public record DaringGambitDraftOption : StandardDraftOption<DaringGambitCard>
    {
        public override int DraftCost => 4;
    }
}