using FluentValidation;
using GoodHamburger.Application.DTOs;

namespace GoodHamburger.Application.Validators;

public class CreateOrderRequestValidator : AbstractValidator<CreateOrderRequest>
{
    public CreateOrderRequestValidator()
    {
        RuleFor(x => x.MenuItemIds)
            .NotNull().WithMessage("Lista de itens é obrigatória.")
            .Must(ids => ids != null && ids.Count > 0).WithMessage("Informe ao menos um item do cardápio.")
            .Must(ids => ids == null || ids.Count == ids.Distinct().Count())
                .WithMessage("Itens duplicados não são permitidos.");
    }
}

public class UpdateOrderRequestValidator : AbstractValidator<UpdateOrderRequest>
{
    public UpdateOrderRequestValidator()
    {
        RuleFor(x => x.MenuItemIds)
            .NotNull().WithMessage("Lista de itens é obrigatória.")
            .Must(ids => ids != null && ids.Count > 0).WithMessage("Informe ao menos um item do cardápio.")
            .Must(ids => ids == null || ids.Count == ids.Distinct().Count())
                .WithMessage("Itens duplicados não são permitidos.");
    }
}
