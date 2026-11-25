# Issue tracker: GitHub

Issues and specs live in AnthonyPitman/coin-collection on GitHub.
Use the gh CLI from this repo.

## Operations

- Read: gh issue view <number> --json number,title,body,labels,comments
- List: gh issue list --state open
- Create: gh issue create --title "..." --body-file <file>
- Edit: gh issue edit <number>
- Comment: gh issue comment <number> --body-file <file>
- Label: gh issue edit <number> --add-label "..."
- Remove label: gh issue edit <number> --remove-label "..."
- Close: post the explanation first, then gh issue close <number>.

For multiline content, write the text to a temporary file and use
--body-file.

Before the first issue creation in a session, verify the target with
gh repo view --json nameWithOwner. It must be AnthonyPitman/coin-collection.

Use native GitHub sub-issues and issue dependencies when available.
Otherwise, put "Part of #<parent>" and "Blocked by: #<number>" at the
top of the child issue body. A ticket is startable when every blocker
is closed.

## Pull requests as a triage surface

PRs as a request surface: no.

## Skill instructions

"Publish to the issue tracker" means create a GitHub issue.
"Fetch the relevant ticket" means read the GitHub issue.

## Wayfinding

Use one issue labelled wayfinder:map as the map.
Link child tickets as sub-issues, or list them in the map body.
Label children wayfinder:research, wayfinder:prototype,
wayfinder:grilling, or wayfinder:task.

Choose the first open, unassigned child in map order with no open
blockers. Claim it by assigning yourself. Resolve it by commenting
with the result, closing it, and adding a result summary and link
to the map's Decisions-so-far.
