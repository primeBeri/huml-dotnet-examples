# Benchmark results — Huml.Net vs System.Text.Json

Recorded **2026-07-08**, `Huml.Net 0.2.0-rc.1`, BenchmarkDotNet v0.14.0, .NET 10.0.9, Windows 11 x64.
Reproduce with `dotnet run -c Release --project benchmarks/HumlNet.Benchmarks`.

The payload is an equivalent nested service-config object (see `Model.cs`), encoded as HUML for
Huml.Net and as JSON for System.Text.Json — so this measures **HUML-vs-JSON throughput for the
same data**, in both reflection and source-generated modes. `Stj_*_Reflection` is the baseline
(ratio 1.00) in each category.

## Serialize

| Method                     | Mean      | Ratio | Allocated | Alloc Ratio |
|--------------------------- |----------:|------:|----------:|------------:|
| Stj_Serialize_SourceGen    |   241.3 ns|  0.68 |     592 B |        0.65 |
| Stj_Serialize_Reflection   |   356.1 ns|  1.00 |     904 B |        1.00 |
| Huml_Serialize_SourceGen   |   530.0 ns|  1.49 |    1624 B |        1.80 |
| Huml_Serialize_Reflection  |   865.4 ns|  2.43 |    1768 B |        1.96 |

## Deserialize

| Method                      | Mean      | Ratio | Allocated | Alloc Ratio |
|---------------------------- |----------:|------:|----------:|------------:|
| Stj_Deserialize_Reflection  |   659.6 ns|  1.00 |    1416 B |        1.00 |
| Stj_Deserialize_SourceGen   |   666.9 ns|  1.01 |    1416 B |        1.00 |
| Huml_Deserialize_SourceGen  | 1,976.5 ns|  3.00 |    6176 B |        4.36 |
| Huml_Deserialize_Reflection | 2,356.8 ns|  3.57 |    6600 B |        4.66 |

## Parse-only

| Method      | Mean      | Ratio | Allocated | Alloc Ratio |
|------------ |----------:|------:|----------:|------------:|
| Stj_Parse   |   424.2 ns|  1.00 |      96 B |        1.00 |
| Huml_Parse  | 1,202.6 ns|  2.83 |    4848 B |       50.50 |

The `0.2.0-rc.1` figures are within run-to-run noise of the `0.2.0-beta.1` recording
(2026-06-11) — the rc's overridden-property serialisation fix carries no throughput cost for
ordinary models.

## Honest commentary

- **System.Text.Json is faster, and that's expected.** STJ is a hyper-optimised, years-mature,
  UTF-8-native serialiser that ships with the runtime. Huml.Net is a young, UTF-16/string-based
  library optimised for correctness and readability first. Being within **~2.4×** on serialise
  and **~3–3.6×** on deserialise at the first release candidate is a respectable starting point,
  not a problem to hide.
- **The source generator earns its keep.** It cuts serialise time by ~39% (865 → 530 ns) and
  deserialise by ~16% (2357 → 1977 ns) versus reflection, and trims allocations — the reflection-
  free path is the one to reach for in hot loops or AOT apps. (STJ's own source-gen shows the same
  pattern on serialise; on deserialise STJ is already so fast that its generator barely moves.)
- **The parse-only row is not apples-to-apples.** `JsonDocument.Parse` is lazy — it scans the
  bytes and defers materialisation, allocating almost nothing (96 B). `Huml.Parse` eagerly builds
  a full immutable `HumlDocument` AST (nodes, strings, positions), which is why it allocates far
  more. Compare it to "JSON DOM construction", not to "JSON scanning". A future lazy/streaming
  reader is explicitly out of scope for the 0.2.0 line.
- **Why use HUML at all, then?** Throughput is rarely the deciding factor for configuration and
  document formats — these payloads are parsed once at startup, not in a request hot path. HUML's
  value is **human readability and strictness** (no YAML footguns), with a `System.Text.Json`-style
  API that .NET developers already know. When you *do* need speed, the source-generated path is
  there.

### Where the gap can close (post-1.0 groundwork)

The largest single lever is the eager AST + UTF-16 string allocation on the read path (the 4.7×
deserialise allocation ratio). An `IBufferWriter<char>` serialise overload (backlog 999.45) and a
lazy reader would both narrow the margin — both are tracked as post-rc work.
