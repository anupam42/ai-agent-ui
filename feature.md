# AI Mentor-Mentee Platform — Feature Specification

## Vision

An AI-powered mentor-mentee platform where AI acts as a **always-on learning companion** for mentees and an **intelligent assistant** for mentors — amplifying the mentor's impact without replacing the human relationship.

---

## Core Concept: AI Learning Companion

AI sits alongside every mentee as a live coach between mentor sessions:
- Mentees get instant answers so they don't block on mentor availability
- AI tracks struggles and briefs the mentor before each session
- Mentor sets goals → AI enforces and reinforces them daily

---

## Features

### F1 — AI Chat Assistant (Mentee-facing)
- Always-on chat scoped to the mentee's learning goal
- AI answers questions within the context of the mentor-defined roadmap
- AI escalates to mentor when question is outside its scope
- Tracks unanswered or repeated questions for mentor review

### F2 — AI-Generated Learning Roadmap
- Mentor defines a high-level goal (e.g., "become a backend engineer in 6 months")
- AI generates a week-by-week learning path
- Mentor reviews and approves the roadmap
- AI adapts the roadmap dynamically based on mentee progress

### F3 — Session Prep Brief (Mentor-facing)
- Before each session, AI generates a brief for the mentor:
  - What the mentee worked on since last session
  - Topics the mentee struggled with
  - Suggested talking points and questions
- Allows one mentor to effectively guide multiple mentees

### F4 — Session Summarizer + Action Items
- After each mentor-mentee session (chat or notes input):
  - AI generates a session summary
  - Extracts action items for the mentee
  - Sets follow-up reminders
  - Archives session history for both parties

### F5 — Progress Dashboard (Mentee)
- Visual timeline of the learning roadmap
- AI-highlighted gaps and at-risk milestones
- Daily nudges and check-ins from AI
- Streak and consistency tracking

### F6 — Mentor Overview Dashboard
- See all mentees and their AI activity summaries at a glance
- Flag mentees who are falling behind (AI-detected)
- Quick access to session history and roadmap per mentee
- Mentor can update goals or roadmap from this view

### F7 — Goal & Milestone Management
- Mentor sets goals and milestones
- AI breaks milestones into tasks and suggests resources
- Mentee marks tasks complete
- AI validates completion with optional quiz or reflection prompt

---

## User Roles

| Role | Primary Use |
|---|---|
| **Mentee** | Chat with AI, view roadmap, complete tasks, join sessions |
| **Mentor** | Set goals, review AI briefs, conduct sessions, monitor progress |
| **Admin** | Manage pairings, platform settings |

---

## Key Screens

| Screen | Role | Purpose |
|---|---|---|
| Mentee Dashboard | Mentee | Daily AI nudge, pending tasks, progress summary |
| AI Chat | Mentee | Always-on assistant scoped to learning goal |
| Learning Roadmap | Mentee | Visual week-by-week plan with progress indicators |
| Session Prep | Mentor | AI-generated brief before each session |
| Mentor Overview | Mentor | All mentees, activity summaries, flags |
| Session Notes | Both | Post-session summary and action items |
| Goal Manager | Mentor | Set and update goals, milestones, roadmap |

---

## What Makes It Unique

The AI does not replace the mentor — it **amplifies** the mentor's time. With AI handling between-session support, one mentor can effectively guide 10x more mentees while maintaining a high-quality, personalized experience.

---

## Tech Stack (Proposed)

- **Frontend:** Angular
- **Backend:** .NET Core (AI Orchestrator)
- **AI Model:** Claude (Anthropic) or OpenAI
- **Database:** PostgreSQL + Vector DB (for semantic search on learning content)
- **Real-time:** SignalR (for chat and notifications)

---

## Implementation Phases

| Phase | Features | Priority |
|---|---|---|
| Phase 1 | AI Chat Assistant (F1) + Mentee Dashboard (F5) | P0 |
| Phase 2 | Learning Roadmap generation (F2) + Goal Manager (F7) | P0 |
| Phase 3 | Session Prep Brief (F3) + Session Summarizer (F4) | P1 |
| Phase 4 | Mentor Overview Dashboard (F6) | P1 |
| Phase 5 | Adaptive roadmap, escalation, quizzes | P2 |

---

## Open Questions

- How are mentor-mentee pairs created? (admin-assigned vs self-service)
- What is the session format? (in-platform chat, video, or external tool like Zoom)
- Should AI have memory across sessions? (recommended: yes, per mentee)
- What content sources can AI reference? (curated library, web, mentor-uploaded docs)
