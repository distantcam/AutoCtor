using AutoCtor;

[ServiceProvider]
[Singleton<IServiceOne, ServiceOne>]
[Singleton<IServiceTwo, ServiceTwo>]
[Singleton<IServiceThree, ServiceThree>]
public partial class MultipleServicesProvider;

public interface IServiceOne;
public interface IServiceTwo;
public interface IServiceThree;
public class ServiceOne : IServiceOne;
public class ServiceTwo : IServiceTwo;
public class ServiceThree : IServiceThree;
