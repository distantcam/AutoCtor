namespace Benchmarks;

public interface IScoped1
{
    void Do1();
}

public interface IScoped2
{
    void Do2();
}

public interface IScoped3
{
    void Do3();
}

public class Scoped1 : IScoped1
{
    public void Do1() { }
}

public class Scoped2 : IScoped2
{
    public void Do2() { }
}

public class Scoped3 : IScoped3
{
    public void Do3() { }
}
