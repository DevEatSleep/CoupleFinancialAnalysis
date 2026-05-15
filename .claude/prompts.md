# Prompt Library — CoupleFinancialAnalysis

Slash commands handle most tasks. Use the free-form prompts below only for cases not covered by a command.

---

## Slash Commands (use these first)

| Command | When to use |
|---|---|
| `/fix @Controller.cs` | Diagnose and fix a bug, get a regression test |
| `/refactor @Service.cs` | Clean refactor with minimal diff, SOLID-compliant |
| `/feature add language persistence` | New feature — plan first, then code |
| `/test @BotService.cs` | Generate unit tests for a file |
| `/audit @Controllers/` | Code quality audit with prioritized issues |
| `/clean @Home.razor` | Remove dead code, unused imports, duplication |
| `/security @AuthController.cs` | Security vulnerability scan with fixes |
| `/analyze` | Full codebase SOLID audit and improvement plan |
| `/commit` | Generate a Conventional Commit from staged changes |

---

## Free-form Prompts (not covered by slash commands)

### Architecture

```
Propose a clean architecture for [module].
Follow SOLID, respect the existing project conventions (ASP.NET Core controllers,
Blazor WASM services, shared constants in Shared/Constants.cs).
```

### Technical migration

```
Migrate @OldService.cs to [new approach].
Propose a plan file by file, then apply diffs one at a time.
```

### Update tests after a change

```
Update the tests impacted by the changes made to @Service.cs.
Explain what changed and why each test was updated.
```

### Test plan for a feature

```
Write a complete test plan for the [feature] feature.
Cover happy path, edge cases, and error scenarios.
```

### Performance optimization

```
Optimize @Method.cs for performance without changing its observable behavior.
Explain the bottleneck and the fix.
```

### Commit history cleanup

```
Group the current staged changes into a single coherent commit.
Propose a Conventional Commit message: type(scope): description.
```

### Add strict input validation

```
Add strong input validation to @Controller.cs.
Prevent injection, missing fields, and out-of-range values.
Follow the existing validation style in the project.
```
