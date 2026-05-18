using FluentValidation;
using ProductAPI.Application.DTOs.Products;

namespace ProductAPI.Application.Validators;

public class UpdateProductValidator : AbstractValidator<UpdateProductDto>
{
    public UpdateProductValidator()
    {
        RuleFor(x => x.ProductName)
            .NotEmpty().WithMessage("Product name is required.")
            .MaximumLength(255).WithMessage("Product name must not exceed 255 characters.");
    }
}