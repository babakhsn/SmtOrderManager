namespace SmtOrderManager.Application.Contracts;

public sealed record Paging(int Skip = 0, int Take = 50)
{
    public int NormalizedSkip => Skip < 0 ? 0 : Skip;
    public int NormalizedTake => Take switch
    {
        <= 0 => 50,
        > 200 => 200,
        _ => Take
    };
}
