<!--
GENERATED FILE - DO NOT EDIT
This file was generated by [MarkdownSnippets](https://github.com/SimonCropp/MarkdownSnippets).
Source File: /readme.source.md
To change this file edit the source file and then run MarkdownSnippets.
-->

# AutoCtor

[![Build Status](https://img.shields.io/github/actions/workflow/status/distantcam/autoctor/build.yml)](https://github.com/distantcam/AutoCtor/actions/workflows/build.yml)
[![NuGet Status](https://img.shields.io/nuget/v/AutoCtor.svg)](https://www.nuget.org/packages/AutoCtor/)
[![Nuget Downloads](https://img.shields.io/nuget/dt/autoctor.svg)](https://www.nuget.org/packages/AutoCtor/)


AutoCtor is a Roslyn Source Generator that will automatically create a constructor for your class for use with constructor Dependency Injection.

<a id='toc'></a>
<!-- toc -->
## Contents

  * [NuGet packages](#nuget-packages)
  * [Usage](#usage)
    * [Your code](#your-code)
    * [What gets generated](#what-gets-generated)
  * [More Features](#more-features)
    * [Post constructor Initialisation](#post-constructor-initialisation)
    * [Keyed Services](#keyed-services)
    * [Initialize with parameters](#initialize-with-parameters)
    * [Initialize readonly fields with ref or out](#initialize-readonly-fields-with-ref-or-out)
    * [Argument Guards](#argument-guards)
    * [Property Initialisation](#property-initialisation)
  * [More examples](#more-examples)
  * [Embedding the attributes in your project](#embedding-the-attributes-in-your-project)
  * [Preserving usage of the `[AutoConstruct]` attribute](#preserving-usage-of-the-autoconstruct-attribute)
  * [Stats](#stats)<!-- endToc -->

## NuGet packages

https://nuget.org/packages/AutoCtor/

## Usage

### Your code

<!-- snippet: Basic -->
<a id='snippet-Basic'></a>
```cs
[AutoConstruct]
public partial class ExampleClass
{
    private readonly IService _service;
}
```
<sup><a href='/src/AutoCtor.Example/BasicExamples.cs#L6-L14' title='Snippet source file'>snippet source</a> | <a href='#snippet-Basic' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

### What gets generated

<!-- snippet: BasicGeneratedCode -->
<a id='snippet-BasicGeneratedCode'></a>
```cs
public ExampleClass(IService service)
{
    _service = service;
}
```
<sup><a href='/src/AutoCtor.Example/BasicExamples.cs#L18-L23' title='Snippet source file'>snippet source</a> | <a href='#snippet-BasicGeneratedCode' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

<a href='#toc' title='Back to Contents'>Back to Contents</a>
## More Features

### Post constructor Initialisation

You can mark a method to be called at the end of the constructor with the attribute `[AutoPostConstruct]`. This method must return void.

<!-- snippet: PostConstruct -->
<a id='snippet-PostConstruct'></a>
```cs
[AutoConstruct]
public partial class PostConstructMethod
{
    private readonly IService _service;

    [AutoPostConstruct]
    private void Initialize()
    {
    }
}
```
<sup><a href='/src/AutoCtor.Example/PostConstructExamples.cs#L10-L23' title='Snippet source file'>snippet source</a> | <a href='#snippet-PostConstruct' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

<!-- snippet: PostConstructGeneratedCode -->
<a id='snippet-PostConstructGeneratedCode'></a>
```cs
public PostConstructMethod(IService service)
{
    _service = service;
    Initialize();
}
```
<sup><a href='/src/AutoCtor.Example/PostConstructExamples.cs#L27-L33' title='Snippet source file'>snippet source</a> | <a href='#snippet-PostConstructGeneratedCode' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

### Keyed Services

When using `Microsoft.Extensions.DependencyInjection` you can mark fields and properties with `[AutoKeyedService]` and it will be included in the constructor.

<!-- snippet: KeyedService -->
<a id='snippet-KeyedService'></a>
```cs
[AutoConstruct]
public partial class KeyedExampleClass
{
    [AutoKeyedService("key")]
    private readonly IService _keyedService;
}
```
<sup><a href='/src/AutoCtor.Example/BasicExamples.cs#L97-L106' title='Snippet source file'>snippet source</a> | <a href='#snippet-KeyedService' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

<!-- snippet: KeyedServiceGeneratedCode -->
<a id='snippet-KeyedServiceGeneratedCode'></a>
```cs
public KeyedExampleClass(
    [Microsoft.Extensions.DependencyInjection.FromKeyedServices("key")]
    IService keyedService)
{
    _keyedService = keyedService;
}
```
<sup><a href='/src/AutoCtor.Example/BasicExamples.cs#L110-L117' title='Snippet source file'>snippet source</a> | <a href='#snippet-KeyedServiceGeneratedCode' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

### Initialize with parameters

Post constructor methods can also take parameters. These parameters will be passed in from the constructor.

<!-- snippet: PostConstructWithParameters -->
<a id='snippet-PostConstructWithParameters'></a>
```cs
public partial class PostConstructMethodWithParameters
{
    private readonly IService _service;

    [AutoPostConstruct]
    private void Initialize(IInitialiseService initialiseService)
    {
    }
}
```
<sup><a href='/src/AutoCtor.Example/PostConstructExamples.cs#L36-L48' title='Snippet source file'>snippet source</a> | <a href='#snippet-PostConstructWithParameters' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

<!-- snippet: PostConstructWithParametersGeneratedCode -->
<a id='snippet-PostConstructWithParametersGeneratedCode'></a>
```cs
public PostConstructMethodWithParameters(IService service, IInitialiseService initialiseService)
{
    _service = service;
    Initialize(initialiseService);
}
```
<sup><a href='/src/AutoCtor.Example/PostConstructExamples.cs#L52-L58' title='Snippet source file'>snippet source</a> | <a href='#snippet-PostConstructWithParametersGeneratedCode' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

### Initialize readonly fields with ref or out

If a parameter is marked `ref` or `out` and matches the type of a readonly field, it can set that field during construction.

<!-- snippet: PostConstructWithOutParameters -->
<a id='snippet-PostConstructWithOutParameters'></a>
```cs
public partial class PostConstructWithOutParameters
{
    private readonly IService _service;
    private readonly IOtherService _otherService;

    [AutoPostConstruct]
    private void Initialize(IServiceFactory serviceFactory, out IService service)
    {
        service = serviceFactory.CreateService();
    }
}
```
<sup><a href='/src/AutoCtor.Example/PostConstructExamples.cs#L61-L75' title='Snippet source file'>snippet source</a> | <a href='#snippet-PostConstructWithOutParameters' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

<!-- snippet: PostConstructWithOutParametersGeneratedCode -->
<a id='snippet-PostConstructWithOutParametersGeneratedCode'></a>
```cs
public PostConstructWithOutParameters(IOtherService otherService, IServiceFactory serviceFactory)
{
    _otherService = otherService;
    Initialize(serviceFactory, out _service);
}
```
<sup><a href='/src/AutoCtor.Example/PostConstructExamples.cs#L79-L85' title='Snippet source file'>snippet source</a> | <a href='#snippet-PostConstructWithOutParametersGeneratedCode' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

### Argument Guards

Null guards for the arguments to the constructor can be added in 2 ways.

In your project you can add a `AutoCtorGuards` property.

```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <AutoCtorGuards>true</AutoCtorGuards>
  </PropertyGroup>

</Project>
```

In each `AutoConstruct` attribute you can add a setting to enable/disable guards.

<!-- snippet: Guards -->
<a id='snippet-Guards'></a>
```cs
[AutoConstruct(GuardSetting.Enabled)]
public partial class GuardedClass
{
    private readonly Service _service;
}
```
<sup><a href='/src/AutoCtor.Example/GuardsExamples.cs#L5-L13' title='Snippet source file'>snippet source</a> | <a href='#snippet-Guards' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

<!-- snippet: GuardsGeneratedCode -->
<a id='snippet-GuardsGeneratedCode'></a>
```cs
public GuardedClass(Service service)
{
    _service = service ?? throw new ArgumentNullException(nameof(service));
}
```
<sup><a href='/src/AutoCtor.Example/GuardsExamples.cs#L18-L23' title='Snippet source file'>snippet source</a> | <a href='#snippet-GuardsGeneratedCode' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

### Property Initialisation

AutoCtor can set properties that are considered as read only properties.

<!-- snippet: PropertyExamples -->
<a id='snippet-PropertyExamples'></a>
```cs
// AutoCtor will initialise these
public string GetProperty { get; }
protected string ProtectedProperty { get; }
public string InitProperty { get; init; }
public required string RequiredProperty { get; set; }

// AutoCtor will ignore these
public string InitializerProperty { get; } = "Constant";
public string GetSetProperty { get; set; }
public string FixedProperty => "Constant";
public string RedirectedProperty => InitializerProperty;
```
<sup><a href='/src/AutoCtor.Example/BasicExamples.cs#L80-L94' title='Snippet source file'>snippet source</a> | <a href='#snippet-PropertyExamples' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

<a href='#toc' title='Back to Contents'>Back to Contents</a>
## More examples

You can also initialise readonly fields, and AutoCtor will not include them in the constructor.

<!-- snippet: PresetField -->
<a id='snippet-PresetField'></a>
```cs
[AutoConstruct]
public partial class ClassWithPresetField
{
    private readonly IService _service;
    private readonly IList<string> _list = new List<string>();
}
```
<sup><a href='/src/AutoCtor.Example/BasicExamples.cs#L26-L35' title='Snippet source file'>snippet source</a> | <a href='#snippet-PresetField' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

<!-- snippet: PresetFieldGeneratedCode -->
<a id='snippet-PresetFieldGeneratedCode'></a>
```cs
public ClassWithPresetField(IService service)
{
    _service = service;
    // no code to set _list
}
```
<sup><a href='/src/AutoCtor.Example/BasicExamples.cs#L39-L45' title='Snippet source file'>snippet source</a> | <a href='#snippet-PresetFieldGeneratedCode' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

If there is a single base constructor with parameters, AutoCtor will include that base constructor in the constructor it creates.

<!-- snippet: Inherit -->
<a id='snippet-Inherit'></a>
```cs
public abstract class BaseClass
{
    protected IAnotherService _anotherService;

    public BaseClass(IAnotherService anotherService)
    {
        _anotherService = anotherService;
    }
}

[AutoConstruct]
public partial class ClassWithBase : BaseClass
{
    private readonly IService _service;
}
```
<sup><a href='/src/AutoCtor.Example/BasicExamples.cs#L48-L66' title='Snippet source file'>snippet source</a> | <a href='#snippet-Inherit' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

<!-- snippet: InheritGeneratedCode -->
<a id='snippet-InheritGeneratedCode'></a>
```cs
public ClassWithBase(IAnotherService anotherService, IService service) : base(anotherService)
{
    _service = service;
}
```
<sup><a href='/src/AutoCtor.Example/BasicExamples.cs#L70-L75' title='Snippet source file'>snippet source</a> | <a href='#snippet-InheritGeneratedCode' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

<a href='#toc' title='Back to Contents'>Back to Contents</a>
## Embedding the attributes in your project

By default, the `[AutoConstruct]` attributes referenced in your project are contained in an external dll. It is also possible to embed the attributes directly in your project. To do this, you must do two things:

1. Define the MSBuild constant `AUTOCTOR_EMBED_ATTRIBUTES`. This ensures the attributes are embedded in your project.
2. Add `compile` to the list of excluded assets in your `<PackageReference>` element. This ensures the attributes in your project are referenced, instead of the _AutoCtor.Attributes.dll_ library.

Your project file should look like this:

```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <!--  Define the MSBuild constant    -->
    <DefineConstants>AUTOCTOR_EMBED_ATTRIBUTES</DefineConstants>
  </PropertyGroup>

  <!-- Add the package -->
  <PackageReference Include="AutoCtor"
                    PrivateAssets="all"
                    ExcludeAssets="compile;runtime" />
<!--                               ☝ Add compile to the list of excluded assets. -->

</Project>
```

<a href='#toc' title='Back to Contents'>Back to Contents</a>
## Preserving usage of the `[AutoConstruct]` attribute

The `[AutoConstruct]` attributes are decorated with the `[Conditional]` attribute, so their usage will not appear in the build output of your project. If you use reflection at runtime you will not find the `[AutoConstruct]` attributes.

If you wish to preserve these attributes in the build output, you can define the `AUTOCTOR_USAGES` MSBuild variable.

```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <!--  Define the MSBuild constant    -->
    <DefineConstants>AUTOCTOR_USAGES</DefineConstants>
  </PropertyGroup>

  <!-- Add the package -->
  <PackageReference Include="AutoCtor" PrivateAssets="all" />

</Project>
```

<a href='#toc' title='Back to Contents'>Back to Contents</a>
## Stats

![Alt](https://repobeats.axiom.co/api/embed/8d02b2c004a5f958b4365abad3d4d1882dca200f.svg "Repobeats analytics image")
