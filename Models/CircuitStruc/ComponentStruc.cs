namespace CircuitSimulatorWeb.Models.CircuitStruc
{
    public class ComponentStruc
    {
        public string name { get; set; }
        public double? value { get; set; }
        public bool switchClosed { get; set; }
        public Conns[] connectors { get; set; }
        
    }
}
