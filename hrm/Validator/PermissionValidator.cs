using FluentValidation;
using hrm.DTOs;

namespace hrm.Validator
{
    public class PermissionValidator : AbstractValidator<PermissionDto>
    {
        public PermissionValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Name name is required")
                .MinimumLength(3).WithMessage("Name must be at least 3 characters long");
            RuleFor(x => x.Description)
                .NotEmpty().WithMessage("Description is required")
                .MinimumLength(5).WithMessage("Description must be at least 5 characters long");
        }
    }
}
