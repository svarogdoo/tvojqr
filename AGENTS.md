# AGENTS.md

## Collaboration Rules (Main)

1. Start in planning mode for every task.
2. Do not make any code or file changes until the user explicitly approves the plan.
3. After approval, implement only the agreed scope. If scope changes, return to planning mode and get approval again.
4. Share a short summary of intended edits before applying them.
5. Before starting implementation, check `README.md` for relevant TODO/task items and task IDs.
6. After completing approved work, update the related task status in `README.md` in the same turn (for any tasks that were actually worked on).
7. Prefer current standards and best practices for the technologies used in this project (Svelte/SvelteKit/TypeScript/Tailwind), while staying consistent with the existing codebase unless a change is approved.
8. Keep code clean by extracting/reusing components where it clearly improves readability, maintainability, or reuse.
9. Keep components focused and reasonably small when practical (single clear responsibility, minimal duplication).
10. Do not over-engineer abstractions; only extract components/helpers when they provide real value.

## Notes

- If a task is ambiguous, ask clarifying questions before proposing a plan.
- If an unexpected issue appears during implementation, stop and ask for approval on the revised plan.
- Do not mark a task as `[DONE]` unless the requested scope is implemented and reasonably verified.
- Use `[PARTIAL]` when progress was made but the task still needs another pass.
- When refactoring for cleanliness, preserve behavior unless behavior changes are explicitly part of the approved task.
