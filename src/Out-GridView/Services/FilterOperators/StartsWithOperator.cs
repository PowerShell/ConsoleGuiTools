namespace OutGridView.Services.FilterOperators
{
    public class StartsWithOperator : IStringFilterOperator
    {
        public bool HasValue { get; } = true;
        public string Value { get; set; }
        public bool Execute(string input)
        {
            return input.StartsWith(Value);
        }
    }
}