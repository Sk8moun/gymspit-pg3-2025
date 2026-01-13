using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

partial class Program
{
    const string DataFileName = "penezenka.txt"; // cesta napevno (v adresáři aplikace)
    const int MaxItems = 1000;

    // jednoduchý záznam položky
    public record Item(int Value, string Name, string Category);

    // sdílený seznam položek
    public static List<Item> items = new();

    static void Main()
    {
        LoadFromFile();

        while (true) // hlavní smyčka menu
        {
            Console.WriteLine("=== PENĚŽENKA ===");
            Console.WriteLine("1) Vypsat záznamy");
            Console.WriteLine("2) Přidat položku");
            Console.WriteLine("3) Upravit položku");
            Console.WriteLine("4) Smazat položku");
            Console.WriteLine("5) Statistiky");
            Console.WriteLine("6) Výpis s filtrem");
            Console.WriteLine("7) Kategorie (součty)");
            Console.WriteLine("8) Řazení a zobrazení (neměnit pořadí)");
            Console.WriteLine("9) Uložit do souboru");
            Console.WriteLine("0) Uložit a konec");
            Console.Write("Volba: ");
            var choice = Console.ReadLine()?.Trim();

            switch (choice) // volba menu
            {
                case "1":
                    ListItems();
                    break;
                case "2":
                    AddItem();
                    break;
                case "3":
                    EditItem();
                    break;
                case "4":
                    DeleteItem();
                    break;
                case "5":
                    ShowStatistics();
                    break;
                case "6":
                    FilteredList();
                    break;
                case "7":
                    ShowCategorySums();
                    break;
                case "8":
                    SortAndDisplay();
                    break;
                case "9":
                    SaveToFile();
                    break;
                case "0":
                    SaveToFile();
                    return;
                default:
                    Console.WriteLine("Neplatná volba.");
                    break;
            }
        }
    }

    static void LoadFromFile()
    {
        items.Clear();
        var path = Path.Combine(AppContext.BaseDirectory, DataFileName);
        if (!File.Exists(path))
        {
            Console.WriteLine($"Soubor '{DataFileName}' nenalezen — začínám s prázdným seznamem.");
            return;
        }

        var lines = File.ReadAllLines(path);
        int i = 0;
        while (i < lines.Length)
        {
            // Hledáme řádek, který je parsovatelný jako int (hodnota)
            var valLine = lines[i].Trim();
            if (!int.TryParse(valLine, out var value))
            {
                // pokud není parsovatelné, přeskočíme tento řádek
                i++;
                continue;
            }
            i++;
            if (i >= lines.Length)
                break; // chybí název

            var name = lines[i].Trim();
            i++;

            string category = "";
            // pokud existuje další řádek a NENÍ parsovatelný jako int, považujeme ho za kategorii
            if (i < lines.Length && !int.TryParse(lines[i].Trim(), out _))
            {
                category = lines[i].Trim();
                i++;
            }

            if (items.Count < MaxItems)
                items.Add(new Item(value, name, category));
        }

        Console.WriteLine($"Načteno {items.Count} položek ze '{DataFileName}'.");
    }

    static void SaveToFile() // uloží všechny položky do souboru
    {
        var path = Path.Combine(AppContext.BaseDirectory, DataFileName);
        using var w = new StreamWriter(path, false);
        // Formát: pro každou položku uložíme 2 nebo 3 řádky: hodnota, název, kategorie (pokud není prázdná)
        foreach (var it in items)
        {
            w.WriteLine(it.Value);
            w.WriteLine(it.Name ?? "");
            if (!string.IsNullOrEmpty(it.Category))
                w.WriteLine(it.Category);
        }
        Console.WriteLine($"Uloženo {items.Count} položek do '{DataFileName}'.");
    }

