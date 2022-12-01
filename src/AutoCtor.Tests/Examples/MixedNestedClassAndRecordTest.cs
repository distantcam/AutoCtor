public partial class OuterClass1
{
    public partial record OuterRecord1
    {
        public partial class OuterClass2
        {
            [AutoConstruct]
            public partial record MixedNestedClassAndRecordTest
            {
                private readonly int _item;
            }
        }
    }
}
