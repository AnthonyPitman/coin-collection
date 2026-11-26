using CoinCollection.Core;
using Xunit;

namespace CoinCollection.Storage.Tests;

public sealed class JsonStorageTests : IDisposable
{
    private readonly string directory = Path.Combine(Path.GetTempPath(), "coin-collection-tests", Guid.NewGuid().ToString("N"));
    private string CollectionPath => Path.Combine(directory, "collection.json");
    [Fact]
    public void Json_round_trip_preserves_unknowns_no_mark_explicit_P_and_unrecognized_designs()
    {
        var store = new JsonCollectionStorage(CollectionPath);
        Assert.Equal(LoadStatus.Missing, store.Load().Status);
        var entries = new[] {
            new CollectionEntry(new("penny", IssueYear.Known(1974), MintMark.Explicit("P"), new(DesignField.Known("owner-design"), DesignField.NotApplicable)), 2),
            new CollectionEntry(new("penny", IssueYear.Known(1974), MintMark.None, CoinDesign.Unknown), 1),
            new CollectionEntry(new("quarter", IssueYear.Unknown, MintMark.Unknown, new(DesignField.Known("state-quarters"), DesignField.Unknown)), 3) };
        var saved = store.Save(new(entries), StorageRevision.Missing);
        Assert.Equal(SaveStatus.Saved, saved.Status);
        var loaded = new JsonCollectionStorage(CollectionPath).Load();
        Assert.Equal(LoadStatus.Loaded, loaded.Status);
        Assert.Equal(entries, loaded.Snapshot!.Entries);
        Assert.Equal(saved.Revision, loaded.Revision);
        var json = File.ReadAllText(CollectionPath);
        Assert.Contains("\n", json);
        Assert.Contains("\"formatVersion\": 1", json);
        Assert.DoesNotContain("faceValue", json);
    }
    [Theory]
    [InlineData("not json", LoadStatus.Invalid)]
    [InlineData("{\"formatVersion\":2,\"entries\":[]}", LoadStatus.UnsupportedVersion)]
    [InlineData("{\"formatVersion\":1}", LoadStatus.Invalid)]
    [InlineData("{\"entries\":[]}", LoadStatus.Invalid)]
    [InlineData("{\"formatVersion\":1,\"entries\":null}", LoadStatus.Invalid)]
    [InlineData("{\"formatVersion\":1,\"entries\":[null]}", LoadStatus.Invalid)]
    [InlineData("{\"formatVersion\":1,\"entries\":[],\"entries\":[]}", LoadStatus.Invalid)]
    public void Invalid_or_unsupported_existing_file_remains_byte_for_byte_unchanged(string json, LoadStatus status)
    {
        Directory.CreateDirectory(directory);
        File.WriteAllText(CollectionPath, json);
        var before = File.ReadAllBytes(CollectionPath);
        var session = new CollectionSession(new JsonCollectionStorage(CollectionPath));
        Assert.Equal(status, session.Open().Status);
        Assert.Equal(before, File.ReadAllBytes(CollectionPath));
        var coin = new CoinIdentity("penny", IssueYear.Unknown, MintMark.None, CoinDesign.Unknown);
        Assert.Equal(AddStatus.NotOpen, session.Add(new(coin, 1, true)).Status);
        Assert.Equal(before, File.ReadAllBytes(CollectionPath));
    }

    [Fact]
    public async Task Cooperating_writers_with_same_revision_cannot_both_replace_changed_content()
    {
        var store = new JsonCollectionStorage(CollectionPath);
        var initial = store.Save(new(), StorageRevision.Missing);
        var penny = new CoinIdentity("penny", IssueYear.Known(1974), MintMark.None, CoinDesign.Unknown);
        using var gate = new Barrier(2);
        Task<SaveResult> Save(int quantity) => Task.Run(() =>
        {
            gate.SignalAndWait();
            return new JsonCollectionStorage(CollectionPath).Save(new([new(penny, quantity)]), initial.Revision!);
        });
        var results = await Task.WhenAll(Save(1), Save(2));
        Assert.Single(results, result => result.Status == SaveStatus.Saved);
        Assert.Single(results, result => result.Status == SaveStatus.Conflict);
        Assert.Equal(LoadStatus.Loaded, store.Load().Status);
    }

