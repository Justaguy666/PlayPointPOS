namespace Application.Games;

public sealed record GameType
{
    public string Id { get; init; } = string.Empty;

    public string Name { get; set; } = string.Empty;
}
