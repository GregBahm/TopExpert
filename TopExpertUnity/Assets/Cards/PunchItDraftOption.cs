namespace Encounter.Model
{
    public record PunchItDraftOption : StandardDraftOption<PunchItCard>
    {
        public override int DraftCost => 2;
    }
}