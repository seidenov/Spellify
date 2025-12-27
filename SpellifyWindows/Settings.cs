using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Windows.Forms;

namespace Spellify;

static class Settings
{
    private static string ConfigPath => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "Spellify", "config.json"
    );
    
    public static string ApiKey { get; set; } = "";
    public static string Model { get; set; } = "gemini-2.0-flash";
    public static Keys HotKey { get; set; } = Keys.Control | Keys.Q;
    
    public static string HotKeyDisplay => HotKey.ToString().Replace(",", " +");
    
    public static readonly string[] AvailableModels = 
    {
        "gemini-2.0-flash",
        "gemini-2.5-flash-lite",
        "gemini-3-flash-preview"
    };
    
    public static readonly Dictionary<string, Keys> HotKeyPresets = new()
    {
        ["Ctrl+Q"] = Keys.Control | Keys.Q,
        ["Ctrl+`"] = Keys.Control | Keys.Oemtilde,
        ["Ctrl+Shift+F"] = Keys.Control | Keys.Shift | Keys.F,
        ["Ctrl+Shift+Space"] = Keys.Control | Keys.Shift | Keys.Space,
        ["Alt+Q"] = Keys.Alt | Keys.Q,
        ["Alt+F"] = Keys.Alt | Keys.F,
        ["Win+Q"] = Keys.LWin | Keys.Q,
    };
    
    public static void Load()
    {
        try
        {
            if (File.Exists(ConfigPath))
            {
                var json = File.ReadAllText(ConfigPath);
                var data = JsonSerializer.Deserialize<ConfigData>(json);
                if (data != null)
                {
                    ApiKey = data.ApiKey ?? "";
                    Model = data.Model ?? "gemini-2.0-flash";
                    if (HotKeyPresets.TryGetValue(data.HotKey ?? "", out var keys))
                        HotKey = keys;
                }
            }
        }
        catch { }
    }
    
    public static void Save()
    {
        try
        {
            var dir = Path.GetDirectoryName(ConfigPath)!;
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);
            
            var hotKeyName = HotKeyPresets.FirstOrDefault(x => x.Value == HotKey).Key ?? "Ctrl+Shift+F";
            var data = new ConfigData { ApiKey = ApiKey, Model = Model, HotKey = hotKeyName };
            var json = JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(ConfigPath, json);
        }
        catch { }
    }
    
    private class ConfigData
    {
        public string? ApiKey { get; set; }
        public string? Model { get; set; }
        public string? HotKey { get; set; }
    }
}
