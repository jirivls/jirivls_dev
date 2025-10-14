using System.Diagnostics;

namespace AbbyyInstallerWorker
{
    public class Worker : BackgroundService
    {
        private readonly string _eventSource = "AbbyyInstallerWorker";
        private readonly string _eventLogName = "Application";

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            EnsureEventSource();

            await Task.Delay(3000, stoppingToken).ConfigureAwait(false);

            var install = true;
            var runInstall = false;
            var openLicenceManager = true;

            // Instalace
            var installer = "installRnt64.exe";
            var installDir = @"C:\DOCUX5\FRE12_Install";
            var targetDir = @"C:\DOCUX5\FRE12";

            // Uzivatel
            string userName = Environment.UserName;
            string domain = Environment.UserDomainName;

            // Licence
            var licenceManager = "LicenseManager.exe";
            var licenceManagerDir = targetDir + @"\Bin64";

            if (openLicenceManager)
            {
                var licenceProcess = new Process();
                licenceProcess.StartInfo = new ProcessStartInfo
                {
                    FileName = Path.Combine(licenceManagerDir, licenceManager),
                    WorkingDirectory = licenceManagerDir,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true,
                    LoadUserProfile = true
                };

                licenceProcess.OutputDataReceived += (_, e) =>
                {
                    if (e.Data != null) Log(e.Data);
                };
                licenceProcess.ErrorDataReceived += (_, e) =>
                {
                    if (e.Data != null) Log("ERR: " + e.Data, EventLogEntryType.Error);
                };

                licenceProcess.Start();
            }

            if (install & runInstall)
            {
                Log("Start installeru...");

                var process = new Process();
                process.StartInfo = new ProcessStartInfo
                {
                    FileName = Path.Combine(installDir, installer),
                    Arguments = $"INSTALLDIR=\"{targetDir}\" REGISTERCOM=\"Force\"",
                    WorkingDirectory = installDir,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true,
                    LoadUserProfile = true
                };


                process.OutputDataReceived += (_, e) =>
                {
                    if (e.Data != null) Log(e.Data);
                };
                process.ErrorDataReceived += (_, e) =>
                {
                    if (e.Data != null) Log("ERR: " + e.Data, EventLogEntryType.Error);
                };

                process.Start();
                //process.BeginOutputReadLine();
                //process.BeginErrorReadLine();
                await process.WaitForExitAsync(stoppingToken).ConfigureAwait(false);

                Log($"Konec instalace: {process.ExitCode}");
            }
            else
            {
                Log("Start deinstalleru...");

                var process = new Process();
                process.StartInfo = new ProcessStartInfo
                {
                    FileName = Path.Combine(installDir, installer),
                    Arguments = $"/q /v INSTALLDIR=\"{targetDir}\" DeinstallRuntime=Yes",
                    WorkingDirectory = installDir,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                };

                process.OutputDataReceived += (_, e) =>
                {
                    if (e.Data != null) Log(e.Data);
                };
                process.ErrorDataReceived += (_, e) =>
                {
                    if (e.Data != null) Log("ERR: " + e.Data, EventLogEntryType.Error);
                };

                process.Start();
                //process.BeginOutputReadLine();
                //process.BeginErrorReadLine();
                await process.WaitForExitAsync(stoppingToken).ConfigureAwait(false);

                Log($"Konec instalace: {process.ExitCode}");
            }

            await Task.Delay(30000, stoppingToken).ConfigureAwait(false);
        }

        private void Log(string message, EventLogEntryType type = EventLogEntryType.Information)
        {
            Console.WriteLine(message);

            using (var eventLog = new EventLog(_eventLogName))
            {
                eventLog.Source = _eventSource;
                eventLog.WriteEntry(message, type, 1000);
            }
        }

        private void EnsureEventSource()
        {
            if (!EventLog.SourceExists(_eventSource))
            {
                EventLog.CreateEventSource(_eventSource, _eventLogName);
            }
        }
    }
}