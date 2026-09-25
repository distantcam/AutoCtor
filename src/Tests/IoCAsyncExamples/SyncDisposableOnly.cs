using AutoCtor;
using System;

// Everything owned is only synchronously disposable, on a target framework that has
// IAsyncDisposable. DisposeAsync is still part of the surface, because Microsoft's provider
// always has one, but it has nothing to await -- so it is written without the state machine
// an unused async would cost.
[ServiceProvider]
[Singleton<ILedger, Ledger>]
public partial class LedgerProvider;

public interface ILedger;

public class Ledger : ILedger, IDisposable
{
    public void Dispose() { }
}
