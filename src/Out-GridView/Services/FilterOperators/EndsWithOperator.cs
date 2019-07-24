namespace OutGridView.Services.FilterOperators
{
    public class EndsWithOperator : IStringFilterOperator
    {
        public bool HasValue { get; } = true;
        public string Value { get; set; }
        public bool Execute(string input)
        {
            return input.EndsWith(Value);
        }
    }
}