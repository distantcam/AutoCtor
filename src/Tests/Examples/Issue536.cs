public partial interface IInterface
{
    [AutoCtor.AutoConstruct]
    public partial class NestedClass
    {
        public IInterface Item { get; }
    }
}
