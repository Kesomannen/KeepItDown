namespace KeepItDown; 

public static class SharedUI {
    public const string Name = "Keep It Down!";
    public const string Guid = KeepItDownInfo.Guid;
    public const string Version = KeepItDownInfo.Version;
    public const string Description = "Volume control for various sounds in the game.";

    public static string GetDisplayName(VolumeConfig volumeConfig) {
        var text = $"{volumeConfig.Key} Volume";
        if (volumeConfig.Section != "Vanilla") {
            text += $" ({volumeConfig.Section})";
        }
        return text;
    }

    public static void ResetAllVolumes() {
        foreach (var volumeConfig in KeepItDownPlugin.Instance.Config.Volumes.Values) {
            volumeConfig.NormalizedValue = 1f;
        }
    }
}