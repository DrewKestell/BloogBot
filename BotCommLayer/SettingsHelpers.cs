using System.Net;
using Microsoft.Extensions.Configuration;

namespace BotCommLayer;

public static class SettingsHelper
{
    public static IPAddress GetIPAddressFromConfigOrEnv(this IConfiguration configuration, string envVarName, string configKeyName)
    {
        var envValue = Environment.GetEnvironmentVariable(envVarName);
        if (!string.IsNullOrEmpty(envValue))
        {
            if (IPAddress.TryParse(envValue, out var ipAddress))
            {
                return ipAddress;
            }
            throw new ArgumentException($"Environment variable {envVarName} is not a valid IP address.");
        }

        var configValue = configuration[configKeyName];
        if (!string.IsNullOrEmpty(configValue))
        {
            if (IPAddress.TryParse(configValue, out var ipAddress))
            {
                return ipAddress;
            }
            throw new ArgumentException($"Configuration setting {configKeyName} is not a valid IP address.");
        }

        throw new ArgumentException($"Neither environment variable {envVarName} nor configuration setting {configKeyName} are available or valid.");
    }

    public static int GetIntFromConfigOrEnv(this IConfiguration configuration, string envVarName, string configKeyName)
    {
        var envValue = Environment.GetEnvironmentVariable(envVarName);
        if (!string.IsNullOrEmpty(envValue))
        {
            if (int.TryParse(envValue, out var intValue))
            {
                return intValue;
            }
            throw new ArgumentException($"Environment variable {envVarName} is not a valid integer.");
        }

        var configValue = configuration[configKeyName];
        if (int.TryParse(configValue, out var configIntValue))
        {
            return configIntValue;
        }

        throw new ArgumentException($"Neither environment variable {envVarName} nor configuration setting {configKeyName} are available or valid.");
    }
}