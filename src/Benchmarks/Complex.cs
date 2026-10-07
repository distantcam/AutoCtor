namespace Benchmarks;

public interface IComplex1
{
    void Do1();
}

public interface IComplex2
{
    void Do2();
}

public interface IComplex3
{
    void Do3();
}

public interface IService1
{
    void Do1();
}

public interface IService2
{
    void Do2();
}

public interface IService3
{
    void Do3();
}

public class Service1(ITransient1 transient1) : IService1
{
    public void Do1()
    {
        transient1.Do1();
    }
}

public class Service2(ITransient2 transient2) : IService2
{
    public void Do2()
    {
        transient2.Do2();
    }
}

public class Service3(ITransient3 transient3) : IService3
{
    public void Do3()
    {
        transient3.Do3();
    }
}

public class Complex1(
    IService1 service1,
    IService2 service2,
    IService3 service3,
    IMix1 mix1,
    IMix2 mix2,
    IMix3 mix3,
    ISingleton1 singleton1,
    ITransient1 transient1
) : IComplex1
{
    public void Do1()
    {
        service1.Do1();
        service2.Do2();
        service3.Do3();
        mix1.Do1();
        mix2.Do2();
        mix3.Do3();
        singleton1.Do1();
        transient1.Do1();
    }
}

public class Complex2(
    IService1 service1,
    IService2 service2,
    IService3 service3,
    IMix1 mix1,
    IMix2 mix2,
    IMix3 mix3,
    ISingleton2 singleton2,
    ITransient2 transient2
) : IComplex2
{
    public void Do2()
    {
        service1.Do1();
        service2.Do2();
        service3.Do3();
        mix1.Do1();
        mix2.Do2();
        mix3.Do3();
        singleton2.Do2();
        transient2.Do2();
    }
}

public class Complex3(
    IService1 service1,
    IService2 service2,
    IService3 service3,
    IMix1 mix1,
    IMix2 mix2,
    IMix3 mix3,
    ISingleton3 singleton3,
    ITransient3 transient3
) : IComplex3
{
    public void Do3()
    {
        service1.Do1();
        service2.Do2();
        service3.Do3();
        mix1.Do1();
        mix2.Do2();
        mix3.Do3();
        singleton3.Do3();
        transient3.Do3();
    }
}
