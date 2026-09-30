; Unshipped analyzer release
; https://github.com/dotnet/roslyn/blob/main/src/RoslynAnalyzers/Microsoft.CodeAnalysis.Analyzers/ReleaseTrackingAnalyzers.Help.md

### New Rules

Rule ID | Category | Severity | Notes
--------|----------|----------|-------
ACTR010 | AutoCtor | Error | ServiceImplementationMustHaveSinglePublicConstructor
ACTR011 | AutoCtor | Error | ServiceImplementationCannotBeInstantiated
ACTR012 | AutoCtor | Error | ServiceDependencyNotRegistered
ACTR013 | AutoCtor | Error | CircularServiceDependency
ACTR014 | AutoCtor | Error | KeyedServiceDependencyNotRegistered
ACTR015 | AutoCtor | Error | ImplementationNotAssignableToService
ACTR016 | AutoCtor | Error | InvalidOpenGenericRegistration
ACTR017 | AutoCtor | Error | InvalidServiceProviderType
ACTR018 | AutoCtor | Warning | TransientServiceCaptured
ACTR019 | AutoCtor | Error | ScopedServiceCapturedBySingleton
ACTR020 | AutoCtor | Error | OpenGenericConstraintNotSatisfied
ACTR021 | AutoCtor | Error | InvalidServiceFactory
ACTR022 | AutoCtor | Error | ServiceFactoryReturnTypeNotAssignable
ACTR023 | AutoCtor | Error | OpenGenericServiceFactory
ACTR024 | AutoCtor | Error | InvalidProviderFallback
ACTR025 | AutoCtor | Error | ProviderFallbackNotAServiceProvider
ACTR026 | AutoCtor | Warning | ScanFoundNoTypes
ACTR027 | AutoCtor | Warning | ImportedModuleHasNoRegistrations
ACTR028 | AutoCtor | Error | ModuleFactoryMustBeStatic
ACTR029 | AutoCtor | Error | ServiceProviderMustBeSealed
ACTR030 | AutoCtor | Error | ScanHasNoFilter
