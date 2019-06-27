namespace OutGridView.Services.FilterOperators
{
    public interface IFilterOperatorLookup
    {
        string Value { get; set; }
        bool Execute(string input);
    }
}