using hrm.Common;
using hrm.Respository.Agents;
using Microsoft.AspNetCore.Mvc;

namespace hrm.Controllers
{
    public class AgentController : ControllerBase
    {
        private readonly IAgentRespository _agentRespository;

        public AgentController(IAgentRespository agentRespository)
        {
            _agentRespository = agentRespository;
        }
        [HttpGet("get-all")]
        public async Task<IActionResult> GetAllAgents()
        {
            var agents = await _agentRespository.GetAllAgents();
            if (agents == null || !agents.Any())
            {
                return NotFound("No agents found.");
            }
            return Ok(new BaseResponse<IEnumerable<Entities.Agents>>(agents, "Success!", true));
        }
    }
}
