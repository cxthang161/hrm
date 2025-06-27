using FluentValidation;
using hrm.DTOs;

namespace hrm.Validator
{
    public class AgentValidator : AbstractValidator<AgentDto>
    {
        public AgentValidator()
        {
            RuleFor(x => x.AgentCode)
                .NotEmpty().WithMessage("Agent code is required")
                .MinimumLength(3).WithMessage("Agent code must be at least 3 characters long");
            RuleFor(x => x.AgentName)
                .NotEmpty().WithMessage("Agent name is required")
                .MinimumLength(3).WithMessage("Agent name must be at least 3 characters long");
        }
    }
    public class UpdateAgentValidator : AbstractValidator<UpdateAgentDto>
    {
        public UpdateAgentValidator()
        {
            RuleFor(x => x.AgentName)
                .NotEmpty().WithMessage("Agent name is required")
                .Length(3).WithMessage("Agent name must be 3 characters");
        }
    }
}
