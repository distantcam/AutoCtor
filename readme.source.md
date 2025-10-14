# AutoCtor

[![Build Status](https://img.shields.io/github/actions/workflow/status/distantcam/autoctor/build.yml)](https://github.com/distantcam/AutoCtor/actions/workflows/build.yml)
[![NuGet Status](https://img.shields.io/nuget/v/AutoCtor.svg)](https://www.nuget.org/packages/AutoCtor/)
[![Nuget Downloads](https://img.shields.io/nuget/dt/autoctor.svg)](https://www.nuget.org/packages/AutoCtor/)


AutoCtor is a Roslyn Source Generator that will automatically create a constructor for your class for use with constructor Dependency Injection.

```diff
+[AutoConstruct]
public partial class AService
{
    private readonly IDataContext _dataContext;
    private readonly IDataService _dataService;
    private readonly IExternalService _externalService;
    private readonly ICacheService _cacheService;
    private readonly ICacheProvider _cacheProvider;
    private readonly IUserService _userService;

-    public AService(
-        IDataContext dataContext,
-        IDataService dataService,
-        IExternalService externalService,
-        ICacheService cacheService,
-        ICacheProvider cacheProvider,
-        IUserService userService
-    )
-    {
-        _dataContext = dataContext;
-        _dataService = dataService;
-        _externalService = externalService;
-        _cacheService = cacheService;
-        _cacheProvider = cacheProvider;
-        _userService = userService;
-    }
}
```

<a id='toc'></a>
toc

## NuGet packages

https://nuget.org/packages/AutoCtor/

## Usage

### Your code

snippet: Basic

### What gets generated

snippet: BasicGeneratedCode

<a href='#toc' title='Back to Contents'>Back to Contents</a>
## More Features

### Post constructor Initialisation

You can mark a method to be called at the end of the constructor with the attribute `[AutoPostConstruct]`. This method must return void.

snippet: PostConstruct

snippet: PostConstructGeneratedCode

### Keyed Services

When using `Microsoft.Extensions.DependencyInjection` you can mark fields and properties with `[AutoKeyedService]` and it will be included in the constructor.

snippet: KeyedService

snippet: KeyedServiceGeneratedCode

### Initialize with parameters

Post constructor methods can also take parameters. These parameters will be passed in from the constructor.

snippet: PostConstructWithParameters

snippet: PostConstructWithParametersGeneratedCode

### Initialize readonly fields with ref or out

If a parameter is marked `ref` or `out` and matches the type of a readonly field, it can set that field during construction.

snippet: PostConstructWithOutParameters

snippet: PostConstructWithOutParametersGeneratedCode

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

snippet: Guards

snippet: GuardsGeneratedCode

### Property Initialisation

AutoCtor can set properties that are considered as read only properties.

snippet: PropertyExamples

<a href='#toc' title='Back to Contents'>Back to Contents</a>
## More examples

You can also initialise readonly fields, and AutoCtor will not include them in the constructor.

snippet: PresetField

snippet: PresetFieldGeneratedCode

If there is a single base constructor with parameters, AutoCtor will include that base constructor in the constructor it creates.

snippet: Inherit

snippet: InheritGeneratedCode

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
