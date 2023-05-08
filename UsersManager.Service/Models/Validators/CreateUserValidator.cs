using FluentValidation;

namespace UsersManager.Service.Models.Validators;

public class CreateUserValidator : AbstractValidator<CreateUserRequestDto>
{
    public CreateUserValidator()
    {
        RuleFor(x => x.Login)
            .NotEmpty()
            .Length(5, 30);
        RuleFor(x => x.Password)
            .NotEmpty()
            .Length(5, 60);
        RuleFor(x => x.Group)
            .IsInEnum();
    }
}