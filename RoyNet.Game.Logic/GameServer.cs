using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using RoyNet.Game.Logic.Chat;
using RoyNet.GameServer;

namespace RoyNet.Game.Logic
{
    public class GameServer : Server
    {
        private ConcurrentQueue<Order> _orders = new ConcurrentQueue<Order>();

        public GameServer(string address)
            : base(address)
        {
        }

        protected override void OnServerStartup(List<RoyNetModule> modulesContainer)
        {
            base.OnServerStartup(modulesContainer);
            modulesContainer.Add(new ChatModule());
        }

        protected override void OnMainLoop(IEnumerable<Player> players)
        {
            base.OnMainLoop(players);
            Order order;
            if (_orders.TryDequeue(out order))
            {
                switch (order.ContainerName)
                {
                    case ContainerName.Module:
                        break;
                    case ContainerName.Player:
                        if (string.IsNullOrEmpty(order.Key))
                        {
                            Console.WriteLine("players count:{0}", players.Count());
                        }
                        else
                        {
                            //Console.
                        }
                        break;
                }
            }
        }

        public void EnqueueOrder(Order order)
        {
            _orders.Enqueue(order);
        }
    }
}
