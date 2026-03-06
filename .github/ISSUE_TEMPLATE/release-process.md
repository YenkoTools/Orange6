---
name: Release Process
about: Generate a release document and tag the codebase for a new release. Assign to the engineer responsible for the release.
title: "Release Process — v[VERSION]"
labels: release
assignees: ''
---

## Description

Prepare a release document and apply a proper annotated git tag for the new version **v[VERSION]**.

Replace every occurrence of `[VERSION]` and `[PREVIOUS_VERSION]` with the actual version numbers before saving this issue.

---

## Steps

### 1. Verify the Current Git Tag

Confirm the tag the codebase is currently at before starting:

```bash
git fetch --prune --tags
git describe --tags --abbrev=0
```

The output should show **v[PREVIOUS_VERSION]** as the last released tag.

---

### 2. Generate the Release Document

- Review the git log since the codebase was tagged **v[PREVIOUS_VERSION]**:

  ```bash
  git log v[PREVIOUS_VERSION]..HEAD --oneline
  ```

- Organize the changes by Service Assembly:
  - Api
  - Application
  - Domain
  - Infrastructure
  - Tests
  - Client
  - Documentation

- Summarize the changes into a bulleted list per assembly.

- Name the new release markdown document: **Release-[VERSION].md**

- Use the structure in [Release-0.0.3.md](../../Release-0.0.3.md) as the template for the new document.

---

### 3. Tag the Codebase

Once the release document has been merged into the main branch, apply an annotated git tag using the helper script:

```powershell
.\git-tag.ps1 -version v[VERSION] -message "Release v[VERSION]"
```

> **Note:** The `git-tag.ps1` script creates an annotated tag and pushes it to the remote repository automatically. Refer to [git-tag.md](../../git-tag.md) for full usage details.

---

### 4. Verify the Tag

Confirm the tag was applied and pushed successfully:

```bash
git fetch --prune --tags
git tag -l "v[VERSION]"
git show v[VERSION] --stat
```

Verify the tag points to the correct commit on the main branch.

---

### 5. Run the Version Script

Update the `version.json` files for both the Client and Service assemblies:

```powershell
.\version.ps1
```

Confirm that the `GitTag` field in the generated `version.json` reflects **v[VERSION]**.

---

## Acceptance Criteria

- [ ] `Release-[VERSION].md` exists in the repository root and follows the standard release document structure.
- [ ] Changes are organized by Service Assembly (Api, Application, Domain, Infrastructure, Tests, Client, Documentation).
- [ ] The codebase is tagged with the annotated tag `v[VERSION]`.
- [ ] The tag is visible in the remote repository (`git tag -l`).
- [ ] `version.json` (Client and Service) reflects the new tag `v[VERSION]`.
