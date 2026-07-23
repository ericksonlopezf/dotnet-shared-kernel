using BenchmarkDotNet.Running;

// Discover and run all benchmark classes in this assembly.
// Usage:
//   dotnet run -c Release                    → interactive menu
//   dotnet run -c Release -- --filter *      → run all
//   dotnet run -c Release -- --filter *Result*  → filter by name
BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args);
