# AI Usage Declaration — BrickBlast: Velocity Market

AI tools were used as optional pairing aids for planning, code review, documentation drafting,
debugging suggestions, and submission organisation.

All AI-assisted output was reviewed, understood, tested, and owned by the team before final
submission.

AI was not used as a replacement for team responsibility. The final implementation, build
decisions, testing evidence, and submission package were validated by the team.

---

## Tools Used

| Tool | Purpose |
|------|---------|
| GitHub Copilot (in-IDE) | Code completion, region navigation, refactoring suggestions |
| GitHub Copilot Chat | Planning conversations, architecture recommendations, doc drafts |

---

## Scope of AI Assistance

| Area | AI Role | Team Verification |
|------|---------|------------------|
| Game loop architecture | Suggested region structure; team designed all transitions | Curtis reviewed all state logic |
| Store / economy system | Assisted with catalog schema design | Alyssa reviewed purchase/equip pipeline |
| Bonus-pack theming pipeline | Drafted `DrawBonusBody`, icon painters, `GetBonusPackColor` | Curtis reviewed and corrected `FillPolygon` fix |
| Documentation | Drafted README, architecture docs, testing log structure | All team members reviewed and edited |
| Testing entries | Suggested test structure; team ran all tests manually | All tests executed by named tester |
| Network sync service | Suggested `HttpClient` async pattern | Curtis reviewed offline fallback |

---

## What AI Did Not Do

- AI did not make final architectural decisions without team review.
- AI did not run or validate the build — the team ran all builds and tests.
- AI did not submit any deliverable on behalf of the team.
- AI-generated documentation was treated as a draft requiring human editing.

---

## Reviewed and Approved By

- **Curtis Loop** — Team Lead
- **Alyssa Puentes** — Co-Lead
- **Andrea Albisser** — Co-Lead

*Approval date: May 13, 2026*
