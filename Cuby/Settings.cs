using System.Text.Json;

namespace MadEngine;

[Serializable]
public class Settings
{
    public float Friction = 0.95f;
    public float RotSensitivity = 1.0f;
    public float DragSensitivity = 1f;
    public float Restitution = 0.8f;
    public float ZoomSensitivity = 1.0f;

    public float MinZoom = 170f;
    public float MaxZoom = 1f;
}

public static class SettingsManager
{
    public static string Path = "settings.json";

    public static JsonSerializerOptions Options = new()
    {
        WriteIndented = true,
        IndentSize = 4,
        IncludeFields = true
    };

    public static Settings Settings = Load();
    
    public static Settings Load()
    {
        if (!File.Exists(Path)) return new Settings();
        
        string data = File.ReadAllText(Path);
        return JsonSerializer.Deserialize<Settings>(data, Options) ?? new Settings();
    }
    
    public static void Save(Settings settings)
    {
        string data = JsonSerializer.Serialize(settings, Options);
        
        File.WriteAllText(Path, data);
    }
}