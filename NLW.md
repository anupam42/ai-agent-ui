# NLW — Natural Language → Wireframe → Code

## Vision

A tool where anyone — designer, developer, or product manager — can describe a UI screen in plain English and get a **visual interactive wireframe** rendered live in the browser, then convert it to production-ready Angular component code in one click.

No Figma. No boilerplate. Just describe it.

---

## The 3-Step Flow

```
[1] User types a description
        ↓
[2] AI generates a visual wireframe (rendered in browser as interactive blocks)
        ↓
[3] User clicks "Generate Code" → Angular component code is produced
```

---

## What Makes It Unique

| Existing Tools | NLW |
|---|---|
| Generate static images (screenshots) | Renders a live, interactive wireframe in the browser |
| Stop at the wireframe | Converts wireframe to real Angular code |
| Generic output | Code maps to YOUR company's actual design system components |
| One-shot generation | Iterative — refine with follow-up prompts |

---

## Features

### F1 — Natural Language Input Panel
- Text prompt area: "Create a dashboard with a sidebar, stats cards at the top, and a data table below"
- Example prompts / suggestions to guide users
- Conversation history — user can refine: "move the sidebar to the right" or "add a search bar to the header"
- AI remembers context within the session

### F2 — Live Wireframe Renderer
- AI returns a structured JSON layout schema
- Angular renders the JSON as a low-fidelity wireframe:
  - Gray blocks with labels
  - Clear visual hierarchy (header, sidebar, main content, footer)
  - Placeholder text, icons, and images shown as boxes
- No images — pure browser-rendered components
- Responsive preview: toggle between Desktop / Tablet / Mobile

### F3 — Wireframe Interaction & Editing
- Click any element to see its type, label, and mapped component
- Drag to reorder blocks
- Right-click → "Ask AI to change this" for targeted edits
- Undo / redo support

### F4 — AI Iteration via Chat
- After wireframe is generated, continue chatting:
  - "Make the header sticky"
  - "Replace the table with a card grid"
  - "Add a notification bell to the top right"
- Wireframe updates live with each instruction
- Changes are tracked so user can compare versions

### F5 — Component Mapping Panel
- Shows how each wireframe block maps to your design system:
  - `header block` → `AppHeaderComponent`
  - `data table` → `MatTable + PaginatorComponent`
  - `stats card` → `SummaryCardComponent`
- Falls back to Angular Material or Bootstrap when no match exists
- User can override the mapping manually

### F6 — Code Preview & Export
- Split view: wireframe on the left, generated code on the right
- Code tabs:
  - `component.html` — Angular template
  - `component.ts` — Component class with inputs/outputs
  - `component.scss` — Scoped styles
- Syntax highlighted, read-only preview
- Actions:
  - Copy to clipboard
  - Download as `.zip`
  - (Future) Push directly to a branch

### F7 — Version History
- Each iteration saved as a named version
- User can name versions: "v1 - initial", "v2 - sidebar removed"
- Diff view between versions
- Restore any previous version

### F8 — Design System Awareness
- Backend is seeded with your company's Angular component metadata
- AI uses this context to generate code that matches existing patterns
- Enforces design tokens: spacing, color, typography
- Flags if a requested element has no matching component

---

## Screen Layout

```
┌─────────────────────────────────────────────────────────────────┐
│  NLW — Natural Language Wireframe                    [History]  │
├──────────────────────┬──────────────────────┬───────────────────┤
│                      │                      │                   │
│   PROMPT / CHAT      │   WIREFRAME CANVAS   │   CODE PREVIEW    │
│                      │                      │                   │
│  "Describe your UI"  │  [Live rendered       │  component.html   │
│                      │   wireframe blocks]  │  component.ts     │
│  [Conversation       │                      │  component.scss   │
│   history below]     │  [Desktop|Tab|Mobile]│                   │
│                      │                      │  [Copy] [Download]│
│  [Send]              │  [Generate Code →]   │                   │
└──────────────────────┴──────────────────────┴───────────────────┘
```

---

## AI Architecture

### Wireframe Generation
1. User prompt → Backend
2. Backend builds structured prompt:
   - User description
   - Available component metadata (from design system index)
   - Layout rules JSON
3. AI returns a **Wireframe JSON schema**
4. Frontend renders schema as visual blocks

### Wireframe JSON Schema (example)
```json
{
  "layout": "column",
  "children": [
    {
      "type": "header",
      "label": "App Header",
      "sticky": true,
      "children": [
        { "type": "logo", "label": "Logo" },
        { "type": "nav", "items": ["Home", "Reports", "Settings"] },
        { "type": "icon-button", "label": "Notifications" }
      ]
    },
    {
      "type": "row",
      "children": [
        {
          "type": "sidebar",
          "width": 240,
          "items": ["Dashboard", "Analytics", "Users"]
        },
        {
          "type": "main",
          "children": [
            { "type": "stat-card", "label": "Total Users", "span": 4 },
            { "type": "stat-card", "label": "Revenue", "span": 4 },
            { "type": "stat-card", "label": "Active Sessions", "span": 4 },
            { "type": "data-table", "label": "Recent Orders", "span": 12 }
          ]
        }
      ]
    }
  ]
}
```

### Code Generation
1. Wireframe JSON → Component mapping (via design system index)
2. AI generates Angular template using mapped components
3. Backend validates output structure
4. Frontend displays in code preview panel

---

## Tech Stack

| Layer | Technology |
|---|---|
| Frontend | Angular |
| Wireframe Renderer | Angular dynamic components + JSON schema |
| Backend | .NET Core (AI Orchestrator) |
| AI Model | Claude (Anthropic) — recommended for structured JSON output |
| Design System Index | JSON metadata + Vector DB (semantic search) |
| Real-time updates | SignalR or SSE (streaming wireframe updates) |

---

## Implementation Phases

| Phase | Features | Goal |
|---|---|---|
| Phase 1 | F1 (Prompt Input) + F2 (Wireframe Renderer) | Describe → See wireframe |
| Phase 2 | F4 (AI Iteration) + F6 (Code Preview) | Refine → Get code |
| Phase 3 | F5 (Component Mapping) + F8 (Design System) | Company-specific code |
| Phase 4 | F3 (Wireframe Editing) + F7 (Version History) | Polish + UX |
| Phase 5 | Mobile preview, export to zip, branch push | Production readiness |

---

## What You Learn Building This

| AI Concept | Where You Apply It |
|---|---|
| Prompt engineering | Structured JSON generation from natural language |
| RAG | Feeding design system components as AI context |
| Streaming responses | Live wireframe updates as AI responds |
| JSON schema design | Wireframe schema that maps to real components |
| Dynamic rendering | Angular rendering AI-generated JSON as UI |

---

## Open Questions

- Should the wireframe be low-fidelity (gray blocks) or styled (actual components)?
  - Recommendation: start low-fidelity, add styled mode in Phase 5
- Should users be able to save and share wireframes?
  - Yes — add shareable link feature in Phase 4
- Should AI generate multiple layout variants to choose from?
  - Yes — "Show me 3 layouts" as a follow-up prompt
- What Angular version and design system is the company using?
  - This determines the component metadata we feed to AI
