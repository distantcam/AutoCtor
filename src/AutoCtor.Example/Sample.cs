namespace AutoCtor.Example;

public class Sample
{
    public Sample()
    {
        var target = new TargetClass(0, "", false, new ExampleClass(), new ExampleRecord(), new ExampleInterfaceClass(), new ExampleInterfaceClass());
    }
}

public class ExampleClass { }
public record ExampleRecord;

public interface IExampleInterface { }
public class ExampleInterfaceClass : IExampleInterface { }

[AutoConstructAttribute]
public partial class TargetClass
{
    private readonly int _number;
    private readonly string _string;
    private readonly bool _bool;

    private readonly ExampleClass _exampleClass;
    private readonly ExampleRecord _exampleRecord;
    private readonly IExampleInterface _exampleInterface;
    private readonly ExampleInterfaceClass _exampleInterfaceClass;

    private int _not_included;

    public TargetClass(int included, int number, string @string, bool @bool, ExampleClass exampleClass, ExampleRecord exampleRecord, IExampleInterface exampleInterface, ExampleInterfaceClass exampleInterfaceClass) :
        this(number, @string, @bool, exampleClass, exampleRecord, exampleInterface, exampleInterfaceClass)
    {
    }
}
