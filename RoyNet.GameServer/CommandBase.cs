
namespace RoyNet.GameServer
{
    public abstract class CommandBase
    {
        public abstract string Name { get; }
        public abstract void Execute(Server server, Player player, object msg);
    }

    public abstract class CommandBase<TServer, TMsg> : CommandBase
        where TMsg : class
        where TServer:Server
    {
        public override void Execute(Server server, Player player, object msg)
        {
            OnExecute(server as TServer, player, msg as TMsg);
        }

        public abstract void OnExecute(TServer server, Player player, TMsg msg);
    }
}