    [Fact]
    public void Deletion_or_unexpected_creation_is_a_conflict()
    {
        var store = new JsonCollectionStorage(CollectionPath);
        var initial = store.Save(new(), StorageRevision.Missing);
        Assert.Equal(SaveStatus.Conflict, store.Save(new(), StorageRevision.Missing).Status);
        File.Delete(CollectionPath);
        Assert.Equal(SaveStatus.Conflict, store.Save(new(), initial.Revision!).Status);
        Assert.False(File.Exists(CollectionPath));
    }

    [Fact]
    public void Failed_replacement_keeps_previous_document_and_cleans_temporary_write()
    {
        // Windows' sharing rules let a reader prevent replacement while allowing revision reads.
        if (!OperatingSystem.IsWindows()) return;
        var store = new JsonCollectionStorage(CollectionPath);
        var saved = store.Save(new(), StorageRevision.Missing);
        var before = File.ReadAllBytes(CollectionPath);
        using (var reader = new FileStream(CollectionPath, FileMode.Open, FileAccess.Read, FileShare.Read))
        {
            var coin = new CoinIdentity("penny", IssueYear.Unknown, MintMark.None, CoinDesign.Unknown);
            Assert.Equal(SaveStatus.WriteFailure, store.Save(new([new(coin, 1)]), saved.Revision!).Status);
        }
        Assert.Equal(before, File.ReadAllBytes(CollectionPath));
        Assert.Empty(Directory.GetFiles(directory, "*.tmp"));
    }
    public void Dispose()
    {
        var fullPath = Path.GetFullPath(directory);
        var allowed = Path.GetFullPath(Path.Combine(Path.GetTempPath(), "coin-collection-tests")) + Path.DirectorySeparatorChar;
        if (!fullPath.StartsWith(allowed, StringComparison.OrdinalIgnoreCase)) throw new InvalidOperationException("Unexpected temporary path.");
        if (Directory.Exists(fullPath)) Directory.Delete(fullPath, true);
    }
}

public sealed class ConflictTests
{
    [Fact]
    public void Stale_save_refuses_overwrite_and_session_keeps_pending_coin_until_retry()
    {
        var directory = Path.Combine(Path.GetTempPath(), "coin-collection-tests", Guid.NewGuid().ToString("N"));
        var path = Path.Combine(directory, "collection.json");
        var first = new CollectionSession(new JsonCollectionStorage(path));
        Assert.Equal(LoadStatus.Created, first.Open().Status);
        var second = new CollectionSession(new JsonCollectionStorage(path));
        second.Open();
        var penny = new CoinIdentity("penny", IssueYear.Known(1974), MintMark.Explicit("D"), CoinDesign.Unknown);
        first.Add(new(penny, 1, true));
        var before = File.ReadAllBytes(path);
        var result = second.Add(new(penny, 2, true));
        Assert.Equal(AddStatus.AppliedButUnsaved, result.Status);
        Assert.Equal(SaveStatus.Conflict, result.Save!.Status);
        Assert.True(second.HasPendingChanges);
        Assert.Equal(2, second.Summary.CoinCount);
        Assert.Equal(SaveStatus.Conflict, second.RetrySave().Status);
        Assert.Equal(before, File.ReadAllBytes(path));
        Assert.Equal(LoadStatus.PendingChanges, second.Open().Status);
        Assert.Equal(AddStatus.PendingChanges, second.Add(new(penny, 1, true)).Status);
        File.Delete(path);
        File.Delete(path + ".lock");
        Directory.Delete(directory);
    }
}

