using FluentValidation;

namespace SuperShop.GrpcServer.Validation
{
    public class EditProductRequestValidator : AbstractValidator<EditProductRequest>
    {
        public EditProductRequestValidator()
        {
            RuleFor(request => request.CategoryId).GreaterThanOrEqualTo(1).LessThanOrEqualTo(8).WithMessage("Invalid category");
            RuleFor(m => m.UnitsInStock).GreaterThan(0).LessThanOrEqualTo(500).WithMessage("Stock invalid");
            RuleFor(m => m.ProductName).NotEmpty().WithMessage("Name cannot be empty");
        }
    }
}
