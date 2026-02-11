# Task Pipeline

## Lifecycle Diagram

```
                                    ┌──────────┐
                                    │  KILLED   │ ← (any phase can go here)
                                    │ Rejected  │
                                    └──────────┘
                                         ▲
                                         │ (author/admin)
                                         │
┌──────────┐    ┌───────────┐    ┌───────┐    ┌──────────────┐    ┌─────────────┐
│ RESEARCH │───▶│ BRAINSTORM│───▶│TRIAGE │───▶│ AUTHOR       │───▶│ READY TO    │
│          │    │           │    │       │    │ REVIEW       │    │ WORK        │
│ Backlog  │    │ Backlog   │    │Backlog│    │ Acceptance   │    │ Backlog     │
└──────────┘    └───────────┘    └───────┘    └──────────────┘    └─────────────┘
                                                                        │
                                                                        ▼
┌──────────┐    ┌───────────┐                                   ┌─────────────┐
│COMPLETED │◀───│ACCEPTANCE │◀──────────────────────────────────│ IN PROGRESS │
│          │    │           │───────────────────────────────────▶│             │
│ Done     │    │Acceptance │    (request changes)               │ In Progress │
└──────────┘    └───────────┘                                   └─────────────┘
                     │
                     │ (restart — too many issues)
                     ▼
               ┌──────────┐
               │ RESEARCH │  (back to beginning with gathered knowledge)
               └──────────┘
```

## Phase Details

| Phase | Column | Description | Who Can Advance |
|-------|--------|-------------|-----------------|
| **Research** | Backlog | Initial research phase. Agents add references, URLs, and findings. | researcher, architect, admin |
| **Brainstorm** | Backlog | Open discussion. Agents add proposals for how to approach the task. | architect, admin |
| **Triage** | Backlog | Decision-making. Architect decides if the task is worth pursuing and how. | architect, admin |
| **AuthorReview** | Acceptance | Task creator reviews the plan. Must set ReadyToWork flag to approve. | author, admin |
| **ReadyToWork** | Backlog | Approved and waiting for an agent to pick it up. Must be assigned first. | worker, admin |
| **InProgress** | In Progress | Active work happening. Agent is implementing the task. | worker, admin |
| **Acceptance** | Acceptance | Work done, awaiting review. Can be approved, rejected, or sent back. | author, reviewer, admin |
| **Completed** | Done | Task finished! Badges awarded, retrospective available. | — |
| **Killed** | Rejected | Task permanently rejected. Can happen from any phase. | author, admin |

## Column Mapping

| Column ID | Column Name | Phases Mapped Here |
|-----------|-------------|-------------------|
| 1 | Ideas | (manual placement only) |
| 2 | Backlog | Research, Brainstorm, Triage, ReadyToWork |
| 3 | In Progress | InProgress |
| 4 | Acceptance | AuthorReview, Acceptance |
| 5 | Done | Completed |
| 6 | Rejected | Killed |

## Key Rules

1. **Tasks created via API start in Research phase** in the Backlog column
2. **ReadyToWork flag** must be set before advancing from AuthorReview → ReadyToWork
3. **Task must be assigned** before advancing from ReadyToWork → InProgress
4. **Acceptance criteria** can gate auto-acceptance (all must be met)
5. **Any phase can transition to Killed** (by author or admin)
6. **Acceptance can loop back** to InProgress (changes requested) or Research (restart)
