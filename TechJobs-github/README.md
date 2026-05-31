# TechJobs - Egypt Tech Jobs Pipeline

TechJobs is a small open-source data pipeline for collecting recent technology
job listings from public company career APIs. It fetches listings posted during
the last 24 hours, normalizes selected fields, detects experience level and
skills, and removes duplicates.

The current implementation is a Jupyter notebook focused on three public
sources:

- Workable company career pages
- Greenhouse job boards
- Lever career pages

## Features

- Collect recent listings from configurable company slugs and board tokens.
- Normalize title, company, location, work type, and application URL fields.
- Detect Junior, Mid, and Senior experience levels conservatively.
- Extract common technical skills from job titles.
- Deduplicate listings by application URL and normalized title/company pairs.
- Produce a final pandas DataFrame for further analysis or export.

## Quick Start

### Requirements

- Python 3.10+
- Jupyter Notebook or JupyterLab
- `pandas`
- `requests`

### Run

```bash
git clone https://github.com/mohamedabdelmohsen254/TechJobs.git
cd TechJobs
python -m pip install -r requirements.txt
jupyter notebook notebook/Egypt_Jobs_Pipeline.ipynb
```

Run the notebook cells in order. Company identifiers are configured near the
top of the notebook:

```python
WORKABLE_SLUGS = [...]
GREENHOUSE_TOKENS = [...]
LEVER_SLUGS = [...]
```

Greenhouse board tokens are public job-board identifiers, not API secrets.

## Project Structure

```text
.
|-- notebook/
|   `-- Egypt_Jobs_Pipeline.ipynb
|-- .gitignore
|-- LICENSE
|-- README.md
`-- requirements.txt
```

## Contributing

Contributions are welcome. Useful improvements include testable normalization
rules, additional public career-page integrations, and export options. Do not
commit credentials, private datasets, or generated notebook output.

## License

MIT License. See [LICENSE](LICENSE).
