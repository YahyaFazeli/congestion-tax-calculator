using Domain.Base;

namespace Domain.Entities;

public sealed class City : Entity
{
    public const string TaxRulesFieldName = nameof(_taxRules);

    public string Name { get; private set; } = string.Empty;

    private readonly List<TaxRule> _taxRules = [];
    public IReadOnlyCollection<TaxRule> TaxRules => _taxRules;

    private City() { }

    public City(Guid id, string name)
    {
        if (id == Guid.Empty)
            throw new ArgumentException("City ID cannot be empty.", nameof(id));

        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("City name cannot be null or whitespace.", nameof(name));

        if (name.Length > 100)
            throw new ArgumentException("City name cannot exceed 100 characters.", nameof(name));

        Id = id;
        Name = name;
    }

    public static City Create(string name)
    {
        return new(NewId(), name);
    }

    public void AddRule(TaxRule rule)
    {
        if (rule is null)
            throw new ArgumentNullException(nameof(rule), "Tax rule cannot be null.");

        if (rule.CityId != Id)
            throw new ArgumentException(
                $"Tax rule belongs to a different city. Expected CityId: {Id}, Actual: {rule.CityId}",
                nameof(rule)
            );

        if (_taxRules.Any(r => r.Year == rule.Year))
            throw new InvalidOperationException(
                $"A tax rule for year {rule.Year} already exists for this city."
            );

        _taxRules.Add(rule);
    }

    public TaxRule? GetRuleForYear(int year)
    {
        return _taxRules.FirstOrDefault(r => r.Year == year);
    }

    public void UpdateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("City name cannot be null or whitespace.", nameof(name));

        if (name.Length > 100)
            throw new ArgumentException("City name cannot exceed 100 characters.", nameof(name));

        Name = name;
    }

    public override string ToString() => Name;
}
