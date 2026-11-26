namespace CoinCollection.Core;

public sealed record DesignCandidate(string Name, CoinDesign Design, string Source);
public sealed record CatalogLookup(IReadOnlyList<DesignCandidate> Candidates, bool CoverageComplete);

public sealed class CoinCatalog
{
    public static CoinCatalog Seed { get; } = new();
    public string Revision => "seed-1";
    // Deliberately bounded to one example. It establishes neither mint attribution nor uniqueness.
    // Evidence and limitations: docs/research/offline-coin-catalog.md (reviewed 2026-11-25).
    public CatalogLookup Lookup(string coinTypeId, IssueYear year)
    {
        if (coinTypeId == "penny" && (year == IssueYear.Unknown || year == IssueYear.Known(1974)))
            return new(Array.AsReadOnly(new[] { new DesignCandidate("Lincoln Memorial", new(DesignField.Known("lincoln-memorial"), DesignField.NotApplicable), "https://www.usmint.gov/learn/coins-and-medals/circulating-coins/penny") }), false);
        return new(Array.Empty<DesignCandidate>(), false);
    }
}
