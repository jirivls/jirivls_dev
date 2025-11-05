using System;
using System.IO;
using ICSharpCode.SharpZipLib.Zip;
using Spectre.Console;
using Spectre.Console.Cli;
using System.Security.Cryptography;
using System.Threading;
using BclZipFile = System.IO.Compression.ZipFile;

namespace Socosit.Dms.AvaPublisher.Encryption;

public sealed class EncryptCommand : Command<EncryptSettings>
{
    private static readonly byte[] AesKey = Convert.FromBase64String(
        "fX1e1HjR2J+z7O7yLzPkSxvO6aF2C6yab7WcYQ2f4Aw=");

    public override int Execute(CommandContext context, EncryptSettings s,
        CancellationToken cancellationToken)
    {
        try
        {
            var passedOutput = Path.GetFullPath(s.Output!);
            var outputDir = Directory.Exists(passedOutput) ? passedOutput : Path.GetDirectoryName(passedOutput)!;

            var inputName = Path.GetFileNameWithoutExtension(s.Input);
            var timestamp = DateTime.Now.ToString("yyyyMMddHHmmss");
            var outputFileName = $"{timestamp}_{inputName}.zip";
            s.Output = Path.Combine(outputDir, outputFileName);
            s.Output = Path.Combine(outputDir, outputFileName);

            Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(s.Output!))!);
            if (File.Exists(s.Output) && s.Overwrite) File.Delete(s.Output!);

            using var source = BclZipFile.OpenRead(s.Input!);
            using var outFs = new FileStream(s.Output!, FileMode.CreateNew, FileAccess.Write, FileShare.None);
            using var zipOut = new ZipOutputStream(outFs) { IsStreamOwner = true };

            zipOut.SetLevel(s.Level);
            zipOut.UseZip64 = UseZip64.Dynamic;

            var files = 0;

            foreach (var entry in source.Entries)
            {
                if (string.IsNullOrEmpty(entry.Name) && entry.FullName.EndsWith("/"))
                {
                    var dirEntry = new ZipEntry(entry.FullName.Replace('\\', '/'))
                    {
                        DateTime = entry.LastWriteTime.DateTime
                    };
                    zipOut.PutNextEntry(dirEntry);
                    zipOut.CloseEntry();
                    continue;
                }

                if (string.IsNullOrEmpty(entry.Name))
                    continue;

                var newEntry = new ZipEntry(entry.FullName.Replace('\\', '/'))
                {
                    DateTime = entry.LastWriteTime.DateTime,
                    CompressionMethod = CompressionMethod.Deflated
                };

                zipOut.PutNextEntry(newEntry);

                using var inStream = entry.Open();
                EncryptStreamToStream(inStream, zipOut, AesKey);

                zipOut.CloseEntry();
                files++;
            }

            zipOut.Finish();
            AnsiConsole.MarkupLine($"[green]Hotovo.[/] {files} soubor(y) byly zakryptované do: [bold]{s.Output}[/]");
            return 0;
        }
        catch (InvalidDataException ide)
        {
            AnsiConsole.MarkupLine($"[red]Vstupní soubor není validní ZIP:[/] {ide.Message}");
            return 4;
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]Neočekávaná chyba:[/] {ex.Message}");
            return 1;
        }
    }

    private static void EncryptStreamToStream(Stream plain, Stream dest, byte[] key)
    {
        var iv = RandomNumberGenerator.GetBytes(12);
        dest.Write(iv, 0, iv.Length);

        const int tagSize = 16;
        using var aesGcm = new AesGcm(key, tagSize);

        using var msPlain = new MemoryStream();
        plain.CopyTo(msPlain);
        var plainBytes = msPlain.ToArray();

        var cipher = new byte[plainBytes.Length];
        var tag = new byte[tagSize];

        aesGcm.Encrypt(iv, plainBytes, cipher, tag, associatedData: null);

        dest.Write(cipher, 0, cipher.Length);
        dest.Write(tag, 0, tag.Length);
    }
}