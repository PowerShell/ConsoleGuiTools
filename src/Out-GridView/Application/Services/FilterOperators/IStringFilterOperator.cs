namespace OutGridView.Application.Services.FilterOperators
{
    public interface IStringFilterOperator : IFilterOperator
    {
        string Value { get; }
    }
}