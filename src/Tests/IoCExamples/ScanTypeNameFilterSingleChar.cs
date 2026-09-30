using AutoCtor;

// ? in a TypeNameFilter matches exactly one character.
[ServiceProvider]
[ScanSingleton(typeof(IStage), TypeNameFilter = "Stage?")]
public sealed partial class StageProvider;

public interface IStage;
public class Stage1 : IStage;
public class Stage2 : IStage;
public class Stage10 : IStage;
public class Stage : IStage;
