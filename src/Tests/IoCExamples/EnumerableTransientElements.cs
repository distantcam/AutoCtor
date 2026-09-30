using AutoCtor;
using System.Collections.Generic;

// A transient consumer of a transient collection rebuilds every element on each resolve.
[ServiceProvider]
[Transient<IRule, RuleOne>]
[Transient<IRule, RuleTwo>]
[Transient<IRuleSet, RuleSet>]
public sealed partial class RuleSetProvider;

public interface IRule;
public class RuleOne : IRule;
public class RuleTwo : IRule;

public interface IRuleSet;

public class RuleSet : IRuleSet
{
    public RuleSet(IEnumerable<IRule> rules) { }
}
