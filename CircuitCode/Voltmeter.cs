namespace CircuitSimulatorWeb.CircuitCode
{
    internal class Voltmeter: ResistiveComponent
    {
        public Voltmeter(Node nodeA, Node nodeB) : base(nodeA, nodeB, 10000000000.0) { }

    }
}
