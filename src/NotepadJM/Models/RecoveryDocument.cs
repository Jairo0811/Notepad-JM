namespace NotepadJM.Models;

public sealed record RecoveryDocument(string? FilePath, string Content);

public sealed record RecoveredDocument(
    string RecoveryFilePath,
    string? FilePath,
    string Content);
