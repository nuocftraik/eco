# Copilot Context Files - How to Use

## Overview

This directory contains instructions for AI code assistants (GitHub Copilot, Cursor AI, etc.) to understand and follow project conventions.

---

## Files in This Directory

### `.github/copilot-instructions.md` (PRIMARY FILE)

**Purpose**: Main context file for AI assistants

**What it contains:**
- Project architecture overview
- Naming conventions (interfaces, classes, settings)
- Dependency injection patterns
- Configuration binding rules
- Code style standards
- MediatR/CQRS patterns
- File organization structure
- Documentation requirements
- Testing standards
- Common pitfalls & anti-patterns

**Who uses it:**
- GitHub Copilot (auto-loaded in VS/VS Code)
- Cursor AI
- Any AI assistant that scans workspace

**Update frequency:**
- When architectural patterns change
- When new conventions are adopted
- After team code reviews identify new patterns

---

## Supporting Documentation

### `docs/ARCHITECTURE_DECISIONS.md`

**Purpose**: Record of architectural decisions (ADRs)

**Format**: Each decision is a separate ADR entry with:
- Context (why we faced this decision)
- Decision (what we chose)
- Rationale (why we chose it)
- Alternatives considered
- Consequences (tradeoffs)

**Example entries:**
- ADR-001: Clean Architecture with DDD
- ADR-002: MediatR for CQRS
- ADR-003: MailKit for email service
- ADR-005: Marker interfaces for DI

**When to add new ADR:**
- Choosing between technology options (library A vs B)
- Adopting new architectural pattern
- Changing existing pattern (deprecating old approach)

---

### `docs/DEVELOPMENT_PATTERNS.md`

**Purpose**: Quick reference for common patterns

**What it contains:**
- Code templates (copy-paste starting points)
- Naming convention lookup table
- File organization checklist
- Testing patterns
- Troubleshooting guide
- Quick commands (EF migrations, build, run)

**Target audience:**
- AI assistants (for code generation)
- New developers (onboarding reference)
- Experienced developers (quick lookup)

---

## How AI Assistants Use These Files

### GitHub Copilot

```
1. When VS/VS Code opens workspace:
   → Scans for .github/copilot-instructions.md
   → Loads into context window

2. When generating code:
   → Follows patterns from instructions
   → Suggests naming conventions
   → Applies architectural rules

3. When uncertain:
   → References docs/DEVELOPMENT_PATTERNS.md
   → Checks docs/ARCHITECTURE_DECISIONS.md
   → May ask user for clarification
```

### Cursor AI

```
Similar behavior:
- Loads workspace instructions
- Applies patterns to suggestions
- Uses documentation for context
```

### Other AI Assistants

```
If your AI tool supports workspace context:
1. Point it to .github/copilot-instructions.md
2. Reference docs/DEVELOPMENT_PATTERNS.md for examples
3. Check docs/ARCHITECTURE_DECISIONS.md for rationale
```

---

## Verification: Is Copilot Using Instructions?

### Test 1: Ask Direct Questions

```
Q: "What are the naming conventions for service interfaces?"
Expected: "I{Capability}Service pattern, e.g., IMailService"

Q: "How should I structure a new feature?"
Expected: Describes Application → Infrastructure → Host layers

Q: "What marker interface should a transient service use?"
Expected: "ITransientService"
```

### Test 2: Code Suggestions

```
Scenario: Create new file in Application/Common/Storage/
Type: "public interface IStorageService"

Expected suggestions:
- Inherit from ITransientService
- Add XML documentation comment
- Method signature with CancellationToken
```

### Test 3: Pattern Recognition

```
Scenario: Create new settings class
Type: "public class StorageSettings"

Expected suggestions:
- Properties with { get; set; } = default!;
- Match appsettings.json section name
- XML documentation comments
```

---

## Troubleshooting

### Problem: Copilot Not Following Patterns

**Solutions:**

1. **Verify file exists**
   ```sh
   ls -la .github/copilot-instructions.md
   # Should show file at repo root
   ```

2. **Verify file committed**
   ```sh
   git ls-files | grep copilot-instructions
   # Should list .github/copilot-instructions.md
   ```

3. **Restart IDE**
   - Close Visual Studio / VS Code
   - Reopen workspace
   - Copilot reloads context on startup

4. **Clear Copilot cache** (if needed)
   - Windows: Delete `%LocalAppData%\Microsoft\VisualStudio\Copilot`
   - macOS: Delete `~/Library/Application Support/Code/User/globalStorage/github.copilot`

5. **Re-authenticate Copilot**
   - Sign out of GitHub account in IDE
   - Sign back in
   - Verify Copilot subscription active

---

### Problem: Instructions Too Generic or Too Specific

**Balance guidelines:**

```
❌ TOO SPECIFIC:
"When creating email service, use MailKit version 4.3.0"
→ Problem: Only applies to one module

✅ GENERIC:
"When creating external service integration, use pattern: {Tech}{Capability}Service"
→ Better: Applies to any module

❌ TOO GENERIC:
"Follow clean code principles"
→ Problem: Too vague, no actionable guidance

✅ SPECIFIC ENOUGH:
"Service interfaces must inherit from ITransientService for DI auto-registration"
→ Better: Clear, actionable rule
```

---

## Updating Instructions

### When to Update

```
✅ Update when:
- New architectural pattern adopted (e.g., switching to vertical slice)
- Technology stack changes (e.g., migrating from MediatR to another library)
- Team identifies common mistakes in code reviews
- New best practices emerge (e.g., performance optimizations)

❌ Don't update for:
- One-off feature-specific details
- Temporary workarounds
- Personal preferences not agreed by team
```

### Update Process

```sh
# 1. Edit instructions
code .github/copilot-instructions.md

# 2. Add new section or modify existing
## New Pattern: [Pattern Name]
[Description]
[Examples]
[Rules]

# 3. Test with Copilot
# - Ask questions to verify understanding
# - Generate sample code to check suggestions

# 4. Commit changes
git add .github/copilot-instructions.md
git commit -m "Update Copilot instructions: Add [Pattern Name]"
git push origin dev

# 5. Notify team
# - Post in Slack/Teams: "Updated Copilot instructions with [Pattern]"
# - Team members pull latest changes
# - Everyone restarts IDE for Copilot to reload
```

---

## Cross-Machine Consistency

### How It Works

```
Developer A (Office Laptop):
1. Works on feature X following instructions
2. Commits code + updates instructions (if needed)
3. Pushes to Git

Developer B (Home Desktop):
1. Pulls latest from Git
2. Copilot loads .github/copilot-instructions.md
3. Works on feature Y with same patterns
4. Code looks consistent (same naming, structure, style)

Result: Code appears written by one person, even across team
```

### Benefits

```
✅ Onboarding: New developers get instant context via Copilot
✅ Consistency: Same patterns across all machines/developers
✅ Documentation: Instructions = executable documentation
✅ Knowledge Preservation: Team knowledge stored in Git, not heads
✅ AI Alignment: All AI assistants follow same rules
```

---

## Best Practices

### 1. Keep Instructions Generic

```markdown
❌ BAD: "When implementing email service, use SMTP port 587"
✅ GOOD: "When implementing external service, use {Tech}{Feature}Service pattern"
```

### 2. Provide Examples, Not Just Rules

```markdown
✅ GOOD:
"Naming pattern: I{Capability}Service

Examples:
- IMailService (email capability)
- IFileStorageService (storage capability)
- INotificationService (notification capability)

Avoid:
- IEmailService (ambiguous)
- ISmtpService (too specific)
"
```

### 3. Explain WHY, Not Just WHAT

```markdown
✅ GOOD:
"Use constructor injection (not property injection)

Rationale:
- Dependencies are explicit and required
- Prevents runtime NullReferenceException
- Easier to unit test (mock dependencies)
- Follows SOLID principles (explicit dependencies)
"
```

### 4. Use Consistent Formatting

```markdown
✅ GOOD:
// Pattern: {Template}
✅ public class GoodExample { }
❌ public class BadExample { }

Rationale: [Explanation]
```

### 5. Version Your Instructions

```markdown
**Version**: 2.0
**Last Updated**: 2026-01-30
**Changed Since Last Version**:
- Added marker interface pattern
- Clarified settings naming convention
- Removed email-specific examples
```

---

## FAQ

### Q: Do I need to restart IDE after updating instructions?

**A**: Yes, for GitHub Copilot. It loads context on workspace open, not continuously.

---

### Q: Can I have multiple instruction files?

**A**: Yes, you can create:
- `.github/copilot-instructions.md` (main)
- `.github/copilot-csharp.md` (language-specific)
- `.github/copilot-testing.md` (testing-specific)

Copilot may load all `.github/copilot-*.md` files.

---

### Q: How do I know if Copilot read my instructions?

**A**: Test with questions:
- Ask about specific patterns you documented
- Check code suggestions for compliance
- Generate sample code and verify structure

---

### Q: What if team disagrees on a pattern?

**A**: 
1. Discuss in team meeting or code review
2. Document decision in `ARCHITECTURE_DECISIONS.md` (ADR)
3. Update instructions to reflect agreed pattern
4. Commit and push so everyone has latest

---

### Q: Should I document third-party library usage?

**A**: Only if you have **project-specific patterns** for using it.

```markdown
❌ DON'T document:
"Entity Framework Core is an ORM" (obvious)

✅ DO document:
"Always use AsNoTracking() for read-only queries in this project"
(project-specific convention)
```

---

## Maintenance Schedule

### Weekly
- [ ] Review code PRs for new patterns to document
- [ ] Check if Copilot suggestions still align with rules

### Monthly
- [ ] Update examples if project structure changed
- [ ] Verify all links in documentation are valid
- [ ] Review ADRs for deprecated decisions

### Quarterly
- [ ] Major review of instructions file
- [ ] Remove outdated patterns
- [ ] Consolidate duplicate rules
- [ ] Version bump if significant changes

---

## Contact

For questions about these instructions:
- **File an issue**: GitHub repo issues
- **Discuss in team chat**: #dev-standards channel
- **Update via PR**: Submit changes for team review

---

**Maintained By**: ECO.WebApi Development Team  
**Last Updated**: 2026-01-30
