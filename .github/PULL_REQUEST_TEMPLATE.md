## 📋 Description
<!-- Describe your changes in detail -->

## 🔗 Related Issues
<!-- Reference any related issue(s): Fixes #123 -->

## 🎯 Type of Change
- [ ] 🐛 Bug fix (non-breaking change fixing an issue)
- [ ] ✨ New feature (non-breaking change adding functionality)
- [ ] 💥 Breaking change (fix or feature causing existing functionality to change)
- [ ] ⚡ Performance improvement
- [ ] 📖 Documentation update
- [ ] 🧪 Tests enhancement
- [ ] 🔧 Maintenance / CI/CD

## 🛡️ Architectural & Quality Checklist
- [ ] Code strictly follows Clean Architecture, DDD, and Repository Invariants
- [ ] Code is 100% NativeAOT & Trimming compliant (`EnableTrimAnalyzer=true`)
- [ ] Zero unnecessary heap allocations on hot execution paths
- [ ] All public APIs have XML documentation comments (CS1591 enforced)
- [ ] Unit tests added/updated and passing locally (`dotnet test -c Release`)
- [ ] Stryker mutation score verified ($\ge 95\%$ break threshold)
- [ ] Adheres to Conventional Commits format (`feat:`, `fix:`, `docs:`, `perf:`, etc.)
- [ ] 100% of code, comments, and documentation are in English
