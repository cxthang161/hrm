using hrm.Common;
using hrm.DTOs;
using hrm.Respository.Agents;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace hrm.Controllers
{
    [Route("api/agent")]
    [ApiController]
    [Authorize(Roles = "admin")]
    public class AgentController : ControllerBase
    {
        private readonly IAgentRespository _agentRespository;

        public AgentController(IAgentRespository agentRespository)
        {
            _agentRespository = agentRespository;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var agents = await _agentRespository.GetAllAgents();
            if (agents == null || !agents.Any())
            {
                return NotFound("No agents found.");
            }

            return Ok(new BaseResponse<IEnumerable<Entities.Agents>>(agents, "Success!", true));
        }

        [HttpPost]
        public async Task<IActionResult> CreateAgent([FromBody] AgentDto agent)
        {
            var (message, success) = await _agentRespository.CreateAgent(agent);
            if (!success)
            {
                return BadRequest(new BaseResponse<string>(null, message, false));
            }

            return Ok(new BaseResponse<string>(null, message, true));
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAgent(int id)
        {
            var (message, success) = await _agentRespository.DeleteAgent(id);
            if (!success)
            {
                return BadRequest(new BaseResponse<string>(null, message, false));
            }

            return Ok(new BaseResponse<string>(null, message, true));
        }

        [HttpPut]
        public async Task<IActionResult> UpdateAgent(int agentId, [FromBody] UpdateAgentDto agent)
        {
            var (message, success) = await _agentRespository.UpdateAgent(agentId, agent);
            if (!success)
            {
                return BadRequest(new BaseResponse<string>(null, message, false));
            }

            return Ok(new BaseResponse<string>(null, message, true));
        }
    }
}
