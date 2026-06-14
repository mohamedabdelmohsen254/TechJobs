import sys
import unittest
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
sys.path.insert(0, str(ROOT / "src"))

from techjobs.normalization import clean_title, detect_level, extract_skills


class NormalizationTests(unittest.TestCase):
    def test_clean_title_removes_salary_and_pipe_suffix(self):
        title = "Backend Engineer $80k - $120k per year | Remote"
        self.assertEqual(clean_title(title), "Backend Engineer")

    def test_detect_level_orders_common_seniority_terms(self):
        self.assertEqual(detect_level("Senior Data Engineer"), "Senior")
        self.assertEqual(detect_level("Junior Python Developer"), "Junior")
        self.assertEqual(detect_level("Software Engineer II"), "Mid")
        self.assertEqual(detect_level("Product Engineer"), "Not specified")

    def test_extract_skills_normalizes_labels(self):
        skills = extract_skills("Senior Node.js Developer", "React, AWS, C++ and REST API")
        self.assertEqual(skills, "aws, c++, node.js, react, rest api")

    def test_extract_skills_avoids_partial_word_matches(self):
        self.assertEqual(extract_skills("Governance Analyst", "maintains cargo records"), "")


if __name__ == "__main__":
    unittest.main()
