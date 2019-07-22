namespace OutGridView.Services.FilterOperators
{
    public class NotEqualsOperator : IFilterOperator
    {
        public bool HasValue { get; } = true;
        public string Value { get; set; }
        public bool Execute(string input)
        {
            return !input.Equals(Value);
        }
    }
}