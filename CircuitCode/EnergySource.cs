using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CircuitSimulatorWeb.CircuitCode
{
    internal class EnergySource : Component // A side is positive, B side is negative
    {
        public double voltage;
        public EnergySource(Node nodeA, Node nodeB, double voltage) : base(nodeA, nodeB)
        {
            this.voltage = voltage;
        }
    }
}
