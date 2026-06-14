# Contributing to TechJobs

Thanks for helping improve TechJobs. The project is intentionally small and reproducible, so contributions should keep the pipeline easy to inspect and run.

## Good First Contributions

- Add tests for cleaning, deduplication, and source adapter behavior.
- Improve field normalization for title, company, location, work type, and skills.
- Add export helpers for CSV or JSON outputs.
- Improve documentation for running the notebook or scheduled jobs.
- Add new source adapters only when automated access is permitted by the provider.

## Source Adapter Rules

- Use public endpoints or company career-page APIs that allow this collection pattern.
- Keep request volume conservative and document any rate-limit assumptions.
- Do not commit credentials, private datasets, generated output, or browser-session data.
- Avoid sources that prohibit automated collection or require account-only access.

## Pull Request Checklist

- Keep the PR focused on one behavior or documentation improvement.
- Include tests for reusable Python logic when possible.
- Run the notebook or relevant tests before opening the PR.
- Confirm that no credentials or generated data are included.
