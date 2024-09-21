using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CircuitSimulatorWeb.CircuitCode
{
    internal class Component
    {
        public Node NodeA { get; set; }
        public Node NodeB { get; set; }

        public Component(Node nodeA, Node nodeB)
        {
            nodeA.AddComponent(this);
            this.NodeA = nodeA;
            nodeB.AddComponent(this);
            this.NodeB = nodeB;
        }
        public void ChangeNode(Node oldNode, Node newNode)
        {
            if(NodeA == oldNode)
            {
                NodeA = newNode;
            }
            else if(NodeB == oldNode)
            {
                NodeB = newNode;
            }
            else
            {
                Console.WriteLine("ERROR at ChangeNode, oldNode doesn't match either node");
            }
        }
        public Node GetNext(Node node)
        {
            if (node == NodeA)
            {
                return NodeB;
            }
            if (node == NodeB)
            {
                return NodeA;
            }
            return null;
        }
        public bool IsNodeA(Node node)
        {
            return NodeA == node;
        }
    }
}
