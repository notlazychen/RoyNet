namespace RoyNet.Game.Logic
{
    /// <summary>
    /// GM
    /// </summary>
    public class GameManager
    {
        public GameServer Server { get; private set; }
        public static GameManager CreateGameManager(string address = null)
        {
            GameManager bs = new GameManager();
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
