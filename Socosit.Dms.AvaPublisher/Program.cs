using Socosit.Dms.AvaPublisher.Encryption;
using Spectre.Console.Cli;

namespace Socosit.Dms.AvaPublisher;

public static class Program
{
    public static int Main(string[] args)
    {
        var app = new CommandApp();
        app.Configure(c =>
        {
            c.SetApplicationName("AvaZipEncrypt");
            c.SetApplicationVersion("1.0.0");
            c.PropagateExceptions();
            c.ValidateExamples();

            c.AddCommand<EncryptCommand>("encrypt")
                .WithDescription("Čtení souborů a uložení do zakryptovaného ZIP (AES-256).")
                .WithExample("encrypt",
                    "--input", "C:\\in.zip",
                    "--output", "C:\\out-protected.zip", "--overwrite",
                    "--level", "6");
        });

        return app.Run(args);
    }
}