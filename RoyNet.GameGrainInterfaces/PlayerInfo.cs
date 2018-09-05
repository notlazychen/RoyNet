using Orleans.Concurrency;
using System;

namespace AdventureGrainInterfaces
{
    [Immutable]
    public class PlayerInfo
    {
        public string Key { get; set; }
        public string Name { get; set; }
    }
}
