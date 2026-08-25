# Internationalization & Culture Invariance (i18n)

---

## 1. Domain Primitive Invariance

All entity identifiers, temporal attributes, and numeric values in `EricksonLopez.SharedKernel` enforce `CultureInfo.InvariantCulture` during string conversions (`ToString`, `TryParse`) to ensure cross-platform consistency.
