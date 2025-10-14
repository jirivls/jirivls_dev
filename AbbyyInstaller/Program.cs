using System.Diagnostics;

namespace AbbyyInstaller;

internal class Program
{
    static void Main()
    {
        var install = false;
        var openLicenceManager = false;

        // Instalace
        var installer = "installRnt64.exe";
        var installDir = @"C:\DOCUX5\FRE12_Install";
        var targetDir = @"C:\DOCUX5\FRE12";

        // Uzivtel

        string userName = Environment.UserName;
        string domain = Environment.UserDomainName;

        // Licence
        var licenceManager = "LicenseManager.exe";
        var licenceManagerDir = targetDir + @"\Bin64";

        if (install)
        {
            Console.WriteLine("Start installeru...");

            var process = new Process();
            process.StartInfo = new ProcessStartInfo
            {
                FileName = Path.Combine(installDir, installer),
                Arguments = $"INSTALLDIR=\"{targetDir}\" REGISTERCOM=\"Force\"",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true,
            };

            process.OutputDataReceived += (_, e) =>
            {
                if (e.Data != null) Console.WriteLine(e.Data);
            };
            process.ErrorDataReceived += (_, e) =>
            {
                if (e.Data != null) Console.WriteLine("ERR: " + e.Data);
            };

            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
            process.WaitForExit();

            Console.WriteLine($"Konec instalace: {process.ExitCode}");

            if (openLicenceManager)
            {
                var licenceProcess = new Process();
                licenceProcess.StartInfo = new ProcessStartInfo
                {
                    FileName = Path.Combine(licenceManagerDir, licenceManager),
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                };

                licenceProcess.OutputDataReceived += (_, e) =>
                {
                    if (e.Data != null) Console.WriteLine(e.Data);
                };
                licenceProcess.ErrorDataReceived += (_, e) =>
                {
                    if (e.Data != null) Console.WriteLine("ERR: " + e.Data);
                };

                licenceProcess.Start();
            }
        }
        else
        {
            {
                Console.WriteLine("Start deinstalleru...");

                var process = new Process();
                process.StartInfo = new ProcessStartInfo
                {
                    FileName = Path.Combine(installDir, installer),
                    Arguments = $"/q /v INSTALLDIR=\"{targetDir}\" DeinstallRuntime=Yes",
                    WorkingDirectory = installDir,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true,
                    LoadUserProfile = true
                };


                var test = $"\"{process.StartInfo.FileName}\" {process.StartInfo.Arguments}";

                process.OutputDataReceived += (_, e) =>
                {
                    if (e.Data != null) Console.WriteLine(e.Data);
                };
                process.ErrorDataReceived += (_, e) =>
                {
                    if (e.Data != null) Console.WriteLine("ERR: " + e.Data);
                };

                process.Start();
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();
                process.WaitForExit();

                Console.WriteLine($"Konec instalace: {process.ExitCode}");
            }
        }
    }
}