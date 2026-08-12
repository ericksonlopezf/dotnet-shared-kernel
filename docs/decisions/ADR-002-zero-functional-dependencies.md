# ADR-002: Zero Functional Dependencies Exception for netstandard2.0

## Status
**Superseded** by ADR-009 (Target Framework Strategy).

## Context
Originally, the library targeted both `net10.0` and `netstandard2.0`, which required build-time polyfills like `PolySharp`. 

## Decision
As per ADR-009, this library now targets `net10.0` exclusively. The exceptions for `netstandard2.0` polyfills are no longer applicable. The strict "Zero Functional Dependencies" rule remains in effect, with `EricksonLopez.Result` being the only allowed dependency.
