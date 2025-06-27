using FluentValidation;
using hrm.DTOs;

namespace hrm.Validator
{
    public class ConfigValidator : AbstractValidator<ConfigDto>
    {
        public ConfigValidator()
        {
            RuleFor(x => x.ConfigFile)
                .NotEmpty().WithMessage("Config file is required");
            RuleFor(x => x.Logo)
                .NotEmpty().WithMessage("Logo is required");
            RuleFor(x => x.Background)
                .NotEmpty().WithMessage("Background is required");
            RuleFor(x => x.AgentId)
                .NotEmpty().WithMessage("AgentId is required")
                .GreaterThan(0).WithMessage("AgentId must be greater than 0");
            RuleFor(x => x.NameTemplate)
                .NotEmpty().WithMessage("Name template is required")
                .MinimumLength(3).WithMessage("Name template must be at least 3 characters long");
        }
    }

    public class ConfigUpdateValidator : AbstractValidator<ConfigUpdateDto>
    {
        public ConfigUpdateValidator()
        {
            RuleFor(x => x.AgentId)
                .NotEmpty().WithMessage("AgentId is required")
                .GreaterThan(0).WithMessage("AgentId must be greater than 0");
        }
    }
}
