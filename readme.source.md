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

## Examples

### Basic

snippet: Basic

<details><summary>What gets generated</summary>

snippet: Basic.cs#Basic.g.verified.cs

</details>

### Inherited

snippet: Inherited

<details><summary>What gets generated</summary>

snippet: Inherited.cs#Inherited.g.verified.cs

</details>

### Properties

snippet: Properties

<details><summary>What gets generated</summary>

snippet: Properties.cs#Properties.g.verified.cs

</details>

<a href='#toc' title='Back to Contents'>Back to Contents</a>
## Post Constructor Initialization

You can mark a method to be called at the end of the constructor with the attribute `[AutoPostConstruct]`. This method must return void.

snippet: PostConstruct

<details><summary>What gets generated</summary>

snippet: PostConstruct.cs#PostConstruct.g.verified.cs

</details>

### Extra Parameters

Post construct methods can also take parameters. The generated constructor will include these parameters.

snippet: PostConstructWithParameter

<details><summary>What gets generated</summary>

snippet: PostConstructWithParameter.cs#PostConstructWithParameter.g.verified.cs

</details>

### `out`/`ref` Parameters

Parameters marked with `out` or `ref` are set back to any fields that match the same type. This can be used to set readonly fields with more complex logic.

snippet: PostConstructWithOutParameter

<details><summary>What gets generated</summary>

snippet: PostConstructWithOutParameter.cs#PostConstructWithOutParameter.g.verified.cs

</details>

### Optional Parameters

snippet: PostConstructWithDefaultParameter

<details><summary>What gets generated</summary>

snippet: PostConstructWithDefaultParameter.cs#PostConstructWithDefaultParameter.g.verified.cs

</details>

<a href='#toc' title='Back to Contents'>Back to Contents</a>
## Argument Guards

Null guards for the arguments to the constructor can be added in 2 ways.

In your project you can add a `AutoCtorGuards` property.

```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <AutoCtorGuards>true</AutoCtorGuards>
  </PropertyGroup>

</Project>
```

**OR**

In each `AutoConstruct` attribute you can add a setting to enable/disable guards.

snippet: Guarded

<details><summary>What gets generated</summary>

snippet: Guarded.cs#Guarded.g.verified.cs

</details>

<a href='#toc' title='Back to Contents'>Back to Contents</a>
## Keyed Services

When using `Microsoft.Extensions.DependencyInjection` you can mark fields and properties with `[AutoKeyedService]` and it will be included in the constructor.

snippet: Keyed

<details><summary>What gets generated</summary>

snippet: Keyed.cs#Keyed.g.verified.cs

</details>

<a href='#toc' title='Back to Contents'>Back to Contents</a>
## Other

### Embedding The Attributes

By default, the `[AutoConstruct]` attributes referenced in your project are contained in an external dll. It is also possible to embed the attributes directly in your project. To do this, you must do two things:

1. Define the constant `AUTOCTOR_EMBED_ATTRIBUTES`. This ensures the attributes are embedded in your project.
2. Add `compile` to the list of excluded assets in your `<PackageReference>` element. This ensures the attributes in your project are referenced, instead of the _AutoCtor.Attributes.dll_ library.

Your project file should look like this:

```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <!--  Define the MSBuild constant    -->
    <DefineConstants>$(DefineConstants);AUTOCTOR_EMBED_ATTRIBUTES</DefineConstants>
  </PropertyGroup>

  <!-- Add the package -->
  <PackageReference Include="AutoCtor"
                    PrivateAssets="all"
                    ExcludeAssets="compile;runtime" />
  <!--                             ☝ Add compile to the list of excluded assets. -->

</Project>
```

<details><summary>What gets generated</summary>

snippet: GeneratedAttributeTests.cs#AutoConstructAttribute.g.verified.cs

</details>

### Keeping Attributes In Code

The `[AutoConstruct]` attributes are decorated with the `[Conditional]` attribute, so their usage will not appear in the build output of your project. If you use reflection at runtime you will not find the `[AutoConstruct]` attributes.

If you wish to preserve these attributes in the build output, add the define constant `AUTOCTOR_USAGES`.

```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <!--  Define the MSBuild constant    -->
    <DefineConstants>$(DefineConstants);AUTOCTOR_USAGES</DefineConstants>
  </PropertyGroup>

</Project>
```

<a href='#toc' title='Back to Contents'>Back to Contents</a>
## Stats

![Alt](https://repobeats.axiom.co/api/embed/8d02b2c004a5f958b4365abad3d4d1882dca200f.svg "Repobeats analytics image")
