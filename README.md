# IsolatedTestFixtureAttribute

The goal of this work is to use existing `NUnit` extensibility mechanisms to achieve `AssemblyLoadContext`-level isolation between unit-test runs.

Typical use case is testing a code which mutates static state, or have complex logic in type initializers.

[`AssemblyLoadContext`](https://docs.microsoft.com/en-us/dotnet/api/system.runtime.loader.assemblyloadcontext) is a new scoping concept in `.NET Core`, which is intended to replace [`AppDomain`](https://docs.microsoft.com/en-us/dotnet/api/system.appdomain)s in many cases.

# Current issues
 * Test method isolation is not achieved (only test classes can be isolated). `IsolatedTests3.TestSharedVariableIncrement` tests are failing, because they're using the same `AssemblyLoadContext`.
 * `AssemblyLoadContext` unloadability. Currently, all created contexts are not unloaded. [DotNet Core Unloadability Design Docs](https://github.com/dotnet/coreclr/blob/master/Documentation/design-docs/unloadability.md).
 * The design is poor. Better design can be supported by NUnit maintainers, see [the issue](https://github.com/nunit/nunit/issues/3444).
 
