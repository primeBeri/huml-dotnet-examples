# Huml.Net — Examples & Benchmarks

Worked examples, end-to-end tests, and performance benchmarks for
[**Huml.Net**](https://github.com/primeBeri/huml-dotnet) — the .NET implementation of
[HUML](https://huml.io).

This is a companion repo. Each example is a small, runnable console app that demonstrates one
feature **and** asserts its own behaviour, so the set doubles as an end-to-end test suite run
against the *published NuGet package* (not a project reference) — which also validates the
package layout itself.

## Layout

```
src/examples/       one console project per feature area (each asserts → exits non-zero on failure)
src/Examples.Shared/  the Check assertion helper
datasets/           shared HUML + JSON payload pairs (small / medium / large / deep)
benchmarks/         BenchmarkDotNet suite: Huml.Net vs System.Text.Json
run-examples.ps1    runs every example and aggregates results (used by CI)
```

## Run the examples

```bash
./run-examples.ps1
```

Or one at a time:

```bash
dotnet run --project src/examples/E01.GettingStarted -c Release
```

| Example | Feature |
|---------|---------|
| E01 GettingStarted | Deserialise, serialise, round-trip |
| E02 NamingPolicies | Kebab-case keys, `[HumlProperty]` overrides |
| E03 Enums | `[HumlEnumValue]` custom wire names |
| E04 Polymorphism | `[HumlPolymorphic]` incl. nested/collection elements |
| E05 CustomConverters | A `HumlConverter<T>` for a value type |
| E06 Populate | Overlay a partial document onto an instance |
| E07 ErrorHandling | The exception contract; `HumlOptions.Strict` |
| E08 SourceGeneration | Reflection-free metadata via `HumlGeneratedContext` |

## Package version under test

Examples reference `Huml.Net` via `PackageReference` at `$(HumlNetVersion)` (default
`0.2.0-beta.1`). During development — before the beta is published to nuget.org — the package is
restored from `local-feed/`, which the [main repo](https://github.com/primeBeri/huml-dotnet)
populates with `dotnet pack`. Once the beta ships, `local-feed/` is dropped and restore comes
from nuget.org. Override the version with:

```bash
./run-examples.ps1 -HumlNetVersion 0.2.0-beta.1
```

## Benchmarks

```bash
dotnet run --project benchmarks/HumlNet.Benchmarks -c Release
```

See [benchmarks/RESULTS.md](benchmarks/RESULTS.md) for a recorded run and commentary.

## Licence

MIT — see [LICENSE](LICENSE).
