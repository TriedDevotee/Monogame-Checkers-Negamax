using System;
using System.Data.Common;
using System.IO;
using System.Net.Http;
using System.Reflection.Metadata;
using System.Security.Cryptography.X509Certificates;
using System.Text.Json;
using Microsoft.Xna.Framework;

/// <summary>
/// Stores all the colors parsed from the .json. 
/// Each color is stored as a byte
/// </summary>
public class ColorObject
{
    public byte r { get; set; }
    public byte g { get; set; }
    public byte b { get; set; }
    public byte a { get; set; }

    public ColorObject() {}

    public ColorObject(float red, float green, float blue, float alpha)
    {
        r = (byte) red;
        g = (byte) green;
        b = (byte) blue;
        a = (byte) alpha;
    }

    /// <summary>
    /// Returns a new color which is constructed with the values of the stored colors
    /// </summary>
    /// <returns></returns>
    public Color GetColor()
    {
        return new(r, g, b, a);
    }
}

/// <summary>
/// Stores each piece of data parsed from the config.json file.
/// Note: Camel case is used to ensure the variables match with the ones in the .json
/// </summary>
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

/// <summary>
/// Stores all the loading and saving methods to do with editting the .json
/// </summary>
public class ConfigData
{
    public GameData config;
    string configPath;

    public ConfigData(){
        string exeDir = AppContext.BaseDirectory;
        configPath = Path.Combine(exeDir, "../../../src/config.json");

        config = LoadConfig();
    }

    /// <summary>
    /// Used to ensure that colors are loaded in the current instance of the application
    /// </summary>
    public void updateConfig()
    {
        config = LoadConfig();
    }

    /// <summary>
    /// Attempts to load the .json file. Starts by checking if the file exists, and if not, uses the deault data provided.
    /// Assuming the file is found, it tries to serialize the information within it.
    /// If that fails, the default values are used, and then those are written to the file (to prevent future issues).
    /// Otherwise, the file is serialized, and the values are used.
    /// </summary>
    /// <returns></returns>
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
            SaveConfig(defaultData);
            return defaultData;
        }
    }

    /// <summary>
    /// Saves to the file
    /// </summary>
    /// <param name="newConfig"></param>
    public void SaveConfig(GameData newConfig)
    {
        string json = JsonSerializer.Serialize(newConfig);
        File.WriteAllText(configPath, json);

        config = LoadConfig();
    }

    /// <summary>
    /// Stores default data. Colors resemble the following:<br/>
    ///  - Player1 = Pink <br/>
    ///  - Player2 = Purple <br/>
    ///  - Board1 = White<br/>
    ///  - Board2 = Black
    /// </summary>
    /// <returns></returns>
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