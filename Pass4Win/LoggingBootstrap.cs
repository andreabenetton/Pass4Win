namespace Pass4Win
{
    using System;
    using System.IO;
    using Serilog;

    public static class LoggingBootstrap
    {
        public static void Configure()
        {
            var logFileName = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal),
                "Pass4Win.log");
            var logLayout = "{Timestamp:HH:mm} [{Level}] ({ThreadId}) {Message}{NewLine}{Exception}";

            Log.Logger = new LoggerConfiguration()
                .Enrich.FromLogContext()
                .Enrich.WithProcessId()
                .Enrich.WithThreadId()
                .WriteTo.Console(outputTemplate: logLayout)
                .WriteTo.File(logFileName, outputTemplate: logLayout)
                .CreateLogger();
        }
    }
}