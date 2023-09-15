using Microsoft.Extensions.Configuration;
using System;

namespace SuncorCustomDialogsBot
{
    public class AppsettingReader
    {
        public T ReadSecret<T>(string sectionName)
        {
            var environment = Environment.GetEnvironmentVariable("NETCORE_ENVIRONMENT");
            var builder = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .AddJsonFile($"appsettings.{environment}.json", optional: true)
                .AddEnvironmentVariables();
            var configurationRoot = builder.Build();
            var secret = configurationRoot.GetSection(sectionName).Get<T>();
            return secret;
        }
    }
}
