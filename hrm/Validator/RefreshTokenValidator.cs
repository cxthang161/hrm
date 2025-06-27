using FluentValidation;
using hrm.DTOs;

namespace hrm.Validator
{
    public class RefreshTokenValidator : AbstractValidator<RefreshTokenResponseDto>
    {
        public RefreshTokenValidator()
        {
            RuleFor(x => x.AccessToken)
                .NotEmpty().WithMessage("AccessToken is required");

            RuleFor(x => x.RefreshToken)
                .NotEmpty().WithMessage("RefreshToken is required");
        }
    }
}
