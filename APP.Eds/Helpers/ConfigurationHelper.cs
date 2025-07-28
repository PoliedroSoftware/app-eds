using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace Helpers
{
    internal class ConfigurationHelper
    {
        private static readonly IConfigurationRoot builder = new ConfigurationBuilder()
            .AddJsonFile("configuration.json")
            .Build();

        public static string GetString(string key)
        {
            string envVar = Environment.GetEnvironmentVariable(key);
            if (envVar != null )
            {
                return envVar;
            }

            string value = builder[$"appSettings:{key}"];
            if ( value == null )
            {
                throw new NullReferenceException($"Key {key} is null");
            }
            return value;
        }
    }
}
