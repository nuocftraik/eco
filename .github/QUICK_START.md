# Copilot Instructions - Quick Start Guide

> **For**: New developers or anyone setting up Copilot for this project  
> **Time needed**: 5 minutes  
> **Result**: Copilot will understand and follow project conventions

---

## 🚀 Quick Setup (3 Steps)

### Step 1: Pull Latest Code
```sh
git pull origin dev
```

This pulls all context files:
- `.github/copilot-instructions.md`
- `docs/ARCHITECTURE_DECISIONS.md`
- `docs/DEVELOPMENT_PATTERNS.md`

---

### Step 2: Restart IDE
```
Visual Studio: Close → Reopen solution
VS Code: Close → Reopen workspace
```

**Why**: Copilot loads instructions on startup

---

### Step 3: Verify It Works
```
Open Copilot Chat
Ask: "What are the naming conventions for service interfaces?"

Expected answer: "I{Capability}Service pattern..."

✅ If answer matches → You're done!
❌ If not → See Troubleshooting below
```

---

## 🧪 Run Full Validation (Optional)

```sh
# 1. Open validation file
code .github/COPILOT_VALIDATION.md

# 2. Ask all 18 test questions in Copilot Chat
# 3. Score your results

Target: 18/18 (perfect)
Acceptable: 15-17 (minor issues)
Needs work: <15 (review instructions)
```

---

## 📚 What Files Do What

| File | Purpose | When to read |
|------|---------|--------------|
| `.github/copilot-instructions.md` | Full context for AI | When Copilot behaves wrong |
| `docs/DEVELOPMENT_PATTERNS.md` | Quick reference | When coding (copy templates) |
| `docs/ARCHITECTURE_DECISIONS.md` | Why we chose patterns | When questioning decisions |
| `.github/README_COPILOT.md` | User manual | When first learning system |
| `.github/COPILOT_VALIDATION.md` | Test suite | After updating instructions |
| `.github/SYSTEM_OVERVIEW.md` | Complete explanation | When explaining to others |

---

## 🎯 Test Drive Copilot

### Test 1: Create Interface

```
1. Create file: src/Core/Application/Common/Storage/IStorageService.cs
2. Type: "public interface IStorageService"
3. Wait for Copilot suggestion

Expected suggestion:
/// <summary>
/// Service for storage operations
/// </summary>
public interface IStorageService : ITransientService
{
    Task<string> SaveAsync(Stream content, CancellationToken ct);
}

✅ If matches pattern → Working!
```

---

### Test 2: Implement Service

```
1. Create file: src/Infrastructure/Infrastructure/Storage/LocalStorageService.cs
2. Type: "public class LocalStorageService"
3. Wait for Copilot suggestion

Expected suggestion:
public class LocalStorageService : IStorageService
{
    private readonly StorageSettings _settings;
    private readonly ILogger<LocalStorageService> _logger;

    public LocalStorageService(
        IOptions<StorageSettings> settings,
        ILogger<LocalStorageService> logger)
    {
        _settings = settings.Value;
        _logger = logger;
    }
    
    public async Task<string> SaveAsync(Stream content, CancellationToken ct)
    {
        // ...
    }
}

✅ If matches pattern → Working!
```

---

### Test 3: Create Settings Class

```
1. Create file: src/Infrastructure/Infrastructure/Storage/StorageSettings.cs
2. Type: "public class StorageSettings"
3. Wait for Copilot suggestion

Expected suggestion:
/// <summary>
/// Configuration settings for storage
/// </summary>
public class StorageSettings
{
    public string BasePath { get; set; } = default!;
    public int MaxFileSizeMB { get; set; } = 10;
    public bool EnableCompression { get; set; }
}

✅ If matches pattern → Working!
```

---

## 🔧 Troubleshooting

### Problem: Copilot suggestions don't follow patterns

**Solution 1: Verify files exist**
```sh
ls -la .github/copilot-instructions.md
ls -la docs/ARCHITECTURE_DECISIONS.md
ls -la docs/DEVELOPMENT_PATTERNS.md

# All should show files
```

**Solution 2: Verify files committed**
```sh
git ls-files | grep -E "(copilot|ARCHITECTURE|DEVELOPMENT)"

# Should list:
# .github/copilot-instructions.md
# .github/README_COPILOT.md
# .github/COPILOT_VALIDATION.md
# docs/ARCHITECTURE_DECISIONS.md
# docs/DEVELOPMENT_PATTERNS.md
```

**Solution 3: Restart IDE**
```
Close Visual Studio / VS Code completely
Reopen workspace
Wait 10 seconds for Copilot to load
```

**Solution 4: Clear Copilot cache**
```
Windows:
Delete folder: %LocalAppData%\Microsoft\VisualStudio\Copilot

macOS:
Delete folder: ~/Library/Application Support/Code/User/globalStorage/github.copilot

Restart IDE after deleting
```

