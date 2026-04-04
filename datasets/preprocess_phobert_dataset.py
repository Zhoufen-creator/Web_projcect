import csv
import json
import re
from pathlib import Path


ROOT = Path(__file__).resolve().parent
SPLITS_DIR = ROOT / "splits"
OUTPUT_DIR = ROOT / "processed_phobert_v1"
LABEL_MAP_PATH = ROOT / "label_mapping_v1.json"


def normalize_whitespace(text: str) -> str:
    return re.sub(r"\s+", " ", (text or "").strip())


def choose_model_text(row: dict) -> str:
    text_vi = normalize_whitespace(row.get("text_vi", ""))
    text_ascii = normalize_whitespace(row.get("text_ascii", ""))
    return text_vi or text_ascii


def read_label_map() -> dict:
    with LABEL_MAP_PATH.open("r", encoding="utf-8") as f:
        return json.load(f)


def load_split(csv_path: Path, label_map: dict) -> list[dict]:
    records = []
    with csv_path.open("r", encoding="utf-8-sig", newline="") as f:
        reader = csv.DictReader(f)
        for row in reader:
            specialty = normalize_whitespace(row.get("specialty_name_vi", ""))
            if specialty not in label_map:
                raise ValueError(f"Unknown specialty label: {specialty}")

            model_text = choose_model_text(row)
            if not model_text:
                raise ValueError(f"Empty text in row combined_id={row.get('combined_id')}")

            records.append(
                {
                    "combined_id": int(row["combined_id"]),
                    "dataset_origin": row.get("dataset_origin", ""),
                    "source_id": row.get("id", ""),
                    "text": model_text,
                    "label": specialty,
                    "label_id": label_map[specialty],
                    "disease_name_vi": row.get("disease_name_vi", ""),
                    "source_name": row.get("source_name", ""),
                    "source_url": row.get("source_url", ""),
                    "review_status": row.get("review_status", ""),
                }
            )
    return records


def export_jsonl(records: list[dict], path: Path) -> None:
    with path.open("w", encoding="utf-8") as f:
        for record in records:
            f.write(json.dumps(record, ensure_ascii=False) + "\n")


def export_csv(records: list[dict], path: Path) -> None:
    fieldnames = [
        "combined_id",
        "dataset_origin",
        "source_id",
        "text",
        "label",
        "label_id",
        "disease_name_vi",
        "source_name",
        "source_url",
        "review_status",
    ]
    with path.open("w", encoding="utf-8-sig", newline="") as f:
        writer = csv.DictWriter(f, fieldnames=fieldnames)
        writer.writeheader()
        writer.writerows(records)


def build_summary(split_records: dict[str, list[dict]]) -> dict:
    summary = {"totals": {}, "label_distribution": {}, "origin_distribution": {}}
    for split_name, records in split_records.items():
        summary["totals"][split_name] = len(records)

        label_counts = {}
        origin_counts = {}
        for record in records:
            label = record["label"]
            origin = record["dataset_origin"]
            label_counts[label] = label_counts.get(label, 0) + 1
            origin_counts[origin] = origin_counts.get(origin, 0) + 1

        summary["label_distribution"][split_name] = label_counts
        summary["origin_distribution"][split_name] = origin_counts
    return summary


def main() -> None:
    OUTPUT_DIR.mkdir(exist_ok=True)
    label_map = read_label_map()

    split_files = {
        "train": SPLITS_DIR / "train.csv",
        "validation": SPLITS_DIR / "validation.csv",
        "test": SPLITS_DIR / "test.csv",
    }

    split_records = {}
    for split_name, split_path in split_files.items():
        records = load_split(split_path, label_map)
        split_records[split_name] = records

        export_jsonl(records, OUTPUT_DIR / f"{split_name}.jsonl")
        export_csv(records, OUTPUT_DIR / f"{split_name}.csv")

    with (OUTPUT_DIR / "label_mapping.json").open("w", encoding="utf-8") as f:
        json.dump(label_map, f, ensure_ascii=False, indent=2)

    summary = build_summary(split_records)
    with (OUTPUT_DIR / "summary.json").open("w", encoding="utf-8") as f:
        json.dump(summary, f, ensure_ascii=False, indent=2)

    print("Preprocessing completed.")
    for split_name, records in split_records.items():
        print(f"{split_name}: {len(records)} records")


if __name__ == "__main__":
    main()
