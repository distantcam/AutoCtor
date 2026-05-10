public class NestedTypeWithConstructor
{
    public class Inner
    {
        private readonly string _value;

        public Inner(string value)
        {
            _value = value;
        }
    }
}
