using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CircuitSimulatorWeb.CircuitCode
{
    internal class Node
    {
        public List<Component> Components
        { get; private set; }
        public double Voltage;
        public int NumberIdent;
        public bool Calculated;


        public Node()
        {
            Components = new List<Component>();
            Voltage = 0;
            NumberIdent = -1;
            Calculated = false;
        }
        public void AddComponent(Component component)
        {
            Components.Add(component);
        }
        public void FuseNode(Node node)
        {
            foreach (Component component in Components.ToList())
            {
                component.ChangeNode(this, node);
                node.AddComponent(component);
            }
            // this node is now deleted
        }
        public double[] MakeEquation(int size) // the last value is reserved for the other side of the equal sign
        {
            double[] result = new double[size+1];
            foreach (Component component in Components)
            {
                if (component is ResistiveComponent)
                {
                    Node node = component.GetNext(this);
                    double resistance = ((ResistiveComponent)component).resistance;
                    if (node.Calculated)
                    {
                        result[size] += node.Voltage / resistance;
                    }
                    else
                    {
                        result[node.NumberIdent] -= 1 / resistance;
                    }
                    result[NumberIdent] += 1 / resistance;
                }
            }
            return result;
        }
    }
}
