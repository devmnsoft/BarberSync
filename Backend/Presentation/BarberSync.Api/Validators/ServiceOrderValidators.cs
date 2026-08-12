using BarberSync.Application.Operations;
using FluentValidation;

namespace BarberSync.Api.Validators;

public sealed class OpenServiceOrderRequestValidator : AbstractValidator<OpenServiceOrderRequest> { public OpenServiceOrderRequestValidator() => RuleFor(x=>x.ClientId).NotEmpty(); }
public sealed class AddServiceItemRequestValidator : AbstractValidator<AddServiceItemRequest> { public AddServiceItemRequestValidator() { RuleFor(x=>x.ServiceId).NotEmpty(); RuleFor(x=>x.ProfessionalId).NotEmpty(); RuleFor(x=>x.Quantity).GreaterThan(0); } }
public sealed class AddProductItemRequestValidator : AbstractValidator<AddProductItemRequest> { public AddProductItemRequestValidator() { RuleFor(x=>x.ProductId).NotEmpty(); RuleFor(x=>x.Quantity).GreaterThan(0); } }
public sealed class UpdateOrderItemRequestValidator : AbstractValidator<UpdateOrderItemRequest> { public UpdateOrderItemRequestValidator() { RuleFor(x=>x.Quantity).GreaterThan(0); RuleFor(x=>x.Discount).GreaterThanOrEqualTo(0); } }
public sealed class ApplyDiscountRequestValidator : AbstractValidator<ApplyDiscountRequest> { public ApplyDiscountRequestValidator() { RuleFor(x=>x.Amount).GreaterThan(0); RuleFor(x=>x.Reason).NotEmpty().MaximumLength(300); } }
public sealed class ApplyCouponRequestValidator : AbstractValidator<ApplyCouponRequest> { public ApplyCouponRequestValidator() => RuleFor(x=>x.Code).NotEmpty().MaximumLength(60); }
public sealed class ApplyCashbackRequestValidator : AbstractValidator<ApplyCashbackRequest> { public ApplyCashbackRequestValidator() => RuleFor(x=>x.Amount).GreaterThan(0); }
public sealed class RegisterPaymentRequestValidator : AbstractValidator<RegisterPaymentRequest>
{ public RegisterPaymentRequestValidator() { RuleFor(x=>x.IdempotencyKey).NotEmpty().MaximumLength(100); RuleFor(x=>x.Splits).NotEmpty(); RuleForEach(x=>x.Splits).ChildRules(s=>{s.RuleFor(x=>x.Method).Must(x=>new[]{"Cash","Pix","Debit","Credit"}.Contains(x,StringComparer.OrdinalIgnoreCase)).WithMessage("Forma de pagamento inválida.");s.RuleFor(x=>x.Amount).GreaterThan(0);s.RuleFor(x=>x).Must(x=>x.Method.Equals("Cash",StringComparison.OrdinalIgnoreCase)||x.ReceivedAmount is null).WithMessage("Valor recebido é exclusivo de pagamentos em dinheiro.");s.RuleFor(x=>x).Must(x=>x.ReceivedAmount is null||x.ReceivedAmount>=x.Amount).WithMessage("Valor recebido não pode ser menor que o valor aplicado.");}); } }
public sealed class RefundPaymentRequestValidator : AbstractValidator<RefundPaymentRequest> { public RefundPaymentRequestValidator() => RuleFor(x=>x.Reason).NotEmpty().MaximumLength(300); }
