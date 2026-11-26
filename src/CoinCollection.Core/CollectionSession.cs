namespace CoinCollection.Core;

public sealed record StorageRevision(string? Token)
{
    public static StorageRevision Missing { get; } = new((string?)null);
}
public enum LoadStatus { Loaded, Missing, Created, Invalid, UnsupportedVersion, ReadFailure, InitializationFailure, PendingChanges }
public enum SaveStatus { Saved, Conflict, WriteFailure }
public sealed record LoadResult(LoadStatus Status, CollectionSnapshot? Snapshot = null, StorageRevision? Revision = null, string? Error = null);
public sealed record SaveResult(SaveStatus Status, StorageRevision? Revision = null, string? Error = null);
public interface ICollectionStorage
{
    LoadResult Load();
    SaveResult Save(CollectionSnapshot snapshot, StorageRevision expectedRevision);
}
public sealed record CollectionEntry(CoinIdentity Coin, int Quantity);
public sealed record CollectionSnapshot
{
    public IReadOnlyList<CollectionEntry> Entries { get; }
    public CollectionSnapshot(IEnumerable<CollectionEntry>? entries = null) => Entries = Array.AsReadOnly((entries ?? []).ToArray());
}
public sealed record DenominationSummary(string Denomination, long CoinCount, decimal FaceValue);
public sealed record CollectionSummary(IReadOnlyList<DenominationSummary> Rows, long CoinCount, decimal FaceValue);
public enum AddStatus { AppliedAndSaved, AppliedButUnsaved, Invalid, ConfirmationRequired, PendingChanges, NotOpen }
public sealed record AddRequest(CoinIdentity Coin, int Quantity = 1, bool ConfirmUnrecognized = false);
public sealed record AddResult(AddStatus Status, string? Error = null, SaveResult? Save = null, int? ResultingQuantity = null);

public sealed class CollectionSession(ICollectionStorage storage)
{
    private StorageRevision revision = StorageRevision.Missing;
    private bool opened;
    public CollectionSnapshot Snapshot { get; private set; } = new();
    public bool HasPendingChanges { get; private set; }
    public CollectionSummary Summary
    {
        get
        {
            var rows = CoinTypes.Denominations.Select(d =>
            {
                var count = Snapshot.Entries.Where(e => CoinTypes.Find(e.Coin.CoinTypeId)?.Denomination == d.Name).Sum(e => (long)e.Quantity);
                return new DenominationSummary(d.Name, count, count * d.FaceValue);
            }).ToArray();
            return new(Array.AsReadOnly(rows), rows.Sum(r => r.CoinCount), rows.Sum(r => r.FaceValue));
        }
    }
    public LoadResult Open()
    {
        if (HasPendingChanges) return new(LoadStatus.PendingChanges, Error: "Save pending changes before reopening.");
        var result = storage.Load();
        if (result.Status == LoadStatus.Missing)
        {
            Snapshot = new();
            revision = StorageRevision.Missing;
            var save = storage.Save(Snapshot, revision);
            if (save.Status != SaveStatus.Saved) return new(LoadStatus.InitializationFailure, Error: save.Error ?? save.Status.ToString());
            opened = true;
            revision = save.Revision!;
            return new(LoadStatus.Created, Snapshot, revision);
        }
        opened = result.Status == LoadStatus.Loaded;
        if (opened) { Snapshot = result.Snapshot!; revision = result.Revision!; }
        return result;
    }
    public AddResult Add(AddRequest request)
    {
        if (!opened) return new(AddStatus.NotOpen, "Open a collection first.");
        if (HasPendingChanges) return new(AddStatus.PendingChanges, "Save pending changes before adding another coin.");
        var error = CoinValidation.Validate(request.Coin, request.Quantity);
        if (error is not null) return new(AddStatus.Invalid, error);
        if (!request.ConfirmUnrecognized) return new(AddStatus.ConfirmationRequired, "This small reference seed cannot verify this combination. Confirm it before adding.");
        var entries = Snapshot.Entries.ToList();
        var index = entries.FindIndex(e => e.Coin == request.Coin);
        var oldQuantity = index < 0 ? 0 : entries[index].Quantity;
        if (request.Quantity > int.MaxValue - oldQuantity) return new(AddStatus.Invalid, "Resulting quantity exceeds the supported maximum.");
        var quantity = oldQuantity + request.Quantity;
        if (index < 0) entries.Add(new(request.Coin, quantity));
        else entries[index] = new(request.Coin, quantity);
        Snapshot = new(entries);
        HasPendingChanges = true;
        var save = RetrySave();
        return new(save.Status == SaveStatus.Saved ? AddStatus.AppliedAndSaved : AddStatus.AppliedButUnsaved, save.Error, save, quantity);
    }
    public SaveResult RetrySave()
    {
        if (!opened) return new(SaveStatus.WriteFailure, Error: "Open a collection first.");
        if (!HasPendingChanges) return new(SaveStatus.Saved, revision);
        var result = storage.Save(Snapshot, revision);
        if (result.Status == SaveStatus.Saved) { revision = result.Revision!; HasPendingChanges = false; }
        return result;
    }
}
