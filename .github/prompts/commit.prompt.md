---
agent: agent
description: Generate a git commit message from staged changes and commit
---

Review the staged diff and generate a git commit message following these rules.

## Header

```
<type>(<scope>): <short imperative description, ≤72 chars>
```

**type** — choose one:
| type | when to use |
|------|-------------|
| `feat` | new feature or behaviour |
| `fix` | bug fix |
| `refactor` | code change that is not a feat or fix |
| `test` | adding or correcting tests |
| `build` | changes to build system, dependencies, CI |
| `docs` | documentation only |
| `chore` | housekeeping (renaming, moving files, config) |

**scope** — the primary assembly or area affected (e.g. `api`, `application`, `domain`, `infrastructure`, `tests`, `client`, `ci`). Use the most specific scope that covers the change. Omit if the change spans too many areas to be meaningful.

## Body — organised by assembly area

Separate the header from the body with a blank line. Group bullet points under bold assembly headings. Only include headings that have changes. Use imperative mood. Each bullet should describe *what* changed and *why* if non-obvious.

```
**Domain**
- <change>

**Application**
- <change>

**Infrastructure**
- <change>

**Api**
- <change>

**Tests**
- <change>

**Build / CI**
- <change>

**Client**
- <change>
```

## Rules

- Do not include a footer unless there is a breaking change (`BREAKING CHANGE:`) or a linked issue (`Closes #n`).
- Never wrap lines in the body at fewer than 72 characters.
- Do not prefix bullets with the assembly name — they are already grouped under headings.
- If the diff is a single-assembly, single-concern change the body may be omitted and the header alone is sufficient.

## Steps

1. Run `git diff --staged --name-only` to check for staged changes.
   - If there are **no staged changes**, run `git status --short` to show the user what is unstaged, then ask the user which files to stage (specific paths, or all with `-A`), and run `git add <paths>` accordingly.
2. Run `git diff --staged` to retrieve the full staged diff.
3. Analyse the diff and compose the commit message following the rules above.
4. Present the commit message to the user and ask: **"Do you want to commit with this message? (yes / no / edit)"**
   - **yes** — proceed to step 5.
   - **no** — abort; do not run `git commit`.
   - **edit** — ask the user what to change, revise the message, then repeat step 4.
5. Once the user confirms with **yes**, run `git commit -m "<header>" -m "<body>"` to submit the commit.
