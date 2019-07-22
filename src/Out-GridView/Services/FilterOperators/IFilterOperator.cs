namespace OutGridView.Services.FilterOperators
{
    public interface IFilterOperator
    {
        bool HasValue { get; }
        object Value { get; set; }
        bool Execute(string input);
    }
}