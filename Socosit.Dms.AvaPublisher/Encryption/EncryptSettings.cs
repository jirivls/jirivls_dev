using Spectre.Console.Cli;
using System.ComponentModel;
using Spectre.Console;

namespace Socosit.Dms.AvaPublisher.Encryption;

public sealed class EncryptSettings : CommandSettings
{
    [CommandOption("-i|--input <PATH>")]
    [Description("Path to the source ZIP (required).")]
    public string? Input { get; set; }

    [CommandOption("-o|--output <PATH>")]
    [Description("Path for the encrypted ZIP to create (required).")]
    public string? Output { get; set; }

    [CommandOption("--overwrite")]
    [Description("Overwrite output file if it exists.")]
    public bool Overwrite { get; set; } = false;

    [CommandOption("--level <0-9>")]
    [Description("Compression level 0..9 (default: 6).")]
    public int Level { get; set; } = 6;

    public override ValidationResult Validate()
    {
        if (string.IsNullOrWhiteSpace(Input))
            return ValidationResult.Error("Missing --input");
        if (string.IsNullOrWhiteSpace(Output))
            return ValidationResult.Error("Missing --output");
        if (!File.Exists(Input))
            return ValidationResult.Error($"Input ZIP not found: {Input}");
        if (Level < 0 || Level > 9)
            return ValidationResult.Error("Invalid --level. Must be 0..9.");
        if (File.Exists(Output) && !Overwrite)
            return ValidationResult.Error($"Output exists: {Output}. Use --overwrite.");
        return ValidationResult.Success();
    }
}