---
applyTo: "src/DocFlow.Domain/**,tests/DocFlow.Domain.Tests/**"
---
# DocFlow.Domain rules
- Domain layer is pure C#: no ProjectReference and no PackageReference in DocFlow.Domain.csproj. No EF Core, ASP.NET, MediatR, FluentValidation, Stateless or any other NuGet package.
- Never edit any .csproj or Directory.Build.props (another task is changing them in parallel).
- No I/O, no DateTime.Now/UtcNow, no static mutable state. Time is passed in as a parameter.
- Aggregates: private setters, private ctor + static factory, collections exposed as IReadOnlyList, invariants checked BEFORE mutating state.
- Business-rule violations throw DomainException or a subclass. Never return bool/error codes for rule violations.
- Keep it simple. No State-pattern frameworks, no generic base classes, no speculative features.
- Tests: plain xUnit Assert only (no FluentAssertions, no Moq). Naming: Method_Scenario_ExpectedResult. Never weaken or delete a test to make it pass; report it instead.
- Before finishing any task: build, run tests, list changed files and the assumptions you made.
- If the spec is ambiguous, ask instead of inventing.