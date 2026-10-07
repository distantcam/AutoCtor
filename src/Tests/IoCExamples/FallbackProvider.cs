using System;
using System.Collections.Generic;
using AutoCtor;

// Anything the provider has no registration for is asked of the fallback rather than coming
// back null. The member is read on the miss path only, so it can be assigned from a
// constructor of your own, and the provider never disposes what it does not own.
//
// Declaring a fallback also says that constructor dependencies may come from outside, so
// IReporter is wired to it instead of being reported by ACTR012. Nothing is known about
// what comes back, so it takes no part in the graph: no ordering, no lifetime checks.
//
// A collection is both containers' registrations, the fallback's first. IEnumerable<IAudit>
// is the case that shows why: nothing here is registered for it, and an empty array would
// have been a wrong answer rather than a missing one.
[ServiceProvider(Fallback = nameof(_host))]
[Singleton<IClock, Clock>]
[Singleton<ILedger, Ledger>]
public sealed partial class HostedProvider
{
    private readonly IServiceProvider _host;

    public HostedProvider(IServiceProvider host) => _host = host;
}

public interface IClock;
public interface IReporter;
public interface IAudit;
public interface ILedger;
public class Clock(IReporter reporter) : IClock
{
    public IReporter Reporter => reporter;
}
public class Ledger(IEnumerable<IAudit> audits) : ILedger
{
    public IEnumerable<IAudit> Audits => audits;
}
