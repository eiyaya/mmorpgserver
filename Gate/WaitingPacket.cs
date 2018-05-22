#region using

using System.Diagnostics;
using Scorpion;

#endregion

namespace Gate
{
    internal class WaitingPacket
    {
        public ServiceDesc Desc { get; set; }
        public Stopwatch Watch { get; set; }
    }
}