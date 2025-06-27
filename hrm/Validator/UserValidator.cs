using FluentValidation;
using hrm.DTOs;

namespace hrm.Validator
{
    public class CreateUserValidator : AbstractValidator<CreateUserDto>
    {
        public CreateUserValidator()
        {
            RuleFor(x => x.UserName)
                .NotEmpty().WithMessage("Username is required")
                .MinimumLength(5).WithMessage("Username must be at least 5 characters long");

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Password is required")
                .MinimumLength(8).WithMessage("Password must be at least 8 characters long");
            RuleFor(x => x.AgentId)
                .NotEmpty().WithMessage("AgentId is required");
            RuleFor(x => x.Permissions)
                .NotEmpty().WithMessage("Permissions are required")
                .Must(permissions => permissions.Count > 0).WithMessage("At least one permission is required");
        }
    }

    public class UpdateUserValidator : AbstractValidator<UpdateUserDto>
    {
        public UpdateUserValidator()
        {
            RuleFor(x => x.UserName)
                .NotEmpty().WithMessage("Username is required")
                .MinimumLength(5).WithMessage("Username must be at least 5 characters long");
            RuleFor(x => x.AgentId)
                .NotEmpty().WithMessage("AgentId is required");
            RuleFor(x => x.Permissions)
                .NotEmpty().WithMessage("Permissions are required")
                .Must(permissions => permissions.Count > 0).WithMessage("At least one permission is required");
        }
    }

    public class ChangePasswordValidator : AbstractValidator<ChangePasswordDto>
    {
        public ChangePasswordValidator()
        {
            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Password is required")
                .MinimumLength(8).WithMessage("Password must be at least 8 characters long");
        }
    }

    public class UserLoginValidator : AbstractValidator<UserLoginDto>
    {
        public UserLoginValidator()
        {
            RuleFor(x => x.UserName)
                .NotEmpty().WithMessage("Username is required");
            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Password is required");
        }
    }
}
