---
name: ship
description: Commits, pushes and raises a PR.
disable-model-invocation: true
---


# Commit, Push and Raise PR

This skill handles the complete workflow of committing staged/unstaged changes, pushing to the remote, and raising a Pull Request against the `main` branch.

## Workflow

1. **Analyse Changes** - Review all pending changes (staged and unstaged) to understand the scope and nature of the work
2. **Commit** - Create a well-formed conventional commit
3. **Push** - Push the current feature branch to origin
4. **Raise PR** - Create a Pull Request targeting `main`

## Commit Message Format

Use Conventional Commits format:
```
<type>: <description>

[optional body with more detail]
```

### Types
- `feat:` - new functionality
- `fix:` - bug fixes
- `refactor:` - code restructuring without behaviour change
- `docs:` - documentation only
- `test:` - adding or updating tests
- `chore:` - maintenance tasks, dependency updates

### Rules
- First line must be **50 characters or fewer** to avoid GitHub truncation
- Description should be lowercase, imperative mood ("add feature" not "added feature")
- Body (if needed) should be wrapped at 72 characters

## Pull Request Format

The PR title should match the first line of the commit message (including the conventional commit prefix).

Use this template for the PR body:
```markdown
## Summary
[2-3 sentences describing what this PR accomplishes and why]

## Changes
[Bullet points of the key modifications, grouped logically]

## Planka Card(s)
- [ ] [Replace with card title and link]

## Test Plan
- [ ] [Specific scenario to verify]
- [ ] [Another test case]
- [ ] [Edge case to check]

## Notes for Reviewers
[Any context that will help reviewers: areas of uncertainty, alternative approaches considered, or specific files to scrutinise]
```

## Execution Steps

1. Run `git status` and `git diff` to understand all pending changes
2. Analyse the changes to determine the appropriate commit type and craft a meaningful message
3. Stage all changes with `git add -A`
4. Commit with the crafted message
5. Push the branch with `git push -u origin HEAD`
6. Create the PR using `gh pr create --base develop --title "<commit first line>" --body "<PR body>"`

## Important Notes

- Derive the commit message and PR content entirely from analysing the actual changes - do not ask the user to describe them
- The test plan should contain **specific, actionable** test cases derived from the changes, not generic placeholders
- If changes span multiple concerns and would benefit from separate commits, note this to the user but proceed with a single commit unless instructed otherwise
