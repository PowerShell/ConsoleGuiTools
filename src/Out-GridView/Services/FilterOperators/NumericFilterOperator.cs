namespace OutGridView.Services.FilterOperators
{
    public abstract class NumericFilterOperator : IFilterOperator
    {
        public bool HasValue { get; } = true;
        public double Value { get; set; }
        public abstract bool Execute(string input);
    }
}