using System;
using System.Drawing;
using System.IO;
using System.Text.Json;

namespace OverNuke
{
    public class Config
    {
        public int FontSize { get; set; } = 12;
        public string FontColor { get; set; } = "#FFFFFF";
        public int LocationX { get; set; } = 0;
        public int LocationY { get; set; } = 100;
        public string Hotkey { get; set; } = "Pause";

        public static Config Load(string path = "config.json")
        {
            if (!File.Exists(path))
                return new Config();

            string json = File.ReadAllText(path);
            return JsonSerializer.Deserialize<Config>(json) ?? new Config();
        }

        public Color GetFontColor()
        {
            return ColorTranslator.FromHtml(FontColor);
        }
    }
}
