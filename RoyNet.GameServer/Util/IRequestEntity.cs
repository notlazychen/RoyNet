namespace RoyNet.GameServer
{
    public interface IRequestEntity
    {
        void Deserialize(byte[] data);
    }

    public interface IRequestEntity<T> : IRequestEntity
    {
        T Entity { get; set; }
    }
}
