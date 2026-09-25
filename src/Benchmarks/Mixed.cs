namespace Benchmarks;

public interface IMix1
{
    void Do1();
}

public interface IMix2
{
    void Do2();
}

public interface IMix3
{
    void Do3();
}

public class Mix1(ISingleton1 singleton1, ITransient1 transient1) : IMix1
{
    public void Do1()
    {
        singleton1.Do1();
        transient1.Do1();
    }
}

public class Mix2(ISingleton2 singleton2, ITransient2 transient2) : IMix2
{
    public void Do2()
    {
        singleton2.Do2();
        transient2.Do2();
    }
}

public class Mix3(ISingleton3 singleton3, ITransient3 transient3) : IMix3
{
    public void Do3()
    {
        singleton3.Do3();
        transient3.Do3();
    }
}
