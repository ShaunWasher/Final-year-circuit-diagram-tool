using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CircuitSimulatorWeb.Models.CircuitStruc;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;

namespace CircuitSimulatorWeb.CircuitCode
{
    internal class Circuit
    {
        public Component[] components;
        public List<Node> nodes;

        public Circuit(ComponentStruc[] structure) // a constructor that takes in the structure from the website
        {
            nodes = new List<Node>();


            static Node getNodeFromLetter(string letter, Component component)
            {
                if (letter == "L")
                {
                    return component.NodeA;
                }
                else
                {
                    return component.NodeB;
                }
            }


            int counter = 0; // get the amount of components
            foreach (ComponentStruc componentSt in structure)
            {
                if (componentSt.connectors.Length != 0) // ignore components in left panel
                {
                    counter++;
                }
            }
            components = new Component[counter];

            foreach (ComponentStruc componentSt in structure)
            {
                if (componentSt.connectors.Length != 0) // ignore components in left panel (shouldn't be needed)
                {
                    switch (componentSt.name[9])
                    {
                        case 'R':
                        case 'L':
                            AddComponent(int.Parse(componentSt.name.Remove(0, 10)), ComponentType.Resistor, (double)componentSt.value); // make the new component
                            break;
                        case 'B':
                            AddComponent(int.Parse(componentSt.name.Remove(0, 10)), ComponentType.Cell, (double)componentSt.value);
                            break;
                        case 'V':
                            AddComponent(int.Parse(componentSt.name.Remove(0, 10)), ComponentType.Voltmeter);
                            break;
                        case 'A':
                            AddComponent(int.Parse(componentSt.name.Remove(0, 10)), ComponentType.Ammeter);
                            break;
                        case 'S':
                            AddComponent(int.Parse(componentSt.name.Remove(0, 10)), ComponentType.Switch, 0, componentSt.switchClosed);
                            break;
                    }
                }
            }
            foreach (ComponentStruc componentSt in structure) // do again to add connections
            {
                foreach (Conns conns in componentSt.connectors)
                {
                    Component compA = components[int.Parse(componentSt.name.Remove(0, 10))];
                    foreach (string conn in conns.connections)
                    {
                        string s = conn.Remove(0, 10);
                        Component compB = components[int.Parse(s.Remove(s.Length-1))];
                        AddWire(getNodeFromLetter(conns.name, compA), getNodeFromLetter(s[^1].ToString(), compB));
                    }
                }
            }
        }
        private Component AddComponent(int id, ComponentType type = ComponentType.Resistor, double value = 0, bool closed = false) // value is the voltage or resistance dependent on the component
        {
            Node node = new Node();
            nodes.Add(node);
            Node node1 = new Node();
            nodes.Add(node1);

            Component output;
            switch (type)
            {
                case ComponentType.Resistor: output = new ResistiveComponent(node, node1, value); break;
                case ComponentType.Cell: output = new EnergySource(node, node1, value); break;
                case ComponentType.Voltmeter: output = new Voltmeter(node, node1); break;
                case ComponentType.Ammeter: output = new Ammeter(node, node1); break;
                case ComponentType.Switch: output = new Switch(node, node1, closed); break;
                default: output = new ResistiveComponent(node, node1, value); break; // the code should never get here
            }
            components[id] = output;
            return output;
        }
        private void AddWire(Node node1, Node node2) // nodes must be in the circuit
        {
            if (node1 != node2)
            {
                node1.FuseNode(node2);
                nodes.Remove(node1);
            }
        }
        public void CalculateVoltages()
        {
            List<double> supernodeNodes = new();
            foreach (Node node in nodes)
            {
                node.Voltage = 0;
                node.Calculated = false;
                node.NumberIdent = -1;
            }
            EnergySource startingBattery = null;
            foreach (Component component in components)
            {
                if (component is EnergySource)
                {
                    startingBattery = (EnergySource)component;
                    break;
                }
            }
            startingBattery.NodeA.Voltage = startingBattery.voltage;
            startingBattery.NodeA.Calculated = true;
            startingBattery.NodeB.Voltage = 0;
            startingBattery.NodeB.Calculated = true;

            bool flag = true;
            while (flag) // doing this multiple times as execution can effect the if statment requirments
            {
                flag = false;
                foreach (Component component in components)
                {
                    if (component is EnergySource && !component.Equals(startingBattery) && (component.NodeA.Calculated ^ component.NodeB.Calculated))
                    {
                        flag = true;
                        if (component.NodeA.Calculated)
                        {
                            component.NodeB.Voltage = component.NodeA.Voltage - ((EnergySource)component).voltage; // negative due to polarity
                            component.NodeB.Calculated = true;
                        }
                        else
                        {
                            component.NodeA.Voltage = component.NodeB.Voltage + ((EnergySource)component).voltage;
                            component.NodeA.Calculated = true;
                        }
                    }
                } 
            }

            List<Node> orderedNodes = new List<Node>();
            int counter = 0;
            foreach (Component component in components) // count supernodes and put their nodes at the start
            {
                if ((component is EnergySource) && (!component.NodeA.Calculated && !component.NodeB.Calculated)){
                    supernodeNodes.Add(((EnergySource)component).voltage);
                    component.NodeA.NumberIdent = counter;
                    orderedNodes.Add(component.NodeA);
                    counter++;
                    component.NodeB.NumberIdent = counter;
                    orderedNodes.Add(component.NodeB);
                    counter++;
                } // will currently not work if two supernodes are adjacent 
            }

            
            foreach (Node node in nodes) // count other nodes
            {
                if (!node.Calculated && node.NumberIdent < 0)
                {
                    node.NumberIdent = counter;
                    orderedNodes.Add(node);
                    counter++;
                }
            }
            if (counter == 0 && supernodeNodes.Count == 0) // end if all nodes are already calculated
            {
                return;
            }

            double[,] matrix = new double[counter, counter];
            double[] vector = new double[counter];

            // handle super node equations first
            for (int i = 0; i < supernodeNodes.Count; i++)
            {
                int pos1 = ((i + 1) * 2) - 1; // pos side of super node
                int pos2 = i * 2; // neg side of super node
                double[] equation1 = orderedNodes[pos1].MakeEquation(counter);
                double[] equation2 = orderedNodes[pos2].MakeEquation(counter);
                double[] combinedEquation = new double[counter + 1];
                for(int j = 0; j < counter+1; j++)
                {
                    combinedEquation[j] = equation1[j] + equation2[j]; // combine equations for super node
                }

                equation1 = combinedEquation; // rearange equation as supernode equation pos1 - x = pos2 is substituted in
                equation1[pos1] += equation1[pos2];
                equation1[counter] -= equation1[pos2] * supernodeNodes[i];
                equation1[pos2] = 0;

                vector[pos1] = equation1[counter];
                for (int j = 0; j < equation1.Length - 1; j++) // add to matrix
                {
                    matrix[pos1, j] = equation1[j];
                }

                equation2 = combinedEquation; // rearange equation as supernode equation pos1 = pos2 + x is substituted in
                equation2[pos2] += equation2[pos1];
                equation2[counter] += equation2[pos1] * supernodeNodes[i];
                equation2[pos1] = 0;

                vector[pos2] = equation2[counter];
                for (int j = 0; j < equation2.Length - 1; j++) // add to matrix
                {
                    matrix[pos2, j] = equation2[j];
                }
            }
            
            // fill in the rest of the matrix starting from after super nodes
            for(int counterTwo = supernodeNodes.Count * 2; counterTwo < orderedNodes.Count; counterTwo++)
            {
                double[] equation = orderedNodes[counterTwo].MakeEquation(counter); // the last value is reserved for the other side of the equal sign
                vector[counterTwo] = equation[equation.Length - 1];
                for(int i = 0; i < equation.Length-1; i++) // add to matrix
                {
                    matrix[counterTwo, i] = equation[i];
                }
            }
            Matrix<double> matrix1 = Matrix<double>.Build.DenseOfArray(matrix);
            matrix1 = matrix1.Inverse();
            Vector<double> vector1 = Vector<double>.Build.DenseOfArray(vector);
            vector1 = matrix1.Multiply(vector1);
            foreach(Node node in nodes)
            {
                if (node.NumberIdent >= 0)
                {
                    node.Voltage = vector1[node.NumberIdent];
                    node.Calculated = true;
                }
            }
        }
        public void updateComponents()
        {
            foreach (Component comp in components)
            {
                if (comp is ResistiveComponent)
                {
                    ((ResistiveComponent)comp).CalculateCurrent();
                }
            }
        }
    }
}
