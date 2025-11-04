using Spectre.Console.Cli;
using System.ComponentModel;
using Spectre.Console;

namespace Socosit.Dms.AvaPublisher.Encryption;

public sealed class EncryptSettings : CommandSettings
{
    [CommandOption("-i|--input <PATH>")]
    [Description("Cesta ke zdrojovému ZIP (poivnná hodnota).")]
    public string? Input { get; set; }

    [CommandOption("-o|--output <PATH>")]
    [Description("Cesta k výstupnímu zakryptovanému ZIP (povinná hodnota).")]
    public string? Output { get; set; }

    [CommandOption("--overwrite")]
    [Description("Přepsat výstupní soubor pokud existuje.")]
    public bool Overwrite { get; set; } = true;

    [CommandOption("--level <0-9>")]
    [Description("Compression level 0..9 (výchozí: 6).")]
    public int Level { get; set; } = 6;

    public override ValidationResult Validate()
    {
        if (string.IsNullOrWhiteSpace(Input))
            return ValidationResult.Error("Chybí --input");
        if (string.IsNullOrWhiteSpace(Output))
            return ValidationResult.Error("Chybí --output");
        if (!File.Exists(Input))
            return ValidationResult.Error($"Nenalezen vstupní ZIP: {Input}");
        if (Level < 0 || Level > 9)
            return ValidationResult.Error("Nevalidní hodnota --level. Must be 0..9.");
        if (File.Exists(Output) && !Overwrite)
            return ValidationResult.Error($"Výstupní soubor existuje: {Output}. Použij --overwrite.");
        return ValidationResult.Success();
    }
}