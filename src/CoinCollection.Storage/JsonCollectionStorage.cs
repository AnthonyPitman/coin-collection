using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Serialization;
using CoinCollection.Core;

namespace CoinCollection.Storage;

public sealed class JsonCollectionStorage : ICollectionStorage
{
    private readonly string path;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true,
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
        RespectRequiredConstructorParameters = true,
        Converters = { new JsonStringEnumConverter(allowIntegerValues: false) }
    };
    public JsonCollectionStorage(string path) => this.path = Path.GetFullPath(path);
    public LoadResult Load()
    {
        try
        {
            byte[] bytes;
            try { bytes = File.ReadAllBytes(path); }
            catch (FileNotFoundException) { return new(LoadStatus.Missing, Revision: StorageRevision.Missing); }
            catch (DirectoryNotFoundException) { return new(LoadStatus.Missing, Revision: StorageRevision.Missing); }
            var revision = Revision(bytes);
            using var document = JsonDocument.Parse(bytes);
            if (HasDuplicateProperties(document.RootElement)) return new(LoadStatus.Invalid, Error: "Duplicate JSON properties are not valid.");
            if (!document.RootElement.TryGetProperty("formatVersion", out var version) || !version.TryGetInt32(out var number))
                return new(LoadStatus.Invalid, Error: "A numeric formatVersion is required.");
            if (number != 1) return new(LoadStatus.UnsupportedVersion, Error: $"Unsupported format version {number}.");
            var data = JsonSerializer.Deserialize<CollectionDocument>(bytes, JsonOptions);
            if (data?.Entries is null) return new(LoadStatus.Invalid, Error: "Collection entries are required.");
            foreach (var entry in data.Entries)
            {
                var error = entry is null ? "Null collection entry." : CoinValidation.Validate(entry.Coin, entry.Quantity);
                if (error is not null) return new(LoadStatus.Invalid, Error: error);
            }
            if (data.Entries.GroupBy(e => e.Coin).Any(g => g.Count() > 1)) return new(LoadStatus.Invalid, Error: "Duplicate collection identities are not valid.");
            return new(LoadStatus.Loaded, new(data.Entries), revision);
        }
        catch (Exception exception) when (exception is JsonException or InvalidOperationException or FormatException or OverflowException)
        { return new(LoadStatus.Invalid, Error: exception.Message); }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException or System.Security.SecurityException)
        { return new(LoadStatus.ReadFailure, Error: exception.Message); }
    }
    public SaveResult Save(CollectionSnapshot snapshot, StorageRevision expectedRevision)
    {
        string? temporaryPath = null;
        try
        {
            foreach (var entry in snapshot.Entries)
            {
                var error = CoinValidation.Validate(entry.Coin, entry.Quantity);
                if (error is not null) return new(SaveStatus.WriteFailure, Error: error);
            }
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            using var coordination = AcquireSaveLock();
            var bytes = JsonSerializer.SerializeToUtf8Bytes(new CollectionDocument(1, snapshot.Entries.ToArray()), JsonOptions);
            temporaryPath = path + "." + Guid.NewGuid().ToString("N") + ".tmp";
            using (var file = new FileStream(temporaryPath, FileMode.CreateNew, FileAccess.Write, FileShare.None))
            {
                file.Write(bytes);
                file.Flush(flushToDisk: true);
            }
            var current = CurrentRevision();
            if (current != expectedRevision) return new(SaveStatus.Conflict, Error: "The collection changed since it was opened or saved. Pending changes were retained.");
            if (expectedRevision == StorageRevision.Missing) File.Move(temporaryPath, path, overwrite: false);
            else File.Move(temporaryPath, path, overwrite: true);
            return new(SaveStatus.Saved, Revision(bytes));
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException or System.Security.SecurityException or JsonException)
        { return new(SaveStatus.WriteFailure, Error: exception.Message); }
        finally
        {
            if (temporaryPath is not null)
            {
                try { File.Delete(temporaryPath); }
                catch (IOException) { }
                catch (UnauthorizedAccessException) { }
            }
        }
    }
    // The stable sidecar is never deleted: deleting it could split cooperating writers across locks.
    private FileStream AcquireSaveLock()
    {
        for (var attempt = 0; ; attempt++)
        {
            try { return new FileStream(path + ".lock", FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None); }
            catch (IOException) when (attempt < 20) { Thread.Sleep(50); }
        }
    }
    private StorageRevision CurrentRevision()
    {
        try { return Revision(File.ReadAllBytes(path)); }
        catch (FileNotFoundException) { return StorageRevision.Missing; }
        catch (DirectoryNotFoundException) { return StorageRevision.Missing; }
    }
    private static bool HasDuplicateProperties(JsonElement element)
    {
        if (element.ValueKind == JsonValueKind.Object)
        {
            var names = new HashSet<string>(StringComparer.Ordinal);
            foreach (var property in element.EnumerateObject())
                if (!names.Add(property.Name) || HasDuplicateProperties(property.Value)) return true;
        }
        else if (element.ValueKind == JsonValueKind.Array)
        {
            foreach (var item in element.EnumerateArray())
                if (HasDuplicateProperties(item)) return true;
        }

        return false;
    }
    private static StorageRevision Revision(byte[] bytes) => new(Convert.ToHexString(SHA256.HashData(bytes)));
    private sealed record CollectionDocument(int FormatVersion, CollectionEntry[] Entries);
}
