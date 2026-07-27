namespace NotepadJM.Models;

public sealed class AppSettings
{
    public string Theme { get; set; } = "Dark";
    public bool WordWrap { get; set; } = true;
    public List<string> RecentFiles { get; set; } = [];
}
