import argparse
import json
from pathlib import Path

import torch
from transformers import (
    AutoModelForSequenceClassification,
    AutoTokenizer,
    Trainer,
    TrainingArguments,
)


ROOT = Path(__file__).resolve().parent
PROCESSED_DIR = ROOT / "processed_phobert_v1"
DEFAULT_MODEL_NAME = "vinai/phobert-base"


def load_jsonl(path: Path) -> list[dict]:
    records = []
    with path.open("r", encoding="utf-8") as f:
        for line in f:
            line = line.strip()
            if not line:
                continue
            records.append(json.loads(line))
    return records


class PhoBertTextDataset(torch.utils.data.Dataset):
    def __init__(self, records: list[dict], tokenizer, max_length: int):
        self.records = records
        self.tokenizer = tokenizer
        self.max_length = max_length

    def __len__(self) -> int:
        return len(self.records)

    def __getitem__(self, idx: int) -> dict:
        row = self.records[idx]
        encoded = self.tokenizer(
            row["text"],
            truncation=True,
            padding="max_length",
            max_length=self.max_length,
            return_tensors="pt",
        )

        item = {key: value.squeeze(0) for key, value in encoded.items()}
        item["labels"] = torch.tensor(row["label_id"], dtype=torch.long)
        return item


def safe_div(numerator: float, denominator: float) -> float:
    return numerator / denominator if denominator else 0.0


def compute_metrics(eval_pred) -> dict:
    logits, labels = eval_pred
    predictions = logits.argmax(axis=-1)

    total = len(labels)
    correct = int((predictions == labels).sum())
    accuracy = safe_div(correct, total)

    unique_labels = sorted(set(labels.tolist()) | set(predictions.tolist()))
    macro_precision = 0.0
    macro_recall = 0.0
    macro_f1 = 0.0

    for label_id in unique_labels:
        tp = int(((predictions == label_id) & (labels == label_id)).sum())
        fp = int(((predictions == label_id) & (labels != label_id)).sum())
        fn = int(((predictions != label_id) & (labels == label_id)).sum())

        precision = safe_div(tp, tp + fp)
        recall = safe_div(tp, tp + fn)
        f1 = safe_div(2 * precision * recall, precision + recall)

        macro_precision += precision
        macro_recall += recall
        macro_f1 += f1

    label_count = len(unique_labels) or 1
    macro_precision = macro_precision / label_count
    macro_recall = macro_recall / label_count
    macro_f1 = macro_f1 / label_count

    return {
        "accuracy": accuracy,
        "macro_precision": macro_precision,
        "macro_recall": macro_recall,
        "macro_f1": macro_f1,
    }


def build_label_mappings(label_mapping: dict) -> tuple[dict, dict]:
    id2label = {int(v): k for k, v in label_mapping.items()}
    label2id = {k: int(v) for k, v in label_mapping.items()}
    return id2label, label2id


def parse_args() -> argparse.Namespace:
    parser = argparse.ArgumentParser(description="Fine-tune PhoBERT for specialty classification.")
    parser.add_argument("--model-name", default=DEFAULT_MODEL_NAME, help="Hugging Face model name or local path.")
    parser.add_argument("--data-dir", default=str(PROCESSED_DIR), help="Directory containing processed JSONL files.")
    parser.add_argument("--output-dir", default=str(ROOT / "phobert_runs" / "run_v1"), help="Training output directory.")
    parser.add_argument("--max-length", type=int, default=128, help="Maximum token length.")
    parser.add_argument("--epochs", type=int, default=4, help="Number of training epochs.")
    parser.add_argument("--train-batch-size", type=int, default=8, help="Training batch size per device.")
    parser.add_argument("--eval-batch-size", type=int, default=8, help="Evaluation batch size per device.")
    parser.add_argument("--learning-rate", type=float, default=2e-5, help="Learning rate.")
    parser.add_argument("--weight-decay", type=float, default=0.01, help="Weight decay.")
    parser.add_argument("--warmup-ratio", type=float, default=0.1, help="Warmup ratio.")
    parser.add_argument("--seed", type=int, default=42, help="Random seed.")
    parser.add_argument(
        "--local-files-only",
        action="store_true",
        help="Only load model/tokenizer from local files or local Hugging Face cache.",
    )
    return parser.parse_args()


def main() -> None:
    args = parse_args()
    data_dir = Path(args.data_dir)
    output_dir = Path(args.output_dir)
    output_dir.mkdir(parents=True, exist_ok=True)

    with (data_dir / "label_mapping.json").open("r", encoding="utf-8") as f:
        label_mapping = json.load(f)

    train_records = load_jsonl(data_dir / "train.jsonl")
    validation_records = load_jsonl(data_dir / "validation.jsonl")
    test_records = load_jsonl(data_dir / "test.jsonl")

    id2label, label2id = build_label_mappings(label_mapping)

    try:
        tokenizer = AutoTokenizer.from_pretrained(
            args.model_name,
            local_files_only=args.local_files_only,
        )
        model = AutoModelForSequenceClassification.from_pretrained(
            args.model_name,
            num_labels=len(label_mapping),
            id2label=id2label,
            label2id=label2id,
            local_files_only=args.local_files_only,
        )
    except OSError as exc:
        message = [
            "Khong the tai PhoBERT model/tokenizer.",
            f"model-name hien tai: {args.model_name}",
            "Neu dang o moi truong offline, hay:",
            "1. dat model PhoBERT vao mot thu muc local",
            "2. truyen duong dan do qua --model-name",
            "3. them --local-files-only de chi dung file local",
            f"Chi tiet loi goc: {exc}",
        ]
        raise SystemExit("\n".join(message)) from exc

    train_dataset = PhoBertTextDataset(train_records, tokenizer, args.max_length)
    validation_dataset = PhoBertTextDataset(validation_records, tokenizer, args.max_length)
    test_dataset = PhoBertTextDataset(test_records, tokenizer, args.max_length)

    training_args = TrainingArguments(
        output_dir=str(output_dir),
        num_train_epochs=args.epochs,
        per_device_train_batch_size=args.train_batch_size,
        per_device_eval_batch_size=args.eval_batch_size,
        learning_rate=args.learning_rate,
        weight_decay=args.weight_decay,
        warmup_ratio=args.warmup_ratio,
        eval_strategy="epoch",
        save_strategy="epoch",
        logging_strategy="epoch",
        load_best_model_at_end=True,
        metric_for_best_model="macro_f1",
        greater_is_better=True,
        save_total_limit=2,
        seed=args.seed,
        report_to="none",
        use_cpu=not torch.cuda.is_available(),
    )

    trainer = Trainer(
        model=model,
        args=training_args,
        train_dataset=train_dataset,
        eval_dataset=validation_dataset,
        processing_class=tokenizer,
        compute_metrics=compute_metrics,
    )

    trainer.train()

    validation_metrics = trainer.evaluate(eval_dataset=validation_dataset)
    test_metrics = trainer.evaluate(eval_dataset=test_dataset)

    with (output_dir / "validation_metrics.json").open("w", encoding="utf-8") as f:
        json.dump(validation_metrics, f, ensure_ascii=False, indent=2)

    with (output_dir / "test_metrics.json").open("w", encoding="utf-8") as f:
        json.dump(test_metrics, f, ensure_ascii=False, indent=2)

    trainer.save_model(str(output_dir / "best_model"))
    tokenizer.save_pretrained(str(output_dir / "best_model"))

    print("Training completed.")
    print(json.dumps({"validation": validation_metrics, "test": test_metrics}, ensure_ascii=False, indent=2))


if __name__ == "__main__":
    main()
