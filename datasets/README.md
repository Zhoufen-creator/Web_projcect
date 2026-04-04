# Dataset Workspace

Thu muc `datasets/` dung de quan ly du lieu va pipeline PhoBERT cho bai toan:
- `mo ta trieu chung -> chuyen khoa`

## 1. File nao THUC SU can cho mo hinh
Nhung file quan trong nhat:
- `symptom_specialty_seed.csv`
  Seed dataset noi bo.
- `symptom_specialty_sourced.csv`
  Du lieu co tham chieu nguon uy tin.
- `symptom_specialty_combined_v1.csv`
  Tap gop de chia train/validation/test.
- `specialty_targets.csv`
  Danh sach nhan chuyen khoa.
- `label_mapping_v1.json`
  Map nhan sang `label_id`.
- `splits/train.csv`
- `splits/validation.csv`
- `splits/test.csv`
- `processed_phobert_v1/`
  Du lieu da tien xu ly de train/infer.

Script chinh:
- `preprocess_phobert_dataset.py`
- `train_phobert_classifier.py`
- `predict_phobert_specialty.py`
- `benchmark_specialty_models.py`
- `phobert_api.py`

## 2. File nao chi de ho tro / bao cao
Nhung file sau KHONG bat buoc de mo hinh chay, nhung huu ich cho nhom va bao cao:
- `dataset_template.csv`
- `disease_to_specialty_seed.csv`
- `inference_samples.jsonl`
- `benchmarks/`

Tai lieu markdown da duoc gom gon thanh 2 file:
- `dataset_notes.md`
  Ghi chu ve nguon du lieu, provenance, translation, split.
- `phobert_workflow.md`
  Huong dan preprocess, train, infer, benchmark, API.

## 3. Ket luan ngan
- Khong can giu qua nhieu file `.md` rieng le.
- De mo hinh chay, chu yeu can du lieu CSV/JSON + cac script Python + phan tich hop C#.
- Cac file `.md` chi la tai lieu noi bo, khong anh huong truc tiep den runtime.
