﻿//HintName: PostCtorWithKeyedServiceTest.g.cs
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by https://github.com/distantcam/AutoCtor
// </auto-generated>
//------------------------------------------------------------------------------

partial class PostCtorWithKeyedServiceTest
{
	[global::System.Runtime.CompilerServices.CompilerGeneratedAttribute]
	[global::System.Diagnostics.DebuggerNonUserCodeAttribute]
	public PostCtorWithKeyedServiceTest(
		[global::Microsoft.Extensions.DependencyInjection.FromKeyedServices("base")] global::IService service,
		[global::Microsoft.Extensions.DependencyInjection.FromKeyedServices("field")] global::IService service0,
		[global::Microsoft.Extensions.DependencyInjection.FromKeyedServices("postconstruct")] global::IService postConstructService
	) : base(service)
	{
		this._service = service0;
		Initialize(postConstructService);
	}
}
