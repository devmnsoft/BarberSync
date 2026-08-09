namespace BarberSync.Domain.Enums;

public enum AppointmentStatus { Scheduled, Confirmed, CheckedIn, InService, AwaitingPayment, Finished, Cancelled, NoShow }
public enum ServiceOrderStatus { Open, AwaitingPayment, Paid, Cancelled }
public enum PaymentStatus { Pending, Confirmed, Refunded, Failed, Cancelled }
public enum PaymentMethod { Cash, Pix, DebitCard, CreditCard }
public enum CashRegisterStatus { Open, Closed }
public enum CashTransactionType { Open, Supply, Withdrawal, Payment, Expense, Refund, Close }
public enum StockMovementType { Entry, Sale, InternalConsumption, Loss, PositiveAdjustment, NegativeAdjustment, Return, Transfer }
public enum CommissionStatus { Pending, Available, Paid, Reversed }
