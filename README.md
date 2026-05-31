# TechJobs - Egypt Tech Jobs Pipeline

TechJobs is an open-source data pipeline for collecting and cleaning recent
technology job listings that may be relevant to job seekers in Egypt.

The current public reference implementation is a small Jupyter notebook in
[`TechJobs-github/`](TechJobs-github). It focuses on three public company-career
sources that do not require private API credentials:

- Workable company career pages
- Greenhouse job boards
- Lever career pages

## Why This Project Exists

Technology job listings are distributed across many company career pages and
job-board providers. TechJobs provides a reproducible starting point for
collecting recent listings into one consistent dataset that can be analyzed,
filtered, or extended by contributors.

## Current Features

- Fetch listings posted during the last 24 hours.
- Configure public company slugs and board tokens in one notebook cell.
- Normalize title, company, location, work type, and application URL fields.
- Detect Junior, Mid, and Senior experience levels conservatively.
- Extract common technical skills from job titles.
- Deduplicate listings by URL and normalized title/company pairs.
- Produce a final pandas DataFrame for further analysis or export.

## Pipeline

```text
Public career APIs
        |
        v
Recent-listing filter (24 hours)
        |
        v
Field normalization and skill extraction
        |
        v
URL and title/company deduplication
        |
        v
Final pandas DataFrame
```

## Quick Start

Requirements:

- Python 3.10+
- Jupyter Notebook or JupyterLab

Run:

```bash
git clone https://github.com/mohamedabdelmohsen254/TechJobs.git
cd TechJobs/TechJobs-github
python -m pip install -r requirements.txt
jupyter notebook notebook/Egypt_Jobs_Pipeline.ipynb
```

Run the notebook cells in order. No private API key is required for the current
three-source pipeline.

## Privacy and Security

This repository intentionally excludes credentials, private datasets, and
generated notebook output. Contributors should use only public endpoints and
respect each provider's terms and rate limits.

## Roadmap

- Add automated tests for normalization and deduplication rules.
- Add optional CSV and JSON export helpers.
- Add more public career-page adapters where permitted.
- Improve Egypt-specific relevance filtering.
- Document reproducible scheduled execution.

## Contributing

Issues and pull requests are welcome. Useful contributions include source
adapters, test cases, normalization improvements, documentation, and export
options.

## License

The reference implementation in [`TechJobs-github/`](TechJobs-github) is
available under the MIT License.
