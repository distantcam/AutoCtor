public class Nested
{
    class NoDeclaredVisibility
    {
        private readonly string _value;

        public NoDeclaredVisibility(string value)
        {
            _value = value;
        }
    }
}
