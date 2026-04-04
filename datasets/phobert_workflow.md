# PhoBERT Workflow

## 1. Tien xu ly
Script:
- `preprocess_phobert_dataset.py`

Dau vao:
- `splits/train.csv`
- `splits/validation.csv`
- `splits/test.csv`

Dau ra:
- `processed_phobert_v1/train.jsonl`
- `processed_phobert_v1/validation.jsonl`
- `processed_phobert_v1/test.jsonl`
- `processed_phobert_v1/label_mapping.json`
- `processed_phobert_v1/summary.json`

## 2. Train
Script:
- `train_phobert_classifier.py`

Lenh co ban:
```powershell
.\.env3.11\Scripts\python.exe datasets\train_phobert_classifier.py
```

Neu offline:
- dat model local
- dung `--model-name <duong_dan_local>`
- them `--local-files-only`

## 3. Infer
Script:
- `predict_phobert_specialty.py`

Co the:
- predict 1 cau qua `--text`
- predict nhieu cau qua `--input-file`

File mau:
- `inference_samples.jsonl`

## 4. Benchmark
Script:
- `benchmark_specialty_models.py`

Dung de:
- benchmark rule-based hien tai
- so sanh voi PhoBERT sau khi co model

Output mac dinh:
- `benchmarks/benchmark_v1.json`

## 5. HTTP API
Script:
- `phobert_api.py`
- `start_phobert_api.ps1`

Endpoint:
- `GET /health`
- `POST /predict`
- `POST /predict-batch`

Dung khi:
- ASP.NET Core can goi model qua HTTP

Lenh chay nhanh offline bang model da train:
```powershell
powershell -ExecutionPolicy Bypass -File datasets\start_phobert_api.ps1
```

## 6. Local model
Neu may khong co internet va khong co cache Hugging Face:
- dat model vao mot thu muc local
- truyen duong dan qua `--model-name` hoac `--model-dir`
- bat `--local-files-only`

## 7. Tich hop voi ASP.NET Core
Phia C# da co:
- `PhoBertInferenceService`
- `PhoBertApiOptions`
- fallback tu PhoBERT sang rule-based
- trang admin test tai:
  - `/Admin/PhoBertDiagnostics`
