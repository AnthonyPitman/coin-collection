using CoinCollection.Core;
using Xunit;

namespace CoinCollection.Core.Tests;

public class SessionTests
{
    [Fact]
    public void Missing_collection_initializes_and_reports_all_six_zero_denominations()
    {
        var storage = new MemoryStorage();
        var session = new CollectionSession(storage);
        Assert.Equal(LoadStatus.Created, session.Open().Status);
        Assert.False(session.HasPendingChanges);
        Assert.Equal(6, session.Summary.Rows.Count);
        Assert.All(session.Summary.Rows, row => { Assert.Equal(0, row.CoinCount); Assert.Equal(0m, row.FaceValue); });
        Assert.Equal(0, session.Summary.CoinCount);
        Assert.Equal(0m, session.Summary.FaceValue);
    }
}

internal sealed class MemoryStorage : ICollectionStorage
{
    private CollectionSnapshot? snapshot;
    private StorageRevision revision = StorageRevision.Missing;
    public SaveStatus NextSaveStatus { get; set; } = SaveStatus.Saved;
    public LoadResult Load() => snapshot is null
        ? new(LoadStatus.Missing, null, revision)
        : new(LoadStatus.Loaded, snapshot, revision);
    public SaveResult Save(CollectionSnapshot value, StorageRevision expectedRevision)
    {
        if (NextSaveStatus != SaveStatus.Saved) return new(NextSaveStatus, null, "Controlled failure");
        if (expectedRevision != revision) return new(SaveStatus.Conflict, null);
        snapshot = value;
        revision = new(Guid.NewGuid().ToString());
        return new(SaveStatus.Saved, revision);
    }
}

public class AdditionTests
{
    [Fact]
    public void Adding_matching_coins_combines_quantities_and_groups_dollar_types()
    {
        var storage = new MemoryStorage();
        var session = new CollectionSession(storage);
        session.Open();
        var coin = new CoinIdentity("penny", IssueYear.Known(1974), MintMark.Explicit("D"), CoinDesign.Unknown);
        Assert.Equal(AddStatus.AppliedAndSaved, session.Add(new(coin, 1, true)).Status);
        Assert.Equal(AddStatus.AppliedAndSaved, session.Add(new(coin, 1, true)).Status);
        Assert.Equal(AddStatus.AppliedAndSaved, session.Add(new(new("eisenhower-dollar", IssueYear.Unknown, MintMark.None, CoinDesign.Unknown), 1, true)).Status);
        Assert.Equal(AddStatus.AppliedAndSaved, session.Add(new(new("sacagawea-dollar", IssueYear.Unknown, MintMark.None, CoinDesign.Unknown), 1, true)).Status);
        Assert.Equal(3, session.Snapshot.Entries.Count);
        Assert.Equal(2, session.Snapshot.Entries[0].Quantity);
        Assert.Equal(4, session.Summary.CoinCount);
        Assert.Equal(2.02m, session.Summary.FaceValue);
        Assert.Equal(2, session.Summary.Rows.Single(x => x.Denomination == "Dollar").CoinCount);
        var restarted = new CollectionSession(storage);
        restarted.Open();
        Assert.Equal(2.02m, restarted.Summary.FaceValue);
    }
}

public class PendingSaveTests
{
    [Theory]
    [InlineData(SaveStatus.WriteFailure)]
    [InlineData(SaveStatus.Conflict)]
    public void Retrying_failed_save_preserves_one_applied_addition(SaveStatus status)
    {
        var storage = new MemoryStorage();
        var session = new CollectionSession(storage);
        session.Open();
        storage.NextSaveStatus = status;
        var coin = new CoinIdentity("penny", IssueYear.Known(1974), MintMark.Explicit("D"), CoinDesign.Unknown);
        Assert.Equal(AddStatus.AppliedButUnsaved, session.Add(new(coin, 1, true)).Status);
        Assert.True(session.HasPendingChanges);
        Assert.Equal(AddStatus.PendingChanges, session.Add(new(coin, 1, true)).Status);
        storage.NextSaveStatus = SaveStatus.Saved;
        Assert.Equal(SaveStatus.Saved, session.RetrySave().Status);
        Assert.False(session.HasPendingChanges);
        var restarted = new CollectionSession(storage);
        restarted.Open();
        Assert.Equal(1, restarted.Summary.CoinCount);
    }
    [Fact]
    public void Unconfirmed_catalog_miss_and_invalid_quantity_do_not_mutate_collection()
    {
        var session = new CollectionSession(new MemoryStorage());
        session.Open();
        var coin = new CoinIdentity("penny", IssueYear.Known(1850), MintMark.None, CoinDesign.Unknown);
        Assert.Equal(AddStatus.ConfirmationRequired, session.Add(new(coin)).Status);
        Assert.Equal(AddStatus.Invalid, session.Add(new(coin, 0, true)).Status);
        Assert.Empty(session.Snapshot.Entries);
        Assert.Equal(AddStatus.AppliedAndSaved, session.Add(new(coin, 1, true)).Status);
    }
}

public class ReferenceSeedTests
{
    [Fact]
    public void A_single_seed_candidate_is_incomplete_and_never_automatically_selected()
    {
        var lookup = CoinCatalog.Seed.Lookup("penny", IssueYear.Known(1974));
        Assert.False(lookup.CoverageComplete);
        Assert.Single(lookup.Candidates);
        Assert.Equal("lincoln-memorial", lookup.Candidates[0].Design.Family.Value);
        var session = new CollectionSession(new MemoryStorage());
        session.Open();
        var coin = new CoinIdentity("penny", IssueYear.Known(1974), MintMark.Explicit("D"), lookup.Candidates[0].Design);
        Assert.Equal(AddStatus.ConfirmationRequired, session.Add(new(coin)).Status);
        Assert.Empty(session.Snapshot.Entries);
        Assert.Single(CoinCatalog.Seed.Lookup("penny", IssueYear.Unknown).Candidates);
    }
}

public class IdentityTests
{
    [Fact]
    public void Date_inscription_metadata_does_not_split_the_same_issue_year_and_design_identity()
    {
        var session = new CollectionSession(new MemoryStorage());
        session.Open();
        var design = new CoinDesign(DesignField.Known("bicentennial"), DesignField.NotApplicable, "1776–1976");
        var coin = new CoinIdentity("quarter", IssueYear.Known(1976), MintMark.None, design);
        session.Add(new(coin, 1, true));
        session.Add(new(coin with { Design = design with { DateInscription = null } }, 1, true));
        Assert.Single(session.Snapshot.Entries);
        Assert.Equal(2, session.Snapshot.Entries[0].Quantity);
        Assert.Equal("1776–1976", session.Snapshot.Entries[0].Coin.Design.DateInscription);
    }
}
