namespace CoinCollection.Core;

public sealed record Denomination(string Name, decimal FaceValue);
public sealed record CoinType(string Id, string Name, Denomination Denomination);
public static class CoinTypes
{
    private static readonly Denomination Cent = new("Cent", .01m);
    private static readonly Denomination Nickel = new("Nickel", .05m);
    private static readonly Denomination Dime = new("Dime", .10m);
    private static readonly Denomination Quarter = new("Quarter", .25m);
    private static readonly Denomination HalfDollar = new("Half dollar", .50m);
    private static readonly Denomination Dollar = new("Dollar", 1m);

    public static IReadOnlyList<Denomination> Denominations { get; } = Array.AsReadOnly(new[] {
        Cent, Nickel, Dime, Quarter, HalfDollar, Dollar });
    public static IReadOnlyList<CoinType> All { get; } = Array.AsReadOnly(new[] {
        new CoinType("penny", "Penny", Cent), new("nickel", "Nickel", Nickel), new("dime", "Dime", Dime), new("quarter", "Quarter", Quarter), new("half-dollar", "Half dollar", HalfDollar),
        new("eisenhower-dollar", "Eisenhower dollar", Dollar), new("susan-b-anthony-dollar", "Susan B. Anthony dollar", Dollar), new("sacagawea-dollar", "Sacagawea dollar", Dollar), new("presidential-dollar", "Presidential dollar", Dollar), new("american-innovation-dollar", "American Innovation dollar", Dollar) });
    public static CoinType? Find(string id) => All.FirstOrDefault(t => t.Id == id);
}
public enum ValueState { Known, Unknown, NotApplicable }
public sealed record IssueYear(ValueState State, int? Value)
{
    public static IssueYear Known(int year) => new(ValueState.Known, year);
    public static IssueYear Unknown { get; } = new(ValueState.Unknown, null);
}
public enum MintMarkState { Explicit, None, Unknown }
public sealed record MintMark(MintMarkState State, string? Mark)
{
    public static MintMark Explicit(string mark) => new(MintMarkState.Explicit, mark);
    public static MintMark None { get; } = new(MintMarkState.None, null);
    public static MintMark Unknown { get; } = new(MintMarkState.Unknown, null);
}
public sealed record DesignField(ValueState State, string? Value)
{
    public static DesignField Known(string value) => new(ValueState.Known, value);
    public static DesignField Unknown { get; } = new(ValueState.Unknown, null);
    public static DesignField NotApplicable { get; } = new(ValueState.NotApplicable, null);
}
public sealed record CoinDesign(DesignField Family, DesignField Subject, string? DateInscription = null)
{
    public static CoinDesign Unknown { get; } = new(DesignField.Unknown, DesignField.Unknown);
}
public sealed record CoinIdentity(string CoinTypeId, IssueYear Year, MintMark MintMark, CoinDesign Design)
{
    // An inscription describes the issue; it is not another quantity-grouping characteristic.
    public bool Equals(CoinIdentity? other) => other is not null && CoinTypeId == other.CoinTypeId && Year == other.Year
        && MintMark == other.MintMark && Design?.Family == other.Design?.Family && Design?.Subject == other.Design?.Subject;
    public override int GetHashCode() => HashCode.Combine(CoinTypeId, Year, MintMark, Design?.Family, Design?.Subject);
}
public static class CoinValidation
{
    public static string? Validate(CoinIdentity? coin, int quantity)
    {
        if (quantity <= 0) return "Quantity must be a positive whole number.";
        if (coin is null || CoinTypes.Find(coin.CoinTypeId) is null) return "Unknown coin type.";
        if (coin.Year is null || (coin.Year.State == ValueState.Known ? coin.Year.Value is not (>= 1 and <= 9999) : coin.Year.State != ValueState.Unknown || coin.Year.Value is not null)) return "Year must be 1–9999 or explicitly unknown.";
        var mint = coin.MintMark;
        if (mint is null || !Enum.IsDefined(mint.State) || (mint.State == MintMarkState.Explicit ? string.IsNullOrWhiteSpace(mint.Mark) || mint.Mark.Length > 8 || mint.Mark.Any(c => c is < 'A' or > 'Z') : mint.Mark is not null)) return "Mint mark must be uppercase letters, no mark, or unknown.";
        if (coin.Design is null || !ValidField(coin.Design.Family) || !ValidField(coin.Design.Subject)) return "Design fields must be known, unknown, or not applicable.";
        if (coin.Design.Family.State != ValueState.Known && coin.Design.Subject.State == ValueState.Known) return "A known design subject requires a known family.";
        if (coin.Design.DateInscription is { } inscription && (string.IsNullOrWhiteSpace(inscription) || inscription.Length > 100)) return "Invalid date inscription.";
        return null;
    }
    private static bool ValidField(DesignField? field) => field is not null && Enum.IsDefined(field.State) && (field.State == ValueState.Known ? !string.IsNullOrWhiteSpace(field.Value) && field.Value.Length <= 200 : field.Value is null);
}
