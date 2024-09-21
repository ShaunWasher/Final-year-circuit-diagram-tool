namespace CircuitSimulatorWeb.CircuitCode
{
    internal class Ammeter : ResistiveComponent
    {
        public Ammeter(Node nodeA, Node nodeB) : base(nodeA, nodeB, 0.00000000001) { }

    }
}