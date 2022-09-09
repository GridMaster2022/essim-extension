using System;

namespace essim_engine_smo_nl_extended.Domain
{
    public class State
    {
        public DateTime BuildDateTime => Program.BuildDateTime;
        public DateTime BootDateTime => Program.BootDateTime;
        public ItemState EssimExtension { get; set; }
        public ItemState EssimEngine { get; set; }
        public UrlInformation SqsEndpoint { get; set; }

        public State()
        {
            EssimExtension = new ItemState
            {
                Started = true,
                Responsive = true
            };
        }
    }
}