    static void ListItems(IEnumerable<Item>? source = null, bool showTotalsForAll = true)
    {
        var list = source?.ToList() ?? items;
        if (list.Count == 0)
        {
            Console.WriteLine("Žádné položky k zobrazení.");
            return;
        }

        // Hlavička a pevné šířky sloupců
        Console.WriteLine();
        Console.WriteLine("{0,3} {1,10} {2,-30} {3,-15} {4,12}", "č.", "hodnota", "popis", "kategorie", "zůstatek");
        Console.WriteLine(new string('-', 75));

        int running = 0;
        int idx = 1;
        foreach (var it in list)
        {
            running += it.Value;
            Console.WriteLine("{0,3} {1,10} {2,-30} {3,-15} {4,12}", idx, it.Value, Truncate(it.Name, 30), Truncate(it.Category, 15), running);
            idx++;
        }

        Console.WriteLine(new string('-', 75));
        // Statistiky: buď pro všechny nebo pro zobrazené (volitelné)
        if (showTotalsForAll)
            ShowStatisticsInternal(items);
        else
            ShowStatisticsInternal(list);
    }

    static void AddItem()
    {
        if (items.Count >= MaxItems)
        {
            Console.WriteLine($"Nelze přidat — dosaženo maxima položek ({MaxItems}).");
            return;
        }
        int value = ReadInt("Zadej hodnotu (kladné = příjem, záporné = výdaj): ");
        Console.Write("Zadej název: ");
        var name = Console.ReadLine()?.Trim() ?? "";
        Console.Write("Zadej kategorii (může být prázdná): ");
        var cat = Console.ReadLine()?.Trim() ?? "";
        items.Add(new Item(value, name, cat));
        Console.WriteLine("Položka přidána.");
    }

    static void EditItem()
    {
        if (items.Count == 0)
        {
            Console.WriteLine("Žádné položky k editaci.");
            return;
        }
        ListItems();
        int index = ReadInt($"Zadej číslo řádku k úpravě (1..{items.Count}): ", 1, items.Count) - 1;
        var old = items[index];
        Console.WriteLine($"Původně: hodnota={old.Value}, název='{old.Name}', kategorie='{old.Category}'");
        int newValue = ReadInt("Nová hodnota: ");
        Console.Write("Nový název: ");
        var newName = Console.ReadLine()?.Trim() ?? "";
        Console.Write("Nová kategorie: ");
        var newCat = Console.ReadLine()?.Trim() ?? "";
        items[index] = new Item(newValue, newName, newCat);
        Console.WriteLine("Položka upravena.");
    }

    static void DeleteItem()
    {
        if (items.Count == 0)
        {
            Console.WriteLine("Žádné položky ke smazání.");
            return;
        }
        ListItems();
        int index = ReadInt($"Zadej číslo řádku ke smazání (1..{items.Count}): ", 1, items.Count) - 1;
        var removed = items[index];
        items.RemoveAt(index);
        Console.WriteLine($"Položka smazána: {removed.Value} {removed.Name}");
    }

    static void ShowStatistics()
    {
        if (items.Count == 0)
        {
            Console.WriteLine("Žádné položky — žádná statistika.");
            return;
        }
        ShowStatisticsInternal(items);
    }

    static void ShowStatisticsInternal(IEnumerable<Item> list)
    {
        var all = list.ToList();
        long sum = all.Sum(i => (long)i.Value);
        int incomesCount = all.Count(i => i.Value > 0);
        int expensesCount = all.Count(i => i.Value < 0);

        int? maxIncome = all.Where(i => i.Value > 0).Select(i => (int?)i.Value).DefaultIfEmpty(null).Max();
        int? minIncome = all.Where(i => i.Value > 0).Select(i => (int?)i.Value).DefaultIfEmpty(null).Min();
        int? maxExpense = all.Where(i => i.Value < 0).Select(i => (int?)i.Value).DefaultIfEmpty(null).Max();
        int? minExpense = all.Where(i => i.Value < 0).Select(i => (int?)i.Value).DefaultIfEmpty(null).Min();

        Console.WriteLine($"Součet všech hodnot: {sum}");
        Console.WriteLine($"Počet příjmů (>0): {incomesCount}");
        Console.WriteLine($"Počet výdajů (<0): {expensesCount}");
        Console.WriteLine($"Největší příjem: {(maxIncome.HasValue ? maxIncome.Value.ToString() : "N/A")}");
        Console.WriteLine($"Nejmenší příjem: {(minIncome.HasValue ? minIncome.Value.ToString() : "N/A")}");
        Console.WriteLine($"Největší výdaj: {(minExpense.HasValue ? minExpense.Value.ToString() : "N/A")}"); // největší výdaj = nejmenší (nejvíce záporné) číselně = minExpense
        Console.WriteLine($"Nejmenší výdaj: {(maxExpense.HasValue ? maxExpense.Value.ToString() : "N/A")}");
    }

