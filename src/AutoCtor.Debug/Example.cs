﻿using AutoCtor;

public interface IService { }

[AutoConstruct(nameof(Initialize))]
public partial class MyClass
{
    private readonly IService _service;

    private void Initialize()
    {
    }
}
