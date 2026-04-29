public class MultipleNested
{
    public class Parent
    {
        public class Child
        {
            private readonly string _value;

            public Child(string value)
            {
                _value = value;
            }
        }
    }
}
