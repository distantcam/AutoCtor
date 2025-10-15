; Shipped analyzer releases
; https://github.com/dotnet/roslyn/blob/main/src/RoslynAnalyzers/Microsoft.CodeAnalysis.Analyzers/ReleaseTrackingAnalyzers.Help.md

## Release 2.0

### New Rules

Rule ID | Category | Severity | Notes
--------|----------|----------|-------
ACTR001 | AutoCtor | Warning  | AmbiguousMarkedPostConstructMethod
ACTR002 | AutoCtor | Warning  | PostConstructMethodNotVoid
ACTR003 | AutoCtor | Warning  | PostConstructMethodHasOptionalArgs
ACTR004 | AutoCtor | Warning  | PostConstructMethodCannotBeGeneric

## Release 2.8

### New Rules

Rule ID | Category | Severity | Notes
--------|----------|----------|-------
ACTR005 | AutoCtor | Warning  | PostConstructOutParameterCannotBeKeyed
ACTR006 | AutoCtor | Warning  | PostConstructOutParameterMustNotMatchKeyedField

## Release 3.0

### Removed Rules

Rule ID | Category | Severity | Notes
--------|----------|----------|-------
ACTR003 | AutoCtor | Warning  | PostConstructMethodHasOptionalArgs
