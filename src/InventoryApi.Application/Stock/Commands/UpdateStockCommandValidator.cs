using FluentValidation;
using InventoryApi.Domain.Enums;

namespace InventoryApi.Application.Stock.Commands;

public class UpdateStockCommandValidator : AbstractValidator<UpdateStockCommand>
{
    public UpdateStockCommandValidator()
    {
        RuleFor(x => x.ProductId).GreaterThan(0);
        RuleFor(x => x.Type).IsInEnum();
        RuleFor(x => x.Quantity)
            .GreaterThan(0)
            .When(x => x.Type is MovementType.In or MovementType.Out);
        RuleFor(x => x.Reference)
            .NotEmpty()
            .When(x => x.Type == MovementType.In);
        RuleFor(x => x.Reason)
            .NotEmpty()
            .When(x => x.Type == MovementType.Adjustment);
    }
}
