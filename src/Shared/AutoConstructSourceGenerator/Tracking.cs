namespace AutoCtor;

public partial class AutoConstructSourceGenerator
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage(
        "Design", "CA1034:Nested types should not be visible",
        Justification = "Constants used in tests.")]
    public static class TrackingNames
    {
        public static string BuildProperties => nameof(BuildProperties);
        public static string TypeModels => nameof(TypeModels);
        public static string PostCtorMethods => nameof(PostCtorMethods);

        public static IReadOnlyCollection<string> AllTrackers { get; } = [
            BuildProperties,
            TypeModels,
            PostCtorMethods,
        ];
    }
}
