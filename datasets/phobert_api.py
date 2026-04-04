import argparse
import json
from pathlib import Path

from flask import Flask, jsonify, request
from transformers import AutoModelForSequenceClassification, AutoTokenizer

from predict_phobert_specialty import load_label_map, predict_text


ROOT = Path(__file__).resolve().parent
DEFAULT_MODEL_DIR = ROOT / "phobert_runs" / "run_v1" / "best_model"
DEFAULT_LABEL_MAP = ROOT / "processed_phobert_v1" / "label_mapping.json"


def create_app(model_dir: Path, label_map_path: Path, max_length: int, top_k: int, local_files_only: bool) -> Flask:
    app = Flask(__name__)
    app.config["MODEL_DIR"] = str(model_dir)
    app.config["LABEL_MAP_PATH"] = str(label_map_path)
    app.config["MAX_LENGTH"] = max_length
    app.config["TOP_K"] = top_k

    id2label, _ = load_label_map(label_map_path)

    try:
        tokenizer = AutoTokenizer.from_pretrained(
            str(model_dir),
            local_files_only=local_files_only,
        )
        model = AutoModelForSequenceClassification.from_pretrained(
            str(model_dir),
            local_files_only=local_files_only,
        )
        model.eval()
    except OSError as exc:
        raise SystemExit(
            "\n".join(
                [
                    "Khong the khoi tao PhoBERT API.",
                    f"model-dir hien tai: {model_dir}",
                    f"label-map hien tai: {label_map_path}",
                    "Hay dam bao da co best_model local hoac model local hop le.",
                    f"Chi tiet loi goc: {exc}",
                ]
            )
        ) from exc

    app.config["TOKENIZER"] = tokenizer
    app.config["MODEL"] = model
    app.config["ID2LABEL"] = id2label

    @app.get("/health")
    def health():
        return jsonify(
            {
                "status": "ok",
                "model_dir": app.config["MODEL_DIR"],
                "label_map_path": app.config["LABEL_MAP_PATH"],
                "max_length": app.config["MAX_LENGTH"],
                "top_k": app.config["TOP_K"],
            }
        )

    @app.post("/predict")
    def predict():
        payload = request.get_json(silent=True) or {}
        text = payload.get("text", "")
        req_top_k = int(payload.get("top_k", app.config["TOP_K"]))

        if not str(text).strip():
            return jsonify({"error": "Truong 'text' khong duoc de trong."}), 400

        try:
            result = predict_text(
                text=str(text),
                tokenizer=app.config["TOKENIZER"],
                model=app.config["MODEL"],
                id2label=app.config["ID2LABEL"],
                max_length=app.config["MAX_LENGTH"],
                top_k=req_top_k,
            )
        except Exception as exc:
            return jsonify({"error": f"Khong the du doan: {exc}"}), 500

        return jsonify(result)

    @app.post("/predict-batch")
    def predict_batch():
        payload = request.get_json(silent=True) or {}
        texts = payload.get("texts", [])
        req_top_k = int(payload.get("top_k", app.config["TOP_K"]))

        if not isinstance(texts, list) or not texts:
            return jsonify({"error": "Truong 'texts' phai la list khong rong."}), 400

        results = []
        for idx, text in enumerate(texts):
            if not str(text).strip():
                results.append({"index": idx, "error": "Text rong."})
                continue

            try:
                prediction = predict_text(
                    text=str(text),
                    tokenizer=app.config["TOKENIZER"],
                    model=app.config["MODEL"],
                    id2label=app.config["ID2LABEL"],
                    max_length=app.config["MAX_LENGTH"],
                    top_k=req_top_k,
                )
                prediction["index"] = idx
                results.append(prediction)
            except Exception as exc:
                results.append({"index": idx, "error": str(exc)})

        return jsonify({"count": len(results), "results": results})

    return app


def parse_args() -> argparse.Namespace:
    parser = argparse.ArgumentParser(description="Run PhoBERT inference API.")
    parser.add_argument("--model-dir", default=str(DEFAULT_MODEL_DIR), help="Path to trained model directory.")
    parser.add_argument("--label-map", default=str(DEFAULT_LABEL_MAP), help="Path to label mapping JSON.")
    parser.add_argument("--host", default="127.0.0.1", help="Host to bind the API server.")
    parser.add_argument("--port", type=int, default=5000, help="Port to bind the API server.")
    parser.add_argument("--max-length", type=int, default=128, help="Maximum token length.")
    parser.add_argument("--top-k", type=int, default=3, help="Default top-k predictions to return.")
    parser.add_argument("--debug", action="store_true", help="Run Flask in debug mode.")
    parser.add_argument("--local-files-only", action="store_true", help="Only load local model/tokenizer files.")
    return parser.parse_args()


def main() -> None:
    args = parse_args()
    app = create_app(
        model_dir=Path(args.model_dir),
        label_map_path=Path(args.label_map),
        max_length=args.max_length,
        top_k=args.top_k,
        local_files_only=args.local_files_only,
    )
    app.run(host=args.host, port=args.port, debug=args.debug)


if __name__ == "__main__":
    main()
