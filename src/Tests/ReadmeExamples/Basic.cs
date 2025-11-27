using System.Collections.Generic;
using AutoCtor;

public interface IService;

#region Basic

[AutoConstruct]
public partial class Basic
{
    private readonly IService _service;
    private readonly IList<string> _list = new List<string>();
}

#endregion
