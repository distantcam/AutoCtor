namespace Benchmarks;

public interface ISingleton1
{
    void Do1();
}

public interface ISingleton2
{
    void Do2();
}

public interface ISingleton3
{
    void Do3();
}

public class Singleton1 : ISingleton1
{
    public void Do1() { }
}

public class Singleton2 : ISingleton2
{
    public void Do2() { }
}

public class Singleton3 : ISingleton3
{
    public void Do3() { }
}
