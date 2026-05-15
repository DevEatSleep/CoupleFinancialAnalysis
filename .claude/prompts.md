# 📄 **prompts.md — Claude Code Prompt Library (English Version)**

## 🧱 **1. Editing Existing Code**
- **Edit a function**  
  “Edit the function in @file.ts.  
  Apply a minimal diff, follow SOLID principles, and briefly explain the changes.”

- **Fix a bug**  
  “Analyze the bug in @file.ts, identify the root cause, propose a clean fix, and provide an associated test.”

- **Refactor cleanly**  
  “Refactor this module while respecting SOLID.  
  Do not change anything unnecessary. Provide a clear diff.”

---

## 🧩 **2. Multi‑File Work (Composer)**
- **Multi‑file refactor**  
  “Use Composer to refactor these files: @src/service, @src/utils.  
  Provide a plan before making changes. Follow SOLID.”

- **Create a new feature**  
  “Create the feature ‘X’.  
  Generate required files, architecture, interfaces, and tests.  
  Use Composer to structure everything cleanly.”

- **Technical migration**  
  “Migrate this module to <new tech>.  
  Provide a plan, then apply diffs file by file.”

---

## 🧠 **3. Analysis & Architecture**
- **Analyze the project**  
  “Analyze the entire codebase.  
  Identify SOLID violations, technical debt, and duplication.  
  Provide a prioritized improvement plan.”

- **Design an architecture**  
  “Propose a clean architecture for this module.  
  Follow SOLID, DDD when relevant, and existing project conventions.”

- **Code quality audit**  
  “Perform a quality audit on @src.  
  List issues, risks, inconsistencies, and propose fixes.”

---

## 🧪 **4. Testing**
- **Generate tests**  
  “Generate unit tests for @file.ts.  
  Cover edge cases, errors, and expected behavior.”

- **Update tests**  
  “Update tests impacted by this change.  
  Explain what was modified.”

- **Create a test plan**  
  “Provide a complete test plan for this feature.”

---

## 🔄 **5. Git & Smart Commits**
- **Generate a commit message**  
  “Generate a Conventional Commit message based on the diffs.  
  Format: type(scope): short description.”

- **Automatic commit**  
  “After applying the diffs, propose a commit message, then run:  
  `git add . && git commit -m "<message>"`  
  (only after my approval).”

- **Clean commit history**  
  “Group these changes into a single coherent commit.  
  Provide a clear message.”

---

## 🧼 **6. Cleanup & Optimization**
- **Clean the code**  
  “Clean this file: dead code, duplication, unused imports.  
  Follow SOLID and existing conventions.”

- **Optimize performance**  
  “Optimize this function for performance without changing behavior.”

- **Simplify a module**  
  “Simplify this overly complex module.  
  Propose an SRP‑compliant breakdown.”

---

## 🛠️ **7. Code Creation**
- **Create a service**  
  “Create a SOLID‑compliant service with interface, implementation, and tests.”

- **Create a UI component**  
  “Create a clean, typed, testable, and documented UI component.”

- **Create an API endpoint**  
  “Create a full API endpoint: route, controller, service, validation, tests.”

---

## 🔒 **8. Security**
- **Security audit**  
  “Analyze this module for security vulnerabilities.  
  Propose fixes.”

- **Strict validation**  
  “Add strong validation to prevent injections, XSS, and other attacks.”