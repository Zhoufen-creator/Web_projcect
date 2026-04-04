# Ghi chu du lieu

## 1. Nguon du lieu hien tai
Bo du lieu hien tai gom 2 nhom:
- `symptom_specialty_seed.csv`
  Du lieu seed noi bo, tao tu rule keywords trong project.
- `symptom_specialty_sourced.csv`
  Du lieu tham chieu co `source_url`, hien dang dua tren MedlinePlus.

## 2. Provenance can ghi ro trong bao cao
### Seed dataset
- Khong phai dataset hoc thuat goc.
- Duoc tao bang cach:
  1. lay keyword tu `SpecialtyPredictionService`
  2. gom theo khoa
  3. viet lai thanh cau benh nhan
  4. gan nhan khoa

### Sourced dataset
- Trieu chung/chu de tham chieu tu nguon uy tin.
- Nhieu dong hien tai dua tren MedlinePlus.
- Nhan chuyen khoa la phan map bo sung theo taxonomy cua project.

## 3. Translation / chuan hoa
Khi dich du lieu sang tieng Viet, nen theo 3 buoc:
1. Dich tho noi dung
2. Chuan hoa thuat ngu
3. Viet lai theo van phong benh nhan Viet Nam

Nguyen tac:
- Giu `source_text` neu co
- Ghi ro `source_name`, `source_url`
- Danh dau `review_status`

## 4. Cach chia tap du lieu
Bo du lieu v1 da duoc chia thanh:
- `splits/train.csv`
- `splits/validation.csv`
- `splits/test.csv`

Y tuong:
- chia theo `specialty_name_vi`
- khong chia random toan cuc
- co giu mot phan du lieu `sourced` trong validation/test

## 5. Nguon uy tin da dinh huong dung
- MedlinePlus
- bai bao BMC 2023 ve specialty classification
- MIMIC-IV-ED
- MIETIC
- HPO
- SNOMED / ICD mapping

## 6. File nao co the dua vao bao cao
Neu can viet bao cao, co the trich:
- nguon du lieu gom seed + sourced
- seed la weak-labeled internal data
- sourced la du lieu tham chieu nguon uy tin
- bo du lieu da duoc chia train/validation/test de thu nghiem PhoBERT
