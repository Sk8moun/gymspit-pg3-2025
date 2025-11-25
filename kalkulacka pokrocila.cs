using System;
using System.Globalization;

void PrintMenu()
{
    Console.WriteLine();
    Console.WriteLine("Vyberte operaci:");
    Console.WriteLine("  +    sčítání");
    Console.WriteLine("  -    odčítání");
    Console.WriteLine("  *    násobení");
    Console.WriteLine("  /    dělení");
    Console.WriteLine("  konec  ukončí program");
    Console.Write("Volba: ");
}

char ReadOperation()
{
    while (true)
    {
        var input = Console.ReadLine()?.Trim() ?? string.Empty;

        if (string.Equals(input, "konec", StringComparison.OrdinalIgnoreCase))
            return 'k'; // 'k' znamená konec

        if (input.Length == 1)
        {
            char c = input[0];
            if (c == '+' || c == '-' || c == '*' || c == '/')
                return c;
        }

        Console.Write("Neplatná volba. Zadejte +, -, *, / nebo konec: ");
    }
}

double ReadDouble(bool nonZero = false)
{
    while (true)
    {
        var input = Console.ReadLine()?.Trim() ?? string.Empty;

        // Pokusíme se přijmout jak českou (čárka), tak anglickou (tečka) desetinnou čárku.
        if (double.TryParse(input, NumberStyles.Float, CultureInfo.CurrentCulture, out var value)
            || double.TryParse(input.Replace(',', '.'), NumberStyles.Float, CultureInfo.InvariantCulture, out value))
        {
            if (nonZero && Math.Abs(value) < double.Epsilon)
            {
                Console.Write("Číslo nesmí být nula. Zadejte prosím jiné číslo: ");
                continue;
            }

            return value;
        }

        Console.Write("Neplatný vstup. Zadejte prosím číslo: ");
    }
}

double Compute(char operace, double cislo1, double cislo2)
{
    return operace switch
    {
        '+' => cislo1 + cislo2,
        '-' => cislo1 - cislo2,
        '*' => cislo1 * cislo2,
        '/' => cislo1 / cislo2,
        _ => throw new InvalidOperationException("Neznámá operace")
    };
}

void PrintResult(char operace, double cislo1, double cislo2, double vysledek)
{
    // Vypíšeme výsledek v souladu s aktuální kulturou
    Console.WriteLine();
    Console.WriteLine($"{cislo1} {operace} {cislo2} = {vysledek}");
}

Console.WriteLine("Jednoduchá kalkulačka s funkcemi. Pro ukončení zadejte 'konec' jako operaci.");

while (true)
{
    PrintMenu();
    var op = ReadOperation();
    if (op == 'k')
    {
        Console.WriteLine("Konec programu. Nashledanou.");
        break;
    }

    Console.Write("Zadejte první číslo: ");
    double a = ReadDouble();

    Console.Write("Zadejte druhé číslo: ");
    // Pokud je operace dělení, druhé číslo nesmí být nula
    double b = ReadDouble(nonZero: op == '/');

    double result = Compute(op, a, b);
    PrintResult(op, a, b, result);
}
