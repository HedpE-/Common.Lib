namespace Common.Lib.Interfaces
{
    public interface IEndPoint
    {
        string Host { get; }
        string Hostname { get; }
        int Port { get; }
    }
}