namespace NotepadJM.Services;

public interface IDocumentFileService
{
    string Read(string path);
    void WriteAtomically(string path, string content);
}
