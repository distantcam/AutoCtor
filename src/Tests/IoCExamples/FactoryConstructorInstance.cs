using AutoCtor;

// The provider may declare a constructor of its own: nothing is generated on it but field
// initializers, which run first whatever constructor you write. That is how an instance
// built elsewhere gets in, rather than having to be built in a field initializer.
[ServiceProvider]
[Singleton<IClock>(Factory = nameof(_clock))]
[Singleton<IReport, Report>]
public partial class ReportProvider
{
    private readonly IClock _clock;

    public ReportProvider(IClock clock) => _clock = clock;
}

public interface IClock;
public interface IReport;
public class Report(IClock clock) : IReport
{
    public IClock Clock => clock;
}
