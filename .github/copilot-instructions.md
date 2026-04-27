# Project Workflow Instructions

## Core Development Process
- We follow a **Specification-Driven, Phase-based** approach.
- Development is organized in clear **phases**.
- Specs and Architecture documents are the single source of truth.
- Tasks are derived directly from these documents and grouped by phase.

## Key Documents
- Always reference: `docs/SPEC.md` and `docs/ARCHITECTURE.md`.
- When starting any task or phase, first read the relevant sections from SPEC.md and ARCHITECTURE.md.

## Phase & Task Structure
- Tasks are stored in `docs/tasks/` folder.
- Each phase has its own dedicated Markdown file (e.g. `phase-X-*.md`, etc.).
- Phases are executed sequentially unless explicitly stated otherwise.

## Task Creation (Analytics Agent)
- Break down features/requirements from SPEC.md into small, atomic tasks **within the correct phase**.
- Each task should include: goal, acceptance criteria, relevant spec sections, and references to architecture decisions.
- Add new tasks inside the appropriate phase file in `docs/tasks/`.
- Keep phases clean and well-organized.

## Implementation (Developer Agent)
- Always implement tasks from the current phase.
- Follow the phase order defined in SPEC.md.
- Implement **exactly** what is defined in the current task + referenced specs/architecture.
- Do not add extra features or jump to future phases unless explicitly asked.
- After finishing tasks in a phase, suggest updates to specs/docs if needed.

## General Rules
- Prefer reading existing documents over assuming.
- Ask for clarification only if the spec is genuinely ambiguous.
- Maintain clean code, strong typing, and the architectural patterns defined in ARCHITECTURE.md.