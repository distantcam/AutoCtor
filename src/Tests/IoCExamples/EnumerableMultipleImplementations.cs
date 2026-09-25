using AutoCtor;
using System.Collections.Generic;

// Registering a service more than once keeps every registration. The collection sees them
// in registration order; asking for IValidator on its own gets the last.
[ServiceProvider]
[Singleton<IValidator, NameValidator>]
[Singleton<IValidator, EmailValidator>]
[Singleton<IValidator, AgeValidator>]
[Singleton<IValidationRunner, ValidationRunner>]
public partial class ValidationProvider;

public interface IValidator;
public class NameValidator : IValidator;
public class EmailValidator : IValidator;
public class AgeValidator : IValidator;

public interface IValidationRunner;

public class ValidationRunner : IValidationRunner
{
    public ValidationRunner(IEnumerable<IValidator> validators) { }
}
