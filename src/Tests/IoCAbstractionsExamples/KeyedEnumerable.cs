using AutoCtor;
using System.Collections.Generic;

// Keyed registrations are kept out of the unkeyed collection and grouped by key instead,
// so IEnumerable<IPolicy> here has one element, not three.
[ServiceProvider]
[Singleton<IPolicy, DefaultPolicy>]
[Singleton<IPolicy, StrictPolicy>(Key = "strict")]
[Singleton<IPolicy, LoosePolicy>(Key = "strict")]
[Singleton<IPolicyHost, PolicyHost>]
public partial class PolicyProvider;

public interface IPolicy;
public class DefaultPolicy : IPolicy;
public class StrictPolicy : IPolicy;
public class LoosePolicy : IPolicy;

public interface IPolicyHost;

public class PolicyHost : IPolicyHost
{
    public PolicyHost(IEnumerable<IPolicy> unkeyed) { }
}
