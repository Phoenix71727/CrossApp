using Api.Contracts;
using FluentValidation;

namespace Api.Validators;

public sealed class SaveOrderLineRequestValidator : AbstractValidator<SaveOrderLineRequest>
{
    public SaveOrderLineRequestValidator()
    {
        RuleFor(x => x.ProductId)
            .NotEmpty().WithMessage("Ідентифікатор товару обов'язковий.")
            .MaximumLength(50).WithMessage("Ідентифікатор товару не може перевищувати 50 символів.");

        RuleFor(x => x.ProductName)
            .NotEmpty().WithMessage("Назва товару обов'язкова.")
            .MaximumLength(200).WithMessage("Назва товару не може перевищувати 200 символів.");

        RuleFor(x => x.Price)
            .GreaterThanOrEqualTo(0).WithMessage("Ціна товару не може бути від'ємною.");

        RuleFor(x => x.Quantity)
            .GreaterThan(0).WithMessage("Кількість товару має бути більшою за нуль.");
    }
}

public sealed class SaveOrderRequestValidator : AbstractValidator<SaveOrderRequest>
{
    public SaveOrderRequestValidator()
    {
        RuleFor(x => x.CustomerId)
            .NotEmpty().WithMessage("Ідентифікатор клієнта обов'язковий.")
            .MaximumLength(100).WithMessage("Ідентифікатор клієнта не може перевищувати 100 символів.");

        RuleFor(x => x.Lines)
            .NotEmpty().WithMessage("Список позицій замовлення не може бути порожнім.");

        RuleForEach(x => x.Lines)
            .SetValidator(new SaveOrderLineRequestValidator());
    }
}
