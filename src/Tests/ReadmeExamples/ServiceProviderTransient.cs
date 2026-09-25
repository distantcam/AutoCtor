using AutoCtor;

public interface IConnection;
public interface ICommand;

public class Connection : IConnection;

#region ServiceProviderTransient

[ServiceProvider]
[Singleton<IConnection, Connection>]
[Transient<ICommand, Command>]
public partial class CommandContainer;

[AutoConstruct]
public partial class Command : ICommand
{
    private readonly IConnection _connection;
}

#endregion
