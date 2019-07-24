namespace OutGridView.Services.FilterOperators
{
    public interface IStringFilterOperator : IFilterOperator
    {
        string Value { get; }
    }
}