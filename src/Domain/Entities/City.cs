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
        Id = id;
        Name = name;
    }

    public static City Create(string name)
    {
        return new(NewId(), name);
    }

    public void AddRule(TaxRule rule)
    {
        _taxRules.Add(rule);
    }

    public TaxRule? GetRuleForYear(int year)
    {
        return _taxRules.FirstOrDefault(r => r.Year == year);
    }

    public void UpdateName(string name)
    {
        Name = name;
    }

    public override string ToString() => Name;
}
