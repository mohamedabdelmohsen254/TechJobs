# Security Policy

## Supported Scope

Security handling currently covers the public TechJobs notebook pipeline, reusable helper code, and project documentation on the default branch.

## Reporting a Vulnerability

Please avoid posting credentials, private data, or exploit details in public issues. Use GitHub's private vulnerability reporting flow if available for this repository, or contact the maintainer through the GitHub profile linked to the repository owner.

A useful report includes:

- the affected file or workflow
- the impact and reproduction steps
- whether any credential or private data exposure is involved

## Secrets Policy

The repository should not contain API keys, account cookies, generated notebook output, or private datasets. Use local environment variables for credentials in experiments and keep them outside commits.
