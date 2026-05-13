using FluentValidation;
using CadastreInvent.Registry.Application.Commands;
using System.Text.RegularExpressions;

namespace CadastreInvent.Registry.Application.Validators
{
    public class RegisterPartyCommandValidator : AbstractValidator<RegisterPartyCommand>
    {
        public RegisterPartyCommandValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty()
                .MaximumLength(255);

            RuleFor(x => x.Type)
                .IsInEnum();

            RuleFor(x => x.ExtId)
                .NotEmpty();

            When(x => x.GovRegNumType == "СНИЛС", () =>
            {
                RuleFor(x => x.ExtId)
                    .Must(x => Regex.IsMatch(x.Replace("-", "").Replace(" ", ""), @"^\d{11}$"))
                    .WithMessage("СНИЛС должен состоять из 11 цифр.");
            });

            When(x => x.GovRegNumType == "ИНН_ФЛ", () =>
            {
                RuleFor(x => x.ExtId)
                    .Must(x => Regex.IsMatch(x.Replace("-", "").Replace(" ", ""), @"^\d{12}$"))
                    .WithMessage("ИНН физического лица должен состоять из 12 цифр.");
            });

            When(x => x.GovRegNumType == "ИНН_ЮЛ", () =>
            {
                RuleFor(x => x.ExtId)
                    .Must(x => Regex.IsMatch(x.Replace("-", "").Replace(" ", ""), @"^\d{10}$"))
                    .WithMessage("ИНН юридического лица должен состоять из 10 цифр.");
            });

            When(x => x.GovRegNumType == "ОГРН", () =>
            {
                RuleFor(x => x.ExtId)
                    .Must(x => Regex.IsMatch(x.Replace("-", "").Replace(" ", ""), @"^\d{13}$"))
                    .WithMessage("ОГРН должен состоять из 13 цифр.");
            });

            When(x => x.GovRegNumType == "ОГРНИП", () =>
            {
                RuleFor(x => x.ExtId)
                    .Must(x => Regex.IsMatch(x.Replace("-", "").Replace(" ", ""), @"^\d{15}$"))
                    .WithMessage("ОГРНИП должен состоять из 15 цифр.");
            });

            RuleFor(x => x.ContactInfo)
                .NotEmpty();

            When(x => x.ContactType == "Телефон", () =>
            {
                RuleFor(x => x.ContactInfo)
                    .Must(x => Regex.IsMatch(x.Replace("-", "").Replace(" ", "").Replace("(", "").Replace(")", "").Replace("+", ""), @"^\d{10,15}$"))
                    .WithMessage("Неверный формат телефона.");
            });

            When(x => x.ContactType == "Адрес", () =>
            {
                RuleFor(x => x.ContactInfo)
                    .MinimumLength(5)
                    .WithMessage("Неверный формат адреса.");
            });
        }
    }
}