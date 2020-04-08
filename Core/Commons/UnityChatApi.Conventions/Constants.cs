using System;

namespace ASPT.Conventions {
    public class Constants {
        public const string LOG_FILE = @"log/log.txt";
        public const string CONFIG_FILE = "appsettings.json";
        public const string LOG_OUTPUT_TEMPLATE = "{Timestamp: HH:mm: ss} [{Level:u3}] {Properties} {Message:lj}{NewLine}{Exception}";
        public const string CORELLATION_ID = "corellationId";
    }
}
