using System.IO;
using System.Text.Json;

namespace CadastreInvent.Api.Services
{
    public class ThemeSettings
    {
        public string CompanyName { get; set; } = "Государственная кадастровая служба";
        public string SystemName { get; set; } = "Платформа пространственных данных";
        public string Description { get; set; } = "Централизованный комплекс автоматизации кадастрового учета, регистрации имущественных прав и пространственного анализа.";
        public string LogoUrl { get; set; } = "";
    }

    public static class ThemeSettingsManager
    {
        private static readonly string _directoryPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "config");
        private static readonly string _filePath = Path.Combine(_directoryPath, "theme.json");

        public static ThemeSettings GetSettings()
        {
            if (!File.Exists(_filePath))
            {
                return new ThemeSettings();
            }

            try
            {
                var json = File.ReadAllText(_filePath);
                return JsonSerializer.Deserialize<ThemeSettings>(json) ?? new ThemeSettings();
            }
            catch
            {
                return new ThemeSettings();
            }
        }

        public static void SaveSettings(ThemeSettings settings)
        {
            if (!Directory.Exists(_directoryPath))
            {
                Directory.CreateDirectory(_directoryPath);
            }

            var options = new JsonSerializerOptions { WriteIndented = true };
            File.WriteAllText(_filePath, JsonSerializer.Serialize(settings, options));
        }
    }
}