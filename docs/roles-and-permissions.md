# Roles & Permissions

## Roles

| Role | Description |
|------|-------------|
| **admin** | Full access. Can manage agents, users, delete tasks, and override any permission. |
| **worker** | Can take tasks, do work (InProgress), and submit for acceptance. |
| **researcher** | Can add research references and advance from Research to Brainstorm. |
| **architect** | Can triage, propose solutions, and advance through planning phases. |
| **reviewer** | Can approve/reject tasks in the Acceptance phase. |
| **author** | Pseudo-role — the agent who created the task. Has special permissions on their own tasks. |

Agents can have **multiple roles** (comma-separated, e.g., `"admin,worker,researcher"`).

## Permission Matrix

| Action | admin | worker | researcher | architect | reviewer | author |
|--------|:-----:|:------:|:----------:|:---------:|:--------:|:------:|
| Create task | ✅ | ✅ | ✅ | ✅ | ✅ | — |
| Delete task | ✅ | ❌ | ❌ | ❌ | ❌ | ❌ |
| Add research | ✅ | ❌ | ✅ | ✅ | ❌ | — |
| Add proposal | ✅ | ✅ | ✅ | ✅ | ❌ | — |
| Add comment | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ |
| Research → Brainstorm | ✅ | ❌ | ✅ | ✅ | ❌ | ❌ |
| Brainstorm → Triage | ✅ | ❌ | ❌ | ✅ | ❌ | ❌ |
| Triage → AuthorReview | ✅ | ❌ | ❌ | ✅ | ❌ | ❌ |
| AuthorReview → ReadyToWork | ✅ | ❌ | ❌ | ❌ | ❌ | ✅ |
| ReadyToWork → InProgress | ✅ | ✅ | ❌ | ❌ | ❌ | ❌ |
| InProgress → Acceptance | ✅ | ✅ | ❌ | ❌ | ❌ | ❌ |
| Acceptance → Completed | ✅ | ❌ | ❌ | ❌ | ✅ | ✅ |
| Acceptance → InProgress | ✅ | ❌ | ❌ | ❌ | ✅ | ✅ |
| Any → Killed | ✅ | ❌ | ❌ | ❌ | ❌ | ✅ |
| Manage agents | ✅ | ❌ | ❌ | ❌ | ❌ | ❌ |
