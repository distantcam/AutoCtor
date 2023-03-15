# AutoCtor

[![Build Status](https://img.shields.io/github/actions/workflow/status/distantcam/autoctor/on-push-run-tests.yml?branch=main)](https://github.com/distantcam/AutoCtor/actions/workflows/on-push-run-tests.yml)
[![NuGet Status](https://img.shields.io/nuget/v/AutoCtor.svg)](https://www.nuget.org/packages/AutoCtor/)

AutoCtor is a Roslyn Source Generator that will automatically create a constructor for your class for use with constructor Dependency Injection.

## NuGet packages

https://nuget.org/packages/AutoCtor/

## Usage

### Your code

snippet: YourCode

### What gets generated

snippet: GeneratedCode

### More examples

You can also initialize readonly fields, and AutoCtor will not include them in the constructor.

snippet: ExampleWithInitializer

snippet: ExampleWithInitializerGeneratedCode

If there is a single base constructor with parameters, AutoCtor will include that base constructor in the constructor it creates.

snippet: ExampleWithBase

snippet: ExampleWithBaseGeneratedCode

## Embedding the attributes in your project

By default, the `[AutoConstruct]` attributes referenced in your project are contained in an external dll. It is also possible to embed the attributes directly in your project. To do this, you must do two things:

1. Define the MSBuild constant `AUTOCTOR_EMBED_ATTRIBUTES`. This ensures the attributes are embedded in your project.
2. Add `compile` to the list of excluded assets in your `<PackageReference>` element. This ensures the attributes in your project are referenced, insted of the _AutoCtor.Attributes.dll_ library.

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
