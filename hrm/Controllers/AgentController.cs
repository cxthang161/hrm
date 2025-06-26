using hrm.Common;
using hrm.DTOs;
using hrm.Respository.Agents;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace hrm.Controllers
{
    [Route("api/agent")]
    [ApiController]

    public class AgentController : ControllerBase
    {
        private readonly IAgentRespository _agentRespository;

        public AgentController(IAgentRespository agentRespository)
        {
            _agentRespository = agentRespository;
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("get-all")]
        public async Task<IActionResult> GetAll()
        {
            var agents = await _agentRespository.GetAllAgents();
            if (agents == null || !agents.Any())
            {
                return NotFound("No agents found.");
            }
            return Ok(new BaseResponse<IEnumerable<Entities.Agents>>(agents, "Success!", true));
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("create")]
        public async Task<IActionResult> CreateAgent([FromBody] AgentDto agent)
        {
            if (agent == null)
            {
                return BadRequest("Invalid agent data.");
            }
            var (message, success) = await _agentRespository.CreateAgent(agent);
            if (!success)
            {
                return BadRequest(new BaseResponse<string>(null, message, false));
            }
            return Ok(new BaseResponse<string>(null, message, true));
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("delete/{agentId}")]
        public async Task<IActionResult> DeleteAgent(int agentId)
        {
            var (message, success) = await _agentRespository.DeleteAgent(agentId);
            if (!success)
            {
                return BadRequest(new BaseResponse<string>(null, message, false));
            }
            return Ok(new BaseResponse<string>(null, message, true));
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("update/{agentId}")]
        public async Task<IActionResult> UpdateAgent(int agentId, [FromBody] AgentDto agent)
        {
            if (agent == null)
            {
                return BadRequest("Invalid agent data.");
            }
            var (message, success) = await _agentRespository.UpdateAgent(agentId, agent);
            if (!success)
            {
                return BadRequest(new BaseResponse<string>(null, message, false));
            }
            return Ok(new BaseResponse<string>(null, message, true));
        }
    }
}
