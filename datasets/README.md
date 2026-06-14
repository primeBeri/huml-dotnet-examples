# datasets

Shared HUML + JSON payload pairs used by the benchmark suite and available as reference inputs
for integration tests, fuzz seeds, and performance profiling.

Each pair encodes the same data in both HUML and JSON so throughput comparisons are
apples-to-apples.

| Pair | Description |
|------|-------------|
| `small-config.*` | Minimal flat key/value config (6 HUML lines). |
| `medium-document.*` | Nested service config with server and database blocks (20 HUML lines). Used by the BenchmarkDotNet suite. |
| `large-collection.*` | Large repeated-item collection (500+ HUML lines). Exercises allocation-heavy paths. |
| `deep-nesting.*` | Deeply nested structure (6 levels). Tests recursion-depth behaviour near the `HumlOptions.MaxRecursionDepth` limit. |

## Format

Each `.huml` file is a valid HUML v0.2 document with a `%HUML v0.2.0` header.
The corresponding `.json` file encodes the same structure for use with `System.Text.Json`
benchmarks or for reference when authoring new test fixtures.

## Usage

The benchmark suite in `benchmarks/HumlNet.Benchmarks/` uses the medium-document pair via an
embedded model (`Model.cs`) rather than reading these files at runtime — keeping benchmark
startup deterministic and allocation-free. These files serve as the canonical reference source
for that model and as ready-made seeds for fuzz testing or property-based round-trip tests.
