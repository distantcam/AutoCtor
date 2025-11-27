using AutoCtor;

public class Service;

#region PostConstructWithDefaultParameter

[AutoConstruct]
public partial class PostConstructWithDefaultParameter
{
    [AutoPostConstruct]
    private void Initialize(Service? service = default)
    {
    }
}

#endregion
