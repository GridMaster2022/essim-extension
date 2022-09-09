using System;
using essim_engine_smo_nl_extended.Domain;
using essim_extension_core;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace essim_engine_smo_nl_extended.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class StateController : ControllerBase
    {
        [HttpGet]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult Get()
        {
            State responseState = new State
            {
                EssimEngine = new ItemState
                {
                    Started = EssimManager.ApplicationStarted,
                    Responsive = EssimManager.ApplicationResponsive,
                    Url = EssimManager.ApplicationUrl,
                    SimulationProgress = new SimulationProgress
                    {
                        Id = string.IsNullOrEmpty(SimulationProcessor.SimulationId) ? null : SimulationProcessor.SimulationId,
                        State = SimulationProcessor.SimulationStateValue,
                        Description = string.IsNullOrEmpty(SimulationProcessor.SimulationStateDescription) ? null : SimulationProcessor.SimulationStateDescription,
                        DashboardUrl = string.IsNullOrEmpty(SimulationProcessor.SimulationDashboardUrl) ? null : SimulationProcessor.SimulationDashboardUrl,
                        Progress = SimulationProcessor.SimulationProgress
                    }
                },
                SqsEndpoint = new UrlInformation(Environment.GetEnvironmentVariable("AWS_ESSIM_QUEUE_URL"))
            };

            return Ok(responseState);
        }
    }
}
