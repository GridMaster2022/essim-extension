namespace essim_engine_smo_nl_extended.Domain
{
    public class ItemState
    {
        public bool Started { get; set; }
        public bool Responsive { get; set; }
        public string Url { get; set; }
        public SimulationProgress SimulationProgress { get; set; }
    }
}
