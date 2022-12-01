namespace TestNamespace;

public partial class OuterClass1
{
    public partial class OuterClass2
    {
        [AutoConstruct]
        public partial class NamespaceDoubleNestedClassTest
        {
            private readonly int _item;
        }
    }
}
