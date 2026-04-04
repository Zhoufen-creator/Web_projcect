import argparse
import json
from pathlib import Path

from predict_phobert_specialty import load_label_map, predict_text


ROOT = Path(__file__).resolve().parent
TEST_PATH = ROOT / "processed_phobert_v1" / "test.jsonl"
DEFAULT_MODEL_DIR = ROOT / "phobert_runs" / "run_v1" / "best_model"
DEFAULT_LABEL_MAP = ROOT / "processed_phobert_v1" / "label_mapping.json"
DEFAULT_OUTPUT = ROOT / "benchmarks" / "benchmark_v1.json"


SPECIALTY_KEYWORDS = {
    "Noi tong quat": ["sot", "ho", "dau hong", "cam", "met moi", "nhuc dau", "kho tho", "viem", "nong", "lanh"],
    "Tieu hoa": ["dau bung", "tieu chay", "tao bon", "non", "oi", "da day", "kho tieu", "day hoi", "buon non"],
    "Tim mach": ["dau nguc", "hoi hop", "tim dap nhanh", "kho tho", "cao huyet ap", "tuc nguc", "met tim"],
    "Da lieu": ["ngua", "noi man", "di ung", "da", "mun", "phat ban", "man do", "viem da", "nam da"],
    "Mat": ["mo mat", "do mat", "ngua mat", "cay mat", "dau mat", "chay nuoc mat", "nhin mo"],
    "Tai mui hong": ["dau hong", "so mui", "nghet mui", "ho", "viem hong", "tai", "mui", "hong", "u tai"],
    "Xuong khop": ["dau lung", "dau goi", "dau khop", "nhuc xuong", "te tay", "te chan", "cot song", "vai gay"],
}


def load_jsonl(path: Path) -> list[dict]:
    rows = []
    with path.open("r", encoding="utf-8") as f:
        for line in f:
            line = line.strip()
            if line:
                rows.append(json.loads(line))
    return rows


def safe_div(numerator: float, denominator: float) -> float:
    return numerator / denominator if denominator else 0.0


def compute_metrics(y_true: list[str], y_pred: list[str]) -> dict:
    labels = sorted(set(y_true) | set(y_pred))
    correct = sum(1 for truth, pred in zip(y_true, y_pred) if truth == pred)
    accuracy = safe_div(correct, len(y_true))

    macro_precision = 0.0
    macro_recall = 0.0
    macro_f1 = 0.0
    per_label = {}

    for label in labels:
        tp = sum(1 for truth, pred in zip(y_true, y_pred) if truth == label and pred == label)
        fp = sum(1 for truth, pred in zip(y_true, y_pred) if truth != label and pred == label)
        fn = sum(1 for truth, pred in zip(y_true, y_pred) if truth == label and pred != label)

        precision = safe_div(tp, tp + fp)
        recall = safe_div(tp, tp + fn)
        f1 = safe_div(2 * precision * recall, precision + recall)

        macro_precision += precision
        macro_recall += recall
        macro_f1 += f1
        per_label[label] = {
            "precision": round(precision, 6),
            "recall": round(recall, 6),
            "f1": round(f1, 6),
            "support": sum(1 for truth in y_true if truth == label),
        }

    label_count = len(labels) or 1
    return {
        "accuracy": round(accuracy, 6),
        "macro_precision": round(macro_precision / label_count, 6),
        "macro_recall": round(macro_recall / label_count, 6),
        "macro_f1": round(macro_f1 / label_count, 6),
        "per_label": per_label,
    }


def normalize_text(text: str) -> str:
    return " ".join((text or "").lower().strip().split())


def rule_based_predict(text: str) -> dict:
    normalized = normalize_text(text)
    best_specialty = "Noi tong quat"
    best_score = 0
    best_keywords = []

    for specialty, keywords in SPECIALTY_KEYWORDS.items():
        matched = sorted({keyword for keyword in keywords if keyword in normalized})
        score = len(matched)
        if score > best_score:
            best_specialty = specialty
            best_score = score
            best_keywords = matched

    return {
        "predicted_label": best_specialty,
        "match_score": best_score,
        "matched_keywords": best_keywords,
    }


def parse_args() -> argparse.Namespace:
    parser = argparse.ArgumentParser(description="Benchmark rule-based and PhoBERT specialty prediction.")
    parser.add_argument("--test-file", default=str(TEST_PATH), help="Path to evaluation JSONL file.")
    parser.add_argument("--output-file", default=str(DEFAULT_OUTPUT), help="Path to benchmark output JSON file.")
    parser.add_argument("--compare-phobert", action="store_true", help="Also evaluate a trained PhoBERT model.")
    parser.add_argument("--model-dir", default=str(DEFAULT_MODEL_DIR), help="Path to trained PhoBERT model.")
    parser.add_argument("--label-map", default=str(DEFAULT_LABEL_MAP), help="Path to label mapping JSON.")
    parser.add_argument("--top-k", type=int, default=3, help="Top-k predictions for PhoBERT details.")
    parser.add_argument("--max-length", type=int, default=128, help="Maximum token length for PhoBERT.")
    parser.add_argument("--local-files-only", action="store_true", help="Only load PhoBERT from local files.")
    return parser.parse_args()


def benchmark_rule_based(test_rows: list[dict]) -> tuple[dict, list[dict]]:
    y_true = []
    y_pred = []
    details = []

    for row in test_rows:
        result = rule_based_predict(row["text"])
        truth = row["label"]
        pred = result["predicted_label"]
        y_true.append(truth)
        y_pred.append(pred)
        details.append(
            {
                "combined_id": row["combined_id"],
                "text": row["text"],
                "true_label": truth,
                "predicted_label": pred,
                "correct": truth == pred,
                "match_score": result["match_score"],
                "matched_keywords": result["matched_keywords"],
            }
        )

    return compute_metrics(y_true, y_pred), details


def benchmark_phobert(args: argparse.Namespace, test_rows: list[dict]) -> tuple[dict, list[dict]]:
    from transformers import AutoModelForSequenceClassification, AutoTokenizer

    id2label, _ = load_label_map(Path(args.label_map))
    tokenizer = AutoTokenizer.from_pretrained(
        args.model_dir,
        local_files_only=args.local_files_only,
    )
    model = AutoModelForSequenceClassification.from_pretrained(
        args.model_dir,
        local_files_only=args.local_files_only,
    )
    model.eval()

    y_true = []
    y_pred = []
    details = []

    for row in test_rows:
        result = predict_text(
            row["text"],
            tokenizer=tokenizer,
            model=model,
            id2label=id2label,
            max_length=args.max_length,
            top_k=args.top_k,
        )
        truth = row["label"]
        pred = result["predicted_label"]
        y_true.append(truth)
        y_pred.append(pred)
        details.append(
            {
                "combined_id": row["combined_id"],
                "text": row["text"],
                "true_label": truth,
                "predicted_label": pred,
                "correct": truth == pred,
                "confidence": result["confidence"],
                "top_k": result["top_k"],
            }
        )

    return compute_metrics(y_true, y_pred), details


def main() -> None:
    args = parse_args()
    test_rows = load_jsonl(Path(args.test_file))

    benchmark = {
        "test_file": args.test_file,
        "sample_count": len(test_rows),
        "rule_based": {},
    }

    rule_metrics, rule_details = benchmark_rule_based(test_rows)
    benchmark["rule_based"]["metrics"] = rule_metrics
    benchmark["rule_based"]["predictions"] = rule_details

    if args.compare_phobert:
        try:
            phobert_metrics, phobert_details = benchmark_phobert(args, test_rows)
            benchmark["phobert"] = {
                "metrics": phobert_metrics,
                "predictions": phobert_details,
            }
        except OSError as exc:
            benchmark["phobert"] = {
                "error": str(exc),
                "message": "Khong the load PhoBERT model de benchmark.",
            }

    output_path = Path(args.output_file)
    output_path.parent.mkdir(parents=True, exist_ok=True)
    with output_path.open("w", encoding="utf-8") as f:
        json.dump(benchmark, f, ensure_ascii=False, indent=2)

    summary = {
        "sample_count": benchmark["sample_count"],
        "rule_based": benchmark["rule_based"]["metrics"],
    }
    if "phobert" in benchmark and "metrics" in benchmark["phobert"]:
        summary["phobert"] = benchmark["phobert"]["metrics"]

    print(json.dumps(summary, ensure_ascii=False, indent=2))


if __name__ == "__main__":
    main()
