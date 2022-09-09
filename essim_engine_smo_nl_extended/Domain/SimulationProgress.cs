namespace essim_engine_smo_nl_extended.Domain
{
    public class SimulationProgress
    {
        public string Id { get; set; }
        public string State { get; set; }
        public string Description { get; set; }
        public string DashboardUrl { get; set; }
        public double Progress { get; set; }
    }
}