**Solution 5: Re-authenticate**
```
1. Sign out of GitHub in IDE
2. Sign back in
3. Verify Copilot subscription active
4. Restart IDE
```

---

### Problem: Copilot Chat not available

**Check**:
```
1. GitHub Copilot subscription active?
   → Check: https://github.com/settings/copilot

2. Copilot extension installed?
   → VS: Extensions → Manage Extensions → Search "GitHub Copilot"
   → VS Code: Extensions → Search "GitHub Copilot"

3. Signed in to GitHub?
   → Bottom right corner should show GitHub account

4. Workspace is trusted?
   → VS Code: File → Trust Folder
```

---

### Problem: Suggestions are generic (not project-specific)

**This means Copilot didn't load instructions**

**Fix**:
```
1. Verify you're in correct workspace
   → pwd should show D:\MyCode\eco (or your clone path)

2. Verify .github folder at repo root
   → ls .github
   → Should show copilot-instructions.md

3. Check Git branch
   → git branch
→ Should be on 'dev' branch (where instructions are)

4. Pull latest
   → git pull origin dev

5. Restart IDE
```

---

## 📖 Learning Resources

### For Developers

```
Start here:
1. .github/README_COPILOT.md (5 min read)
2. docs/DEVELOPMENT_PATTERNS.md (10 min read)
3. .github/copilot-instructions.md (20 min read)

Total: 35 minutes to understand full system
```

### For Team Leads

```
Understand system design:
1. .github/SYSTEM_OVERVIEW.md (15 min read)
2. docs/ARCHITECTURE_DECISIONS.md (10 min read)
3. .github/copilot-instructions.md (20 min read)

Total: 45 minutes to understand + explain to team
```

---

## 🎓 Common Questions

### Q: Do I need to read all instructions?

**A**: No!

```
For daily work: docs/DEVELOPMENT_PATTERNS.md (quick reference)
For deep understanding: .github/copilot-instructions.md (full context)
For AI behavior: Copilot reads instructions automatically
```

---

### Q: What if I disagree with a pattern?

**A**: 
```
1. Discuss in team meeting or PR comment
2. If team agrees: Update instructions + ARCHITECTURE_DECISIONS.md
3. Commit changes
4. Everyone pulls + restarts IDE
```

---

### Q: How often should I update instructions?

**A**:
```
✅ When: New pattern adopted by team
✅ When: Code review identifies common mistake
✅ When: Technology stack changes

❌ Don't: For one-off features
❌ Don't: For personal preferences
❌ Don't: More than once per week (let patterns stabilize)
```

---

### Q: Can I use this in other projects?

**A**: Yes!

```
1. Copy .github/copilot-instructions.md to new project
2. Edit project-specific sections:
   - Technology stack
   - Layer structure (if different)
   - Naming conventions (if different)
3. Keep generic patterns (async/await, error handling, etc.)
4. Commit to new project repo
5. Team uses same instructions in new project
```

---

## 🚀 Next Steps After Setup

### 1. Bookmark these files
```
Quick reference: docs/DEVELOPMENT_PATTERNS.md
Full context: .github/copilot-instructions.md
Validation: .github/COPILOT_VALIDATION.md
```

### 2. Run monthly validation
```
Open: .github/COPILOT_VALIDATION.md
Run all 18 tests
Score: ____ / 18

If < 18: Update instructions where tests failed
```

### 3. Contribute improvements
```
Found a pattern Copilot misses?
1. Update .github/copilot-instructions.md
2. Add test to .github/COPILOT_VALIDATION.md
3. Submit PR
4. Team reviews + merges
```

---

## 📞 Get Help

```
❓ Copilot not working: See Troubleshooting above
❓ Don't understand pattern: Read docs/ARCHITECTURE_DECISIONS.md (explains WHY)
❓ Need example: Check docs/DEVELOPMENT_PATTERNS.md (code templates)
❓ Want to contribute: Read .github/README_COPILOT.md (update process)
❓ System explanation: Read .github/SYSTEM_OVERVIEW.md (complete overview)

Still stuck? Ask in team chat: #dev-standards
```

---

## ✅ Checklist

Before starting work, verify:

```
□ Pulled latest code (git pull origin dev)
□ Restarted IDE (so Copilot loads instructions)
□ Ran basic validation test (ask Copilot about patterns)
□ Bookmarked DEVELOPMENT_PATTERNS.md for quick reference
□ Know where to find examples (docs/BUILD_*.md files)
```

---

**Setup Time**: 5 minutes  
**Learning Time**: 35 minutes (optional)  
**Benefits**: Lifetime (AI generates consistent code forever)

**Last Updated**: 2026-01-30  
**Maintained By**: ECO.WebApi Development Team

---

🎉 **You're all set! Start coding with Copilot as your project-aware assistant.**
