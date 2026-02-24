## ADDED Requirements

### Requirement: Stage Document Pack Exists
The project MUST maintain a stage-based documentation pack (Discovery → Ops) that can be used to align planning, execution, and acceptance before coding.

#### Scenario: Start a new iteration
- **WHEN** the team starts a new iteration for Arcadia
- **THEN** they MUST review and update the stage document pack
- **AND THEN** they MUST keep links to the current truth specs and runnable verification scripts

### Requirement: Docs Pack Does Not Change Runtime Behavior
The stage document pack MUST NOT introduce runtime behavior changes by itself.

#### Scenario: Documentation-only change
- **WHEN** a change only modifies the stage document pack
- **THEN** it MUST be safe to apply without deploying code
- **AND THEN** it MUST record a checkpoint entry for traceability

