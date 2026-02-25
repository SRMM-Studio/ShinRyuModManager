using Serilog;
using ILogger = NetSparkleUpdater.Interfaces.ILogger;

namespace ShinRyuModManager.UserInterface.Updater;

public class SerilogWriter : ILogger
{
    public void PrintMessage(string message, params object[] arguments)
    {
#pragma warning disable CA2254
        Log.Information(message, arguments);
#pragma warning restore CA2254
    }
}
