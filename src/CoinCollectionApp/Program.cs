using System.Globalization;
using CoinCollection.Core;
using CoinCollection.Storage;

namespace CoinCollectionApp;

internal static class Program
{
    private readonly static CultureInfo UsCulture = CultureInfo.GetCultureInfo("en-US");
    public static int Main(string[] args)
    {
        if (args is ["--help"] or ["-h"])
        {
            Console.WriteLine("Usage: coin-collection [--file <path>]");
            return 0;
        }
        if (args.Length != 0 && (args.Length != 2 || args[0] != "--file"))
        {
            Console.Error.WriteLine("Usage: coin-collection [--file <path>]");
            return 1;
        }
        try
        {
            var path = args.Length == 2 ? Path.GetFullPath(args[1]) : DefaultPath();
            Console.WriteLine($"Collection file: {path}");
            var session = new CollectionSession(new JsonCollectionStorage(path));
            var loaded = session.Open();

            if (loaded.Status is LoadStatus.Loaded or LoadStatus.Created)
            {
                return Run(session);
            }

            Console.Error.WriteLine($"Could not open collection ({loaded.Status}): {loaded.Error}");
            return 1;
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException or ArgumentException or NotSupportedException or System.Security.SecurityException)
        {
            Console.Error.WriteLine($"Could not open collection: {exception.Message}");
            return 1;
        }
    }
    private static string DefaultPath()
    {
        var directory = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        if (string.IsNullOrWhiteSpace(directory)) throw new IOException("The per-user local application data directory could not be resolved.");
        return Path.Combine(directory, "coin-collection", "collection.json");
    }
    private static int Run(CollectionSession session)
    {
        while (true)
        {
            ShowSummary(session);
            Console.WriteLine(session.HasPendingChanges ? "Unsaved changes: [R]etry save  [E]xit" : "[A]dd  [E]xit");
            var choice = Read("Choice: ");
            if (choice is null) return EndOfInput(session);
            switch (choice.ToLowerInvariant())
            {
                case "a" or "add" when !session.HasPendingChanges:
                    if (!Add(session)) return EndOfInput(session);
                    break;
                case "r" or "retry" when session.HasPendingChanges:
                    ShowSave(session.RetrySave());
                    break;
                case "e" or "exit":
                    if (!session.HasPendingChanges) return 0;
                    var discard = Read("Changes are NOT saved. Type DISCARD to exit and discard them, or press Enter to return: ");
                    if (discard is null) return EndOfInput(session);
                    if (discard == "DISCARD") { Console.WriteLine("Discarded unsaved changes."); return 0; }
                    break;
                default:
                    Console.WriteLine("Choose one of the displayed options.");
                    break;
            }
        }
    }
    private static void ShowSummary(CollectionSession session)
    {
        var summary = session.Summary;
        Console.WriteLine();
        if (summary.CoinCount == 0) Console.WriteLine("No coins yet.");
        if (session.HasPendingChanges) Console.WriteLine("Summary includes UNSAVED changes.");
        foreach (var row in summary.Rows)
            Console.WriteLine($"{row.Denomination,-12} {row.CoinCount,8}  {row.FaceValue.ToString("C2", UsCulture),12}");
        Console.WriteLine($"{"Total",-12} {summary.CoinCount,8}  {summary.FaceValue.ToString("C2", UsCulture),12}");
    }
    private static bool Add(CollectionSession session)
    {
        Console.WriteLine("Choose a coin type (or type cancel):");
        for (var i = 0; i < CoinTypes.All.Count; i++) Console.WriteLine($"{i + 1}. {CoinTypes.All[i].Name}");
        CoinType type;
        while (true)
        {
            var selection = Read("Coin type: ");
            if (selection is null) return false;
            if (IsCancel(selection)) return true;
            if (int.TryParse(selection, NumberStyles.None, CultureInfo.InvariantCulture, out var index) && index >= 1 && index <= CoinTypes.All.Count)
            { type = CoinTypes.All[index - 1]; break; }
            Console.WriteLine("Choose a number from the coin types shown.");
        }
        while (true)
        {
            IssueYear year;
            while (true)
            {
                var input = Read($"{type.Name} issue year (1–9999, unknown, or cancel): ");
                if (input is null) return false;
                if (IsCancel(input)) return true;
                if (input.Equals("unknown", StringComparison.OrdinalIgnoreCase) || input == "?") { year = IssueYear.Unknown; break; }
                if (int.TryParse(input, NumberStyles.None, CultureInfo.InvariantCulture, out var value) && value is >= 1 and <= 9999) { year = IssueYear.Known(value); break; }
                Console.WriteLine("Enter an issue year or unknown.");
            }
            MintMark mint;
            while (true)
            {
                var input = Read("Mint mark (letters; Enter = no mark; unknown; cancel): ");
                if (input is null) return false;
                if (IsCancel(input)) return true;
                mint = input.Length == 0 || input.Equals("none", StringComparison.OrdinalIgnoreCase) ? MintMark.None
                    : input.Equals("unknown", StringComparison.OrdinalIgnoreCase) || input == "?" ? MintMark.Unknown
                    : MintMark.Explicit(input.ToUpperInvariant());
                var error = CoinValidation.Validate(new(type.Id, year, mint, CoinDesign.Unknown), 1);
                if (error is null) break;
                Console.WriteLine(error);
            }
            var lookup = CoinCatalog.Seed.Lookup(type.Id, year);
            Console.WriteLine("Reference seed is incomplete. Suggestions require confirmation; no mint is inferred.");
            Console.WriteLine("0. Unknown design");
            for (var i = 0; i < lookup.Candidates.Count; i++) Console.WriteLine($"{i + 1}. {lookup.Candidates[i].Name} (suggestion)");
            CoinDesign design;
            while (true)
            {
                var input = Read("Design (number, or cancel): ");
                if (input is null) return false;
                if (IsCancel(input)) return true;
                if (int.TryParse(input, NumberStyles.None, CultureInfo.InvariantCulture, out var index) && index >= 0 && index <= lookup.Candidates.Count)
                { design = index == 0 ? CoinDesign.Unknown : lookup.Candidates[index - 1].Design; break; }
                Console.WriteLine("Choose one of the displayed design numbers.");
            }
            var coin = new CoinIdentity(type.Id, year, mint, design);
            var validation = session.Add(new(coin));
            if (validation.Status != AddStatus.ConfirmationRequired) { Console.WriteLine(validation.Error); return true; }
            Console.WriteLine(validation.Error);
            var confirmation = Read($"Add one {Describe(coin)}? [y/N]: ");
            if (confirmation is null) return false;
            if (!confirmation.Equals("y", StringComparison.OrdinalIgnoreCase)) return true;
            var result = session.Add(new(coin, 1, true));
            if (result.Status == AddStatus.AppliedAndSaved)
                Console.WriteLine($"Added one {Describe(coin)}. Entry quantity: {result.ResultingQuantity}. Saved.");
            else if (result.Status == AddStatus.AppliedButUnsaved)
            {
                Console.WriteLine($"Added in memory only. Entry quantity: {result.ResultingQuantity}.");
                ShowSave(result.Save!);
                return true;
            }
            else { Console.WriteLine(result.Error); return true; }
            var another = Read($"Add another {type.Name}? [y/N]: ");
            if (another is null) return false;
            if (!another.Equals("y", StringComparison.OrdinalIgnoreCase)) return true;
        }
    }
    private static void ShowSave(SaveResult result) => Console.WriteLine(result.Status == SaveStatus.Saved
        ? "Saved." : $"Save failed ({result.Status}): {result.Error} Changes remain unsaved; retry or explicitly discard at exit.");
    private static int EndOfInput(CollectionSession session)
    {
        if (!session.HasPendingChanges) return 0;
        Console.Error.WriteLine("Input ended with UNSAVED changes. They were not written to the collection file. This run's entries:");
        foreach (var entry in session.Snapshot.Entries) Console.Error.WriteLine($"{entry.Quantity} × {Describe(entry.Coin)}");
        return 1;
    }
    private static string Describe(CoinIdentity coin)
    {
        var year = coin.Year.State == ValueState.Known ? coin.Year.Value!.Value.ToString(CultureInfo.InvariantCulture) : "unknown year";
        var mint = coin.MintMark.State switch { MintMarkState.Explicit => coin.MintMark.Mark, MintMarkState.None => "no mark", _ => "unknown mark" };
        var design = coin.Design.Family.State == ValueState.Known ? coin.Design.Family.Value : "unknown design";
        return $"{year} {mint} {CoinTypes.Find(coin.CoinTypeId)!.Name} / {design}";
    }
    private static bool IsCancel(string input) => input.Equals("cancel", StringComparison.OrdinalIgnoreCase);
    private static string? Read(string prompt) { Console.Write(prompt); return Console.ReadLine()?.Trim(); }
}
