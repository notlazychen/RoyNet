
namespace RoyNet.GameServer
{
    public abstract class CommandBase
    {
        public abstract string Name { get; }
        public abstract void Execute(Player player, object msg);
    }

    public abstract class CommandBase<T> : CommandBase
        where T : class
    {
        public override void Execute(Player player, object msg)
        {
            OnExecute(player, msg as T);
        }

        public abstract void OnExecute(Player player, T msg);
    }
}
