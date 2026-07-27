using System.Windows;
using System.Windows.Media;

namespace NotepadJM.Services;

public sealed class ThemeService : IThemeService
{
    public void Apply(string theme)
    {
        var isDark = !string.Equals(
            theme,
            "Light",
            StringComparison.OrdinalIgnoreCase);

        SetBrush("WindowBackgroundBrush", isDark ? "#0B1020" : "#FFFFFF");
        SetBrush("SurfaceBrush", isDark ? "#141B2D" : "#F1F5F9");
        SetBrush("SurfaceAltBrush", isDark ? "#1B243A" : "#E2E8F0");
        SetBrush("TextBrush", isDark ? "#F8FAFC" : "#0F172A");
        SetBrush("MutedTextBrush", isDark ? "#94A3B8" : "#475569");
        SetBrush("BorderBrush", isDark ? "#26324D" : "#CBD5E1");
    }

    private static void SetBrush(string key, string hexColor)
    {
        Application.Current.Resources[key] =
            new SolidColorBrush((Color)ColorConverter.ConvertFromString(hexColor));
    }
}
