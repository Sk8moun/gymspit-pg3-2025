Console.WriteLine("Vítejte v kalkulačce!");
Console.WriteLine("Operace: +, -, *, / | 'konec' pro ukončení.");

while (true)
{
    Console.Write("\nOperace: ");
    var op = Console.ReadLine();
    if (op == "konec") break;
    if (op != "+" && op != "-" && op != "*" && op != "/")
    {
        Console.WriteLine("Neplatná operace.");
        continue;
    }

    Console.Write("První číslo: ");
    if (!double.TryParse(Console.ReadLine(), out var a))
    {
        Console.WriteLine("Neplatné číslo.");
        continue;
    }

    Console.Write("Druhé číslo: ");
    if (!double.TryParse(Console.ReadLine(), out var b))
    {
        Console.WriteLine("Neplatné číslo.");
        continue;
    }

    if (op == "/" && b == 0)
    {
        Console.WriteLine("Nelze dělit nulou.");
        continue;
    }

    var v = op switch
    {
        "+" => a + b,
        "-" => a - b,
        "*" => a * b,
        "/" => a / b,
        _ => 0
    };
    Console.WriteLine($"Výsledek: {v}");
}
Console.WriteLine("Program ukončen.");