namespace Application.Commands.CalculateTax;

public sealed record CalculateTaxResult(decimal TotalTax, string Currency = "SEK");
