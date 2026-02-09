# Architecture Decision Records

Last Updated: 2026-02-09

## What are ADRs?

Architecture Decision Records (ADRs) document significant architectural decisions made in the project. They capture the context, decision, and consequences of each choice.

## Why ADRs?

- **Historical context:** Understand why decisions were made
- **Onboarding:** Help new team members understand the architecture
- **Avoiding regression:** Prevent revisiting settled decisions
- **Accountability:** Document trade-offs and rationale

## ADR Index

| ADR | Title | Status | Date |
|-----|-------|--------|------|
| [001](001-pipeline-mode-strategy.md) | PipelineMode Strategy | Accepted | 2026-02-09 |
| [002](002-saga-correlation-keys.md) | Saga Correlation Keys | Accepted | 2026-02-09 |
| [003](003-date-range-cascade-limits.md) | Date-Range Cascade Limits | Proposed | 2026-02-09 |

## Creating a New ADR

1. Copy `template.md` to a new file: `NNN-title-in-kebab-case.md`
2. Number sequentially (001, 002, 003, ...)
3. Fill in all sections
4. Update this README with the new ADR
5. Commit with message: `docs: add ADR-NNN <title>`

## ADR Statuses

- **Proposed:** Under discussion, not yet implemented
- **Accepted:** Decision made and implemented
- **Deprecated:** No longer recommended, but still in use
- **Superseded:** Replaced by a newer ADR (link to replacement)

## References

- [ADR GitHub Organization](https://adr.github.io/)
- [Joel Parker Henderson's ADR templates](https://github.com/joelparkerhenderson/architecture-decision-record)
