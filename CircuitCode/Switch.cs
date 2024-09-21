namespace CircuitSimulatorWeb.CircuitCode
{
    internal class Switch: ResistiveComponent
    {
        public Switch(Node nodeA, Node nodeB, bool closed) : base(nodeA, nodeB, 10000000000.0)
        {
            if (closed)
            {
                resistance = 0.00000000001;
            }
        }
    }
}
