namespace Benchmarks;

public interface ITransient1
{
    void Do1();
}

public interface ITransient2
{
    void Do2();
}

public interface ITransient3
{
    void Do3();
}

public class Transient1 : ITransient1
{
    public void Do1() { }
}

public class Transient2 : ITransient2
{
    public void Do2() { }
}

public class Transient3 : ITransient3
{
    public void Do3() { }
}
