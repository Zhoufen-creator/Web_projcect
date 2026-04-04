import argparse
import json
from pathlib import Path

import torch
from transformers import AutoModelForSequenceClassification, AutoTokenizer


ROOT = Path(__file__).resolve().parent
DEFAULT_MODEL_DIR = ROOT / "phobert_runs" / "run_v1" / "best_model"
DEFAULT_LABEL_MAP = ROOT / "processed_phobert_v1" / "label_mapping.json"


def load_label_map(path: Path) -> tuple[dict[int, str], dict[str, int]]:
    with path.open("r", encoding="utf-8") as f:
        raw = json.load(f)
    label2id = {label: int(idx) for label, idx in raw.items()}
    id2label = {idx: label for label, idx in label2id.items()}
    return id2label, label2id


def normalize_whitespace(text: str) -> str:
    return " ".join((text or "").strip().split())


def predict_text(text: str, tokenizer, model, id2label: dict[int, str], max_length: int, top_k: int) -> dict:
    clean_text = normalize_whitespace(text)
    if not clean_text:
        raise ValueError("Input text is empty.")

    encoded = tokenizer(
        clean_text,
        truncation=True,
        padding=True,
        max_length=max_length,
        return_tensors="pt",
    )

    with torch.no_grad():
        outputs = model(**encoded)
        probabilities = torch.softmax(outputs.logits, dim=-1).squeeze(0)

    top_k = min(top_k, probabilities.numel())
    top_values, top_indices = torch.topk(probabilities, k=top_k)

    predictions = []
    for prob, idx in zip(top_values.tolist(), top_indices.tolist()):
        predictions.append(
            {
                "label_id": idx,
                "label": id2label[idx],
                "probability": round(prob, 6),
            }
        )

    return {
        "input_text": clean_text,
        "predicted_label": predictions[0]["label"],
        "predicted_label_id": predictions[0]["label_id"],
        "confidence": predictions[0]["probability"],
        "top_k": predictions,
    }


def parse_args() -> argparse.Namespace:
    parser = argparse.ArgumentParser(description="Predict specialty using a fine-tuned PhoBERT model.")
    parser.add_argument("--model-dir", default=str(DEFAULT_MODEL_DIR), help="Path to trained model directory.")
    parser.add_argument("--label-map", default=str(DEFAULT_LABEL_MAP), help="Path to label mapping JSON file.")
    parser.add_argument("--text", help="Single symptom description to predict.")
    parser.add_argument("--input-file", help="Optional text/JSONL file with multiple inputs.")
    parser.add_argument("--output-file", help="Optional file to save JSONL predictions.")
    parser.add_argument("--max-length", type=int, default=128, help="Maximum token length.")
    parser.add_argument("--top-k", type=int, default=3, help="Number of top predictions to return.")
    parser.add_argument(
        "--local-files-only",
        action="store_true",
        help="Only load tokenizer/model from local files.",
    )
    return parser.parse_args()


def load_inputs(args: argparse.Namespace) -> list[str]:
    inputs = []
    if args.text:
        inputs.append(args.text)

    if args.input_file:
        input_path = Path(args.input_file)
        if input_path.suffix.lower() == ".jsonl":
            with input_path.open("r", encoding="utf-8") as f:
                for line in f:
                    line = line.strip()
                    if not line:
                        continue
                    row = json.loads(line)
                    inputs.append(row.get("text", ""))
        else:
            with input_path.open("r", encoding="utf-8") as f:
                for line in f:
                    line = line.strip()
                    if line:
                        inputs.append(line)

    if not inputs:
        raise SystemExit("Can truyen --text hoac --input-file de du doan.")

    return inputs


def main() -> None:
    args = parse_args()
    model_dir = Path(args.model_dir)
    label_map_path = Path(args.label_map)

    id2label, _ = load_label_map(label_map_path)

    try:
        tokenizer = AutoTokenizer.from_pretrained(
            str(model_dir),
            local_files_only=args.local_files_only,
        )
        model = AutoModelForSequenceClassification.from_pretrained(
            str(model_dir),
            local_files_only=args.local_files_only,
        )
    except OSError as exc:
        message = [
            "Khong the load model/tokenizer de suy luan.",
            f"model-dir hien tai: {model_dir}",
            "Hay dam bao thu muc model da ton tai, vi du datasets/phobert_runs/run_v1/best_model",
            "Hoac tro den mot duong dan model local khac bang --model-dir",
            f"Chi tiet loi goc: {exc}",
        ]
        raise SystemExit("\n".join(message)) from exc

    model.eval()
    inputs = load_inputs(args)

    predictions = [
        predict_text(text, tokenizer, model, id2label, args.max_length, args.top_k)
        for text in inputs
    ]

    if args.output_file:
        output_path = Path(args.output_file)
        output_path.parent.mkdir(parents=True, exist_ok=True)
        with output_path.open("w", encoding="utf-8") as f:
            for row in predictions:
                f.write(json.dumps(row, ensure_ascii=False) + "\n")

    print(json.dumps(predictions, ensure_ascii=False, indent=2))


if __name__ == "__main__":
    main()
