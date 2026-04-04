import json
import math
import sys
from dataclasses import dataclass
from typing import List, Optional

import numpy as np


def c_factor(n: int) -> float:
    if n <= 1:
        return 0.0
    if n == 2:
        return 1.0
    return 2.0 * (math.log(n - 1) + 0.5772156649) - (2.0 * (n - 1) / n)


@dataclass
class Node:
    size: int
    feature: Optional[int] = None
    split: Optional[float] = None
    left: Optional["Node"] = None
    right: Optional["Node"] = None

    @property
    def external(self) -> bool:
        return self.left is None or self.right is None or self.feature is None or self.split is None


class IsolationTree:
    def __init__(self, height_limit: int, rng: np.random.Generator):
        self.height_limit = height_limit
        self.rng = rng
        self.root: Optional[Node] = None

    def fit(self, x: np.ndarray) -> None:
        self.root = self._fit(x, 0)

    def _fit(self, x: np.ndarray, depth: int) -> Node:
        if depth >= self.height_limit or len(x) <= 1:
            return Node(size=len(x))

        mins = x.min(axis=0)
        maxs = x.max(axis=0)
        valid_features = np.where(maxs > mins)[0]

        if len(valid_features) == 0:
            return Node(size=len(x))

        feature = int(self.rng.choice(valid_features))
        split = float(self.rng.uniform(mins[feature], maxs[feature]))

        left_mask = x[:, feature] < split
        right_mask = ~left_mask

        if left_mask.sum() == 0 or right_mask.sum() == 0:
            return Node(size=len(x))

        left = self._fit(x[left_mask], depth + 1)
        right = self._fit(x[right_mask], depth + 1)
        return Node(size=len(x), feature=feature, split=split, left=left, right=right)

    def path_length(self, row: np.ndarray) -> float:
        return self._path_length(self.root, row, 0)

    def _path_length(self, node: Optional[Node], row: np.ndarray, depth: int) -> float:
        if node is None:
            return float(depth)

        if node.external:
            return depth + c_factor(node.size)

        if row[node.feature] < node.split:
            return self._path_length(node.left, row, depth + 1)

        return self._path_length(node.right, row, depth + 1)


class IsolationForest:
    def __init__(self, n_trees: int, sample_size: int, contamination: float, seed: int = 42):
        self.n_trees = max(16, n_trees)
        self.sample_size = max(4, sample_size)
        self.contamination = min(max(contamination, 0.01), 0.49)
        self.seed = seed
        self.trees: List[IsolationTree] = []
        self.actual_sample_size = 0
        self.threshold = 0.0
        self.rng = np.random.default_rng(seed)

    def fit(self, x: np.ndarray) -> None:
        if len(x) == 0:
            self.trees = []
            self.threshold = 1.0
            return

        self.actual_sample_size = min(self.sample_size, len(x))
        height_limit = math.ceil(math.log2(self.actual_sample_size))
        self.trees = []

        for _ in range(self.n_trees):
            if len(x) <= self.actual_sample_size:
                sample = x
            else:
                indices = self.rng.choice(len(x), size=self.actual_sample_size, replace=False)
                sample = x[indices]

            tree = IsolationTree(height_limit, self.rng)
            tree.fit(sample)
            self.trees.append(tree)

        train_scores = self.score_samples(x)
        self.threshold = float(np.quantile(train_scores, 1.0 - self.contamination))

    def score_samples(self, x: np.ndarray) -> np.ndarray:
        if len(self.trees) == 0:
            return np.zeros(len(x))

        cn = c_factor(self.actual_sample_size)
        if cn <= 0:
            return np.zeros(len(x))

        scores = []
        for row in x:
            path_lengths = [tree.path_length(row) for tree in self.trees]
            avg_path = float(np.mean(path_lengths))
            score = 2.0 ** (-avg_path / cn)
            scores.append(score)
        return np.array(scores)


def build_features(weekly_counts: List[int]) -> np.ndarray:
    rows = []
    for index, count in enumerate(weekly_counts):
        previous = weekly_counts[index - 1] if index > 0 else count
        history = weekly_counts[max(0, index - 4):index]
        history_mean = float(np.mean(history)) if history else float(count)
        history_std = float(np.std(history)) if history else 0.0
        ratio = float(count / max(history_mean, 1.0))
        trend = float(count - previous)
        rows.append([float(count), float(previous), history_mean, history_std, ratio, trend])
    return np.array(rows, dtype=float)


def analyze_specialty(item: dict, config: dict) -> dict:
    weekly_counts = item.get("WeeklyCounts", [])
    minimum_history_weeks = config["MinimumHistoryWeeks"]
    surge_multiplier = config["SurgeMultiplier"]
    minimum_current_week_cases = config["MinimumCurrentWeekCases"]

    if len(weekly_counts) < minimum_history_weeks + 1:
        return {
            "SpecialtyId": item["SpecialtyId"],
            "SpecialtyName": item["SpecialtyName"],
            "CurrentWeekCases": weekly_counts[-1] if weekly_counts else 0,
            "BaselineCases": 0.0,
            "AnomalyScore": 0.0,
            "IsAnomaly": False,
            "CurrentDoctors": item.get("CurrentDoctors", 0),
            "SuggestedExtraDoctors": 0,
            "MaxPatientsPerWeek": item.get("MaxPatientsPerWeek", 100),
            "Reason": "Khong du lich su de huan luyen mo hinh."
        }

    features = build_features(weekly_counts)
    historical = features[:-1]
    current = features[-1:]
    historical_counts = weekly_counts[:-1]
    current_week_cases = int(weekly_counts[-1])
    baseline_cases = float(np.mean(historical_counts[-4:])) if historical_counts else 0.0

    forest = IsolationForest(
        n_trees=config["TreeCount"],
        sample_size=config["SampleSize"],
        contamination=config["Contamination"]
    )
    forest.fit(historical)

    current_score = float(forest.score_samples(current)[0])
    threshold = float(forest.threshold)
    passes_volume_rule = current_week_cases >= minimum_current_week_cases
    passes_surge_rule = current_week_cases > max(baseline_cases * surge_multiplier, baseline_cases + 1)
    is_anomaly = bool(current_score >= threshold and passes_volume_rule and passes_surge_rule)

    if is_anomaly:
        reason = (
            f"Tuan hien tai co {current_week_cases} ca, vuot baseline {baseline_cases:.2f} "
            f"va diem anomaly {current_score:.3f} >= nguong {threshold:.3f}."
        )
    else:
        reason = (
            f"Diem anomaly {current_score:.3f}, nguong {threshold:.3f}, "
            f"baseline {baseline_cases:.2f}, current {current_week_cases}."
        )

    return {
        "SpecialtyId": item["SpecialtyId"],
        "SpecialtyName": item["SpecialtyName"],
        "CurrentWeekCases": current_week_cases,
        "BaselineCases": baseline_cases,
        "AnomalyScore": current_score,
        "IsAnomaly": is_anomaly,
        "CurrentDoctors": item.get("CurrentDoctors", 0),
        "SuggestedExtraDoctors": 0,
        "MaxPatientsPerWeek": item.get("MaxPatientsPerWeek", 100),
        "Reason": reason
    }


def main() -> int:
    payload_text = sys.stdin.read()
    if not payload_text.strip():
        print(json.dumps({"Alerts": []}))
        return 0

    payload = json.loads(payload_text)
    alerts = []

    for specialty in payload.get("Specialties", []):
        alerts.append(analyze_specialty(specialty, payload))

    print(json.dumps({"Alerts": alerts}, ensure_ascii=False))
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
