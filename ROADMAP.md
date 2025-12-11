# WodStrat Product Roadmap

---

## Phase 1 — Athlete Profile & Benchmark Tracking (MVP Data Layer)

**Goal:** Build the minimum data model required to personalize WOD strategies.

### Features

#### Backend
- Athlete profile model (age, height/weight, experience level, etc.)
- Benchmark entities (Fran, 500m row, max pull-ups, barbell loads, etc.)
- CRUD endpoints for athlete profile & benchmarks
- Basic server-side validation

#### Frontend
- Profile input screen
- Benchmark list + add/edit/delete
- Onboarding flow prompting users to populate their data

#### Database
- First real schema migration (profiles, benchmarks)

### Value
This provides the minimum viable athlete model AI needs to generate personalized pacing and strategy.

---

## Phase 2 — Workout Input + Parsing Pipeline

**Goal:** Allow users to paste WOD text and convert it into structured data.

### Features

#### Workout Entry
- Paste-in workout text (AMRAP/For Time/EMOM/Intervals)
- Identify components:
  - Workout type
  - Rounds
  - Movements
  - Reps/loads
  - Time caps

#### Parsing Engine v1
- Keyword-based and pattern matching
- Normalize movement names (e.g., "T2B", "toes-to-bar" → toes_to_bar)
- Error detection for malformed workouts

#### UI Features
- Workout input screen
- Parsed workout preview

### Value
Creates the pipeline required for future AI-driven strategy insights.

---

## Phase 3 — Strategy Engine v1 (Rule-Based System)

**Goal:** Deliver the first functioning, non-AI WOD strategy system.

### Features

#### Backend Strategy Engine
Estimate workout difficulty based on:
- Athlete benchmarks
- Rep volume per movement
- Intensity classification

Provide:
- Pacing advice (e.g., "start at 80% effort")
- Rep scheme suggestions
- Expected time ranges
- Redline warnings

#### Frontend
- Strategy display page
- "Regenerate Strategy" button
- Confidence and difficulty scoring indicators

### Examples
- "Thrusters too close to your threshold → break into early sets"
- "Row pace should be between 1:55–2:00 based on your 500m benchmark"

### Value
Allows WodStrat to function without AI initially and establishes the format for future AI responses.

---

## Phase 4 — AI Strategy Engine v2 (LLM Integration)

**Goal:** Introduce personalized AI strategy built on athlete data.

### Features

#### Core Functionality
- Replace rule-based logic with LLM inference
- Provide AI with:
  - Athlete profile
  - Benchmark metrics
  - Parsed workout structure

#### AI-Generated Strategy Includes:
- Recommended pacing
- Rep schemes
- Suggested break points
- Scaling options
- Time estimates
- Risk factors (grip fatigue, lungs, barbell cycling, etc.)

#### Backend
- AI Strategy Service (adapter pattern)
- Caching of AI responses
- Cost control + safety features (tokens, rate-limits)

#### Frontend
- Rich strategy viewer
- Strategy explanation sections
- Variants:
  - "Aggressive Strategy"
  - "Smart & Safe Strategy"
  - "Scaled Strategy"

### Value
Becomes one of the first AI tactical coaches for CrossFit athletes.

---

## Phase 5 — Historical Insights & Athlete Analytics

**Goal:** Give users meaningful feedback loops and performance tracking.

### Features

#### Analytics
- Pace consistency charts
- PR tracking
- Movement weakness identification
- Benchmark trend lines
- Volume load graphs
- Intensity classification heatmaps

#### UI
- Analytics dashboard
- Recent workouts summary
- Personal improvement metrics

### Value
Reinforces user retention and personalization.

---

## Phase 6 — Integrations (SugarWOD, BTWB, Strava/Garmin)

**Goal:** Pull real data automatically into WodStrat.

### Integrations
- SugarWOD export importer (Open source + CSV-style import)
- Beyond The Whiteboard importer
- Garmin/Strava HR & pace sync
- CrossFit Open leaderboard profile sync

### Use Cases
- Sync benchmark PRs
- Sync daily class results
- Automatically adjust AI pacing based on previous effort consistency
- Detect fatigue and training load patterns

### Value
This makes WodStrat far more accurate and eliminates manual data entry.

---

## Phase 7 — Competition Mode (CrossFit Open, HYROX, Local Throwdowns)

**Goal:** Become the strategic preparation tool for competitive athletes.

### Features
- Event templates for known workout formats
- AI predictions:
  - Expected score/time range
  - Heat strategy
  - Pacing segments
- Per-movement "time lost" estimator
- Rehearsal simulations:
  - WOD divides into splits
  - Predicted slowdown at round X
- Warm-up protocols tailored to athlete profile

### Value
Moves WodStrat into the competitive athlete space where demand is high and sticky.

---

## Phase 8 — Social, Community & Coaching Features

**Goal:** Expand into shared challenges, coaching tools, and group analytics.

### Features

#### Social Features
- Share strategies with friends/box
- Leaderboards
- Group challenges (e.g., 5-week Open prep)

#### Coach Tools
- Coach view for multiple athletes
- Strategy generation per athlete
- Feedback loops
- Team dashboards

### Value
Expands WodStrat from personal tool → platform.

---

## Phase 9 — Premium Features & Monetization

### Subscription Features
- Unlimited AI strategies
- Competition mode
- Advanced analytics
- Integration syncing
- Personal periodization planner
- Weekly AI performance reports

### Enterprise/Gym Tier
- White-label box version
- Automated strategy generation for daily WODs
- Class insights & athlete stats
- Coach dashboards

### Value
Sustainable revenue model with high retention.

---

## Phase 10 — Long-Term Vision: The AI Functional Fitness Coach

WodStrat evolves beyond strategy breakdowns into a full adaptive training partner that:

- Understands athlete physiology
- Learns strengths/weaknesses
- Adjusts pacing recommendations dynamically
- Designs personalized programs
- Tracks fatigue & recovery
- Coaches long-term development across modalities
- Supports GAA, HYROX, CrossFit, running, and general training

At this stage, WodStrat becomes the **Strava + SugarWOD + AI Coach** for hybrid athletes.

---

## Summary Roadmap

| Phase | Milestone | Status |
|-------|-----------|--------|
| 0 | Base architecture & infra | ✅ Current |
| 1 | Athlete profile & benchmark data | ⏳ Next |
| 2 | Workout input & parsing | |
| 3 | Rule-based strategy engine | |
| 4 | AI strategy engine | |
| 5 | Analytics & insights | |
| 6 | Integrations | |
| 7 | Competition mode | |
| 8 | Community & coaching | |
| 9 | Monetization | |
| 10 | Full AI coaching ecosystem | |