    static void FilteredList()
    {
        Console.Write("Zadej hledaný řetězec (vyhledává se v popisku, case-insensitive): ");
        var q = Console.ReadLine() ?? "";
        var filtered = items.Where(i => i.Name.IndexOf(q, StringComparison.OrdinalIgnoreCase) >= 0).ToList();
        Console.WriteLine($"Zobrazuji položky obsahující '{q}' ({filtered.Count} z {items.Count}).");
        // podle zadání: vypisujeme pouze vyhovující položky, ale součty počítáme i přes vynechané řádky (tj. totals zůstanou z celého seznamu)
        ListItems(filtered, showTotalsForAll: true);
    }

    static void ShowCategorySums()
    {
        if (items.Count == 0)
        {
            Console.WriteLine("Žádné položky.");
            return;
        }
        var dict = new Dictionary<string, long>(StringComparer.OrdinalIgnoreCase);
        foreach (var it in items)
        {
            var cat = string.IsNullOrEmpty(it.Category) ? "(bez kategorie)" : it.Category;
            if (!dict.ContainsKey(cat)) dict[cat] = 0;
            dict[cat] += it.Value;
        }

        Console.WriteLine("Součty podle kategorií:");
        foreach (var kv in dict.OrderByDescending(k => Math.Abs(k.Value)))
        {
            Console.WriteLine("{0,-25} {1,12}", Truncate(kv.Key, 25), kv.Value);
        }
    }

    static void SortAndDisplay()
    {
        if (items.Count == 0)
        {
            Console.WriteLine("Žádné položky k seřazení.");
            return;
        }
        Console.WriteLine("Řadit podle: 1) hodnoty  2) popisku");
        var field = Console.ReadLine()?.Trim();
        Console.WriteLine("Pořadí: 1) vzestupně  2) sestupně");
        var order = Console.ReadLine()?.Trim();

        var arr = items.ToArray(); // klonujeme pole (neměníme pracovní data)
        Comparison<Item> cmp = (a, b) => 0;
        if (field == "1")
            cmp = (a, b) => a.Value.CompareTo(b.Value);
        else
            cmp = (a, b) => string.Compare(a.Name, b.Name, StringComparison.OrdinalIgnoreCase);

        Array.Sort(arr, cmp);
        if (order == "2")
            Array.Reverse(arr);

        Console.WriteLine("Seřazený výpis (originální seznam zůstává beze změny):");
        ListItems(arr, showTotalsForAll: true);
    }

    static int ReadInt(string prompt, int? min = null, int? max = null)
    {
        while (true)
        {
            Console.Write(prompt);
            var s = Console.ReadLine();
            if (int.TryParse(s, out var v))
            {
                if (min.HasValue && v < min.Value)
                {
                    Console.WriteLine($"Hodnota musí být >= {min.Value}.");
                    continue;
                }
                if (max.HasValue && v > max.Value)
                {
                    Console.WriteLine($"Hodnota musí být <= {max.Value}.");
                    continue;
                }
                return v;
            }
            Console.WriteLine("Neplatné číslo (int). Zkus znovu.");
        }
    }

    static string Truncate(string? s, int max)
    {
        if (string.IsNullOrEmpty(s)) return "";
        return s.Length <= max ? s : s.Substring(0, max - 3) + "...";
    }
}
