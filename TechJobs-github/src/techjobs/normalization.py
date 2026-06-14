# Normalization helpers shared by the TechJobs notebook and tests.

from __future__ import annotations

import re

SENIOR_TERMS = ("senior", "sr.", "sr ", "lead", "principal", "staff")
JUNIOR_TERMS = ("junior", "jr.", "jr ", "entry", "associate", "intern")
MID_TERMS = ("mid", "intermediate", " ii", " iii")

SKILL_PATTERNS = (
    (r"python", "python"),
    (r"java", "java"),
    (r"javascript", "javascript"),
    (r"typescript", "typescript"),
    (r"react", "react"),
    (r"angular", "angular"),
    (r"vue", "vue"),
    (r"node\.?js", "node.js"),
    (r"django", "django"),
    (r"flask", "flask"),
    (r"spring", "spring"),
    (r"docker", "docker"),
    (r"kubernetes", "kubernetes"),
    (r"aws", "aws"),
    (r"azure", "azure"),
    (r"gcp", "gcp"),
    (r"sql", "sql"),
    (r"nosql", "nosql"),
    (r"mongodb", "mongodb"),
    (r"postgresql", "postgresql"),
    (r"redis", "redis"),
    (r"git", "git"),
    (r"ci/cd", "ci/cd"),
    (r"agile", "agile"),
    (r"scrum", "scrum"),
    (r"machine learning", "machine learning"),
    (r"deep learning", "deep learning"),
    (r"tensorflow", "tensorflow"),
    (r"pytorch", "pytorch"),
    (r"nlp", "nlp"),
    (r"data science", "data science"),
    (r"devops", "devops"),
    (r"linux", "linux"),
    (r"c\+\+", "c++"),
    (r"c#", "c#"),
    (r"go", "go"),
    (r"rust", "rust"),
    (r"swift", "swift"),
    (r"kotlin", "kotlin"),
    (r"ruby", "ruby"),
    (r"graphql", "graphql"),
    (r"rest api", "rest api"),
    (r"microservices", "microservices"),
    (r"terraform", "terraform"),
    (r"ansible", "ansible"),
)


def clean_title(title: object) -> str:
    value = "" if title is None else str(title)
    value = re.sub(r"\$[\d,]+[kK]?\s*[-–]\s*\$[\d,]+[kK]?\s*(per\s+\w+)?", "", value)
    value = re.sub(r"\|.*$", "", value)
    value = re.sub(r"\s{2,}", " ", value)
    return value.strip()


def detect_level(title: object) -> str:
    text = "" if title is None else str(title).lower()
    if any(term in text for term in SENIOR_TERMS):
        return "Senior"
    if any(term in text for term in JUNIOR_TERMS):
        return "Junior"
    if any(term in text for term in MID_TERMS):
        return "Mid"
    return "Not specified"


def extract_skills(title: object, description: object = "") -> str:
    text = f"{title or ''} {description or ''}".lower()
    found = set()
    for pattern, label in SKILL_PATTERNS:
        if re.search(r"(?<!\w)(?:" + pattern + r")(?!\w)", text):
            found.add(label)
    return ", ".join(sorted(found))
