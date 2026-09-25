using AutoCtor;
using System.Collections.Generic;

public interface INotifier;

public class EmailNotifier : INotifier;
public class SmsNotifier : INotifier;

#region ServiceProviderEnumerable

[ServiceProvider]
[Singleton<INotifier, EmailNotifier>]
[Singleton<INotifier, SmsNotifier>]
[Singleton<IAlertService, AlertService>]
public partial class AlertContainer;

public interface IAlertService;

public class AlertService : IAlertService
{
    public AlertService(IEnumerable<INotifier> notifiers) { }
}

#endregion
