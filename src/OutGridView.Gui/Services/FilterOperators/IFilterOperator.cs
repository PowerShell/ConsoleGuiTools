namespace OutGridView.Application.Services.FilterOperators
{
    public interface IFilterOperator
    {
        bool HasValue { get; }
        bool Execute(string input);
        string GetPowerShellString();
    }
}