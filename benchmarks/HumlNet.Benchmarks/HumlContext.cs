using Huml.Net.Serialization;
using Huml.Net.Serialization.Attributes;

namespace HumlNet.Benchmarks;

// Huml.Net source-generation context (the reflection-free Huml.Net path).
[HumlSerializable(typeof(ServiceConfig))]
[HumlSerializable(typeof(ServerBlock))]
[HumlSerializable(typeof(DatabaseBlock))]
public partial class HumlContext : HumlGeneratedContext;
