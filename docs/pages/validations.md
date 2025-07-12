# Validations

Flecto provides a built-in validation mechanism to verify that filters are logically 
consistent before executing a query. Depending on the use case, it can either throw 
exceptions immediately or return structured errors to support user-facing validation. 
This adds a layer of safety and prevents malformed or 
logically incorrect inputs from being processed.

### Key principles:

- Filters are only validated if provided (i.e., not null), 
    but when present, they are always subjected to built-in logical checks.
- These checks are executed during query construction and use `Ensure`-style 
    strict validation by default (throwing exceptions on invalid input).
- Validation logic uses the same methods for both internal and external validation: 
    internally, it throws exceptions on invalid input (e.g., via `Ensure`), 
    while externally, it can return structured errors to support user-facing 
    validation. Developers can also inject additional custom validation logic 
    (e.g., business rules) into the same mechanism.
- You can also integrate Flecto filters into external validators like **FluentValidation**.

### Built-in behavior

Each filter in Flecto has a corresponding validator that performs 
**basic logical consistency checks** to ensure correct usage and guard against 
invalid combinations of values.

Below is a summary of built-in validation logic per filter type:

#### `BoolFilter`

- Disallows using `Eq` and `NotEq` simultaneously.
- If `RequireAtLeastOne` is set, requires at least one of `Eq`, `NotEq`, or `Null`.
- Invokes `customValidator` if provided and includes its result.

#### `DateFilter`

- Disallows using `Eq` and `NotEq` simultaneously.
- Ensures range boundaries `Gt`, `Gte`, `Lt`, and `Lte` are logically consistent.
- Validates that `In` and `NotIn` arrays are not empty if provided.
- Invokes `customValidator` if provided.

#### `EnumFilter<T>`

- Disallows using `Eq` and `NotEq` simultaneously.
- Validates that `In` and `NotIn` arrays are not empty if provided.
- Invokes `customValidator` if provided.

#### `FlagsEnumFilter<T>`

- Disallows using `Eq` and `NotEq` simultaneously.
- Disallows setting both `HasFlag` and `NotHasFlag` at the same time.
- Invokes `customValidator` if provided.

#### `GuidFilter`

- Disallows using `Eq` and `NotEq` simultaneously.
- Validates that `In` and `NotIn` arrays are not empty if provided.
- Invokes `customValidator` if provided.

#### `NumericFilter<T>`

- Disallows using `Eq` and `NotEq` simultaneously.
- Ensures logical consistency of range fields: `Gt`, `Gte`, `Lt`, and `Lte`.
- Validates that `In` and `NotIn` arrays are not empty if provided.
- Invokes `customValidator` if provided.

#### `PaginationFilter`

- Validates that `Limit` and `Page` are greater than zero.
- If `maxLimit` is specified, ensures `Limit` does not exceed it.
- Returns an error if the filter itself is null.

#### `SearchFilter`

- Requires `Value` to be non-empty and not just whitespace.
- Validates that `Value` meets optional `minLength` and `maxLength` constraints if provided.

#### `StringFilter`

- Validates individual string fields (`Eq`, `NotEq`, `Contains`, `StartsWith`, `EndsWith`) for emptiness, length, and optional `customValidator`.
- Validates string arrays (`In`, `NotIn`) for emptiness, length, and optional `customArrayValidator`.
- Honors `allowEmptyStrings` to control whether empty strings are permitted.
- Applies `maxLength` if provided to restrict string length.

### Validation API

Flecto provides per-filter validation methods that return structured errors 
in the form of `(Field, Error)` tuples. These are designed for both internal use 
(`Ensure(...)` for exceptions) and user-facing validation (`Validate(...)` 
for result collection).

Each filter type has its own `Validate(...)` method with a signature tailored to its structure. 
All validators:

- Apply **built-in logical checks** (e.g., Eq + NotEq conflict, range consistency, non-empty arrays).
- Optionally use **custom validation logic** via `Func<TFilter, (bool IsValid, string? Error)>`.
- Return a consistent output: `IEnumerable<(string Field, string Error)>`.

#### Example: FlagsEnumFilter

```csharp
public static IEnumerable<(string Field, string Error)> Validate(
    FlagsEnumFilter<UserRole>? filter,
    Func<FlagsEnumFilter<UserRole>, (bool IsValid, string? ErrorMessage)>? customValidator = null)
```

Other filters (e.g. `SearchFilter`, `StringFilter`) may also accept `maxLength`, `minLength`, 
`allowEmptyStrings`, or custom array validators as additional parameters.

### Validation Modes

There are two usage modes for Flecto's validation system:

#### 1. **Internal (Ensure) — always enforced, throws on error**

Internal validation logic (`EnsureValid(...)`) runs automatically during query building 
and throws immediately on validation failure.

This is useful for internal tools, CLI utilities, or safe programmatic usage or if developer 
do not use validation for requests.

#### 2. **User-facing (Validate) — collects errors for feedback**

You can manually invoke `Validate(...)` to get a list of validation results and display them to users:

```csharp
var errors = FlagsEnumFilterValidator<UserRole>.Validate(filter);
if (errors.Any()) {
    return BadRequest(errors);
}
```

This mode allows API endpoints and UIs to return understandable validation errors instead of throwing.

---

### Example: Add custom rule

```csharp
var errors = FlagsEnumFilterValidator<UserRole>.Validate(
    filter,
    customValidator: f => (!f.Eq.HasValue || f.Eq.Value.HasFlag(UserRole.Admin), "Only Admins are allowed"));
```

---

### Example: Use with FluentValidation

```csharp
RuleFor(x => x.Role)
    .Must(f => !FlagsEnumFilterValidator<UserRole>.Validate(f).Any())
    .WithMessage("Invalid Role filter");
```

### Why it matters

Validation gives API developers and consumers predictable behavior, safer filtering, 
and **early failure** before querying the database.
