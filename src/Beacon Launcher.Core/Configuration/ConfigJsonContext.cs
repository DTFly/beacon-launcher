using System.Text.Json.Serialization;

namespace BeaconLaunch.Core.Configuration;

[JsonSourceGenerationOptions(
    WriteIndented = true,
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
[JsonSerializable(typeof(LauncherConfig))]
internal sealed partial class ConfigJsonContext : JsonSerializerContext
{
}
