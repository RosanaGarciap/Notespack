namespace Notespack.Models;

public class SyncNotifierService
{
    public event Action? OnSyncCompleted;

    public void NotifySyncCompleted()
    {
        OnSyncCompleted?.Invoke();
    }
}