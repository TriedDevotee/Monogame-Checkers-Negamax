using System;
using System.Data.Common;
using System.IO;
using System.Net.Http;
using System.Reflection.Metadata;
using System.Security.Cryptography.X509Certificates;
using System.Text.Json;
using Microsoft.Xna.Framework;

public class ColorObject
{
    public byte r { get; set; }
    public byte g { get; set; }
    public byte b { get; set; }
    public byte a { get; set; }

    public Color GetColor()
    {
        return new(r, g, b, a);
    }
}

public class GameData
{
    public ColorObject white_player_color { get; set; }
    public ColorObject black_player_color { get; set; }
    public ColorObject board_color_1 { get; set; }
    public ColorObject board_color_2 { get; set; }

    public GameData() {}
    public GameData(
        ColorObject white, ColorObject black,
        ColorObject board1, ColorObject board2
    )
    {
        white_player_color = white;
        black_player_color = black;
        board_color_1 = board1;
        board_color_2 = board2;
    }
}

public class ConfigData
{
    public GameData config;
    string configPath;

    public ConfigData(){
        string exeDir = AppContext.BaseDirectory;
        configPath = Path.Combine(exeDir, "../../../src/config.json");

        config = LoadConfig();
    }

    public void updateConfig()
    {
        config = LoadConfig();
    }

    private GameData LoadConfig()
    {
        if (!File.Exists(configPath))
        {
            GameData defaultData = useDefaultData();
            config = defaultData;
            return defaultData;
        }

        try
        {       
            string json = File.ReadAllText(configPath);
            return JsonSerializer.Deserialize<GameData>(json);
        }
        catch
        {
            GameData defaultData = useDefaultData();
            config = defaultData;
            SaveConfig();
            return defaultData;
        }
    }

    public void SaveConfig()
    {
        string json = JsonSerializer.Serialize(config);
        File.WriteAllText(configPath, json);
    }

    public GameData useDefaultData()
    {
        return new GameData(
            new ColorObject {r = 255, g = 3, b = 233, a = 255},
            new ColorObject {r = 183, g = 0, b = 255, a = 255},
            new ColorObject {r = 255, g = 255, b = 255, a = 255},
            new ColorObject {r = 0, g = 0, b = 0, a = 255}
        );
    }
}