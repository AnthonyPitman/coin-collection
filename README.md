# coin-collection

An offline, interactive US coin collection tracker. This first slice supports guided additions of one coin at a time, denomination summaries, and automatic native JSON saves.

## Run

Install the .NET 10 SDK, then from the repository root:

```powershell
cd src
dotnet run --project CoinCollectionApp
```

The default collection is `<LocalApplicationData>/coin-collection/collection.json`, resolved with .NET's per-user local application data API on Windows and Linux. The active absolute path is printed at startup. A missing collection is created; failures never fall back to another directory.

Choose a separate native collection with:

```powershell
dotnet run --project CoinCollectionApp -- --file "C:\path\my-coins.json"
```

Select Add, choose one of the ten coin types, enter an issue year (or `unknown`), and enter the observed mint mark. An empty mint-mark response means **no mark**; `unknown` is separate from no mark and explicit `P`. Choose a suggested design or explicit unknown, then confirm. Matching entries combine quantities. Dollar types share one denomination summary row. Successful additions save automatically. Type `cancel` during guided entry to return without adding that coin.

The bundled reference seed intentionally contains just the 1974 penny / Lincoln Memorial example, with no mint attribution or claim of complete coverage. Every addition requires confirmation, including the sole suggestion. Other combinations remain recordable with an unknown design. The seed has its own `seed-1` revision, separate from the JSON format version. See [catalog research and provenance](docs/research/offline-coin-catalog.md).

## Native file and errors

Version 1 JSON is indented and contains `formatVersion` and `entries`. Entries preserve coin type, known/unknown issue year, explicit/no/unknown mint mark, design family/subject states, and positive whole-number quantity. Unknown and not-applicable design fields remain distinct. Totals are derived. Loading never rewrites entries based on catalog recognition; invalid or unsupported files produce an error and remain unchanged.

Saves finish and flush a temporary file beside the destination before replacement. A stable `<collection-path>.lock` sidecar coordinates cooperating app instances; leave this file in place. The expected revision is a hash of the last loaded/saved bytes. Detected changes, deletion, and unexpected creation produce conflicts instead of silent overwrite. An uncooperative external editor can still race after the revision check; this is not a universal filesystem transaction.

After a failed save, the addition remains pending in memory. Retry saves that snapshot without applying the addition twice. Further additions are blocked while a save is pending. Exit requires explicitly typing `DISCARD` to discard pending changes. If input ends while changes are pending, the CLI reports the unsaved entries and exits with a failure status. Full overwrite/save-elsewhere recovery choices arrive in a later slice.

Quantity prompts, browsing/removal, full offline catalog/date normalization, quick entry, and full save recovery are subsequent issues #12–#15. CSV exchange, editing characteristics, GUI, direct add/remove/list commands, and international support are outside this slice.

## Build and verify

From `src`:

```powershell
dotnet build CoinCollection.slnx
dotnet test CoinCollection.slnx
```

Core tests exercise the public collection session, identity/quantity operations, summaries, validation, incomplete-catalog confirmation, and save retries through a controlled storage boundary. Storage integration tests use temporary files for JSON round trips, invalid/unsupported data preservation, revision conflicts, cooperating writers, and failed replacement. The replacement-sharing test applies to Windows sharing semantics.

For a scripted CLI check, pipe these lines into the built app (or enter them interactively):

```text
a
1
1974
D
0
y
n
e
```

Run with a new `--file` path: the sequence adds one 1974 D penny with unknown design, reports $0.01, saves, and exits. Start again with the same path and choose `e`; the summary must still show one penny and $0.01. Selecting `1` at the design prompt instead chooses the Lincoln Memorial suggestion, with explicit confirmation still required.
