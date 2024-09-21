using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CircuitSimulatorWeb.CircuitCode
{
    internal class ResistiveComponent : Component
    {
        public double resistance;
        public double current;
        public double voltage;
        public ResistiveComponent(Node nodeA, Node nodeB, double resistance) : base(nodeA, nodeB)
        {
            this.resistance = resistance;
        }
        public void CalculateCurrent() // can only be used when nodes are set
        {
            current = (NodeA.Voltage-NodeB.Voltage)/resistance;
        }
        public double GetVoltage()
        {
            return NodeA.Voltage - NodeB.Voltage;
        }
    }
}
