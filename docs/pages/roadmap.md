## Roadmap

This roadmap outlines the planned evolution of **Flecto**, focusing on improving extensibility, 
developer experience, and broader ecosystem integrations.

---

### 🔜 Short-Term Goals

These features are either in development or high-priority in the near term.

- **JOIN and ON support**: Enable `.Join(...)` and `.On(...)` clauses for building queries involving related tables.
- **Support GET requests**
- **GET request support with hybrid sorting**:
  - Example: `?sort=name,-createdAt` to mix ascending/descending.
- **DISTINCT support**:
  - Add support for deduplication of selected results via `.Distinct()`.
- **Support system-level extended filters**:
  - Add `.AndWhere(...)` APIs for advanced scenarios.
---

### 🔭 Long-Term Ideas

These ideas are under consideration but not yet scheduled.

- **Type-safe query composition with source generators**
- **JQL support**
- **UI autocomplete schema builder/generator** integration
- ...

---

### 📝 Notes

- All roadmap items are modular. Features will be added in a non-breaking, opt-in manner.
- You can follow the releases and progress via GitHub milestones and discussions.

_This roadmap may evolve based on feedback, contributions, and internal priorities._

