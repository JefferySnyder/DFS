public class SharedState
{
    public List<string> StorageNodes { get; }
    public SharedState(int size)
    {
        StorageNodes = new List<string>(size);
    }
}
