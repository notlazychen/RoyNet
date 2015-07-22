
namespace RoyNet.Server.GameEngine
{
    public abstract class CommandBase
    {
        public abstract string Name { get; }
        public abstract void Execute(AppServer server, Player player, object msg);
    }

    public abstract class CommandBase<TServer, TMsg> : CommandBase
        where TMsg : class
        where TServer:AppServer
    {
        public override void Execute(AppServer server, Player player, object msg)
        {
            OnExecute(server as TServer, player, msg as TMsg);
        }

        public abstract void OnExecute(TServer server, Player player, TMsg msg);
    }
}
