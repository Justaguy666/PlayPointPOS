namespace Application.Members;

public sealed record MemberLookupItem
{
    public string Id { get; init; } = string.Empty;

    public string FullName { get; init; } = string.Empty;

    public string PhoneNumber { get; init; } = string.Empty;
}
