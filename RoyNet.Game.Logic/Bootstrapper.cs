namespace RoyNet.Game.Logic
{
    public class Bootstrapper
    {
        public GameServer Server { get; private set; }
        public static Bootstrapper CreateBootstrapper(string address = null)
        {
            Bootstrapper bs = new Bootstrapper();
            bs.Server = new GameServer(address ?? "tcp://127.0.0.1");
            return bs;
        }

        public void Start()
        {
            Server.Open();
        }

        public void Stop()
        {
            Server.Close();
        }

        public void Order(string cmd)
        {
            Server.EnqueueOrder(new Order()
            {
                ContainerName = ContainerName.Player
            });
        }
    }
}
