# Hướng Dẫn Tính Năng Mới - Staffing & PhoBERT

## 1. Tính Toán Nhân Sự (Staffing Service)

### Mô Tả
Hệ thống tự động tính toán số lượng bác sĩ cần trực cho mỗi chuyên khoa vào tuần tới dựa trên:
- **Trung bình động 4 tuần** (28 ngày gần nhất)
- **Định mức năng suất** (MaxPatientsPerWeek)
- **Tránh hiệu ứng Yo-yo** - chỉ tăng/giảm nếu xu hướng thực sự

### Giải Thuật
```
1. Lấy tổng bệnh nhân của chuyên khoa trong 28 ngày gần nhất
2. Tính trung bình 1 tuần = Tổng / 4
3. Tính số bác sĩ cần = Ceiling(Trung bình / MaxPatientsPerWeek)
4. Đảm bảo luôn >= 1
```

### Cách Sử Dụng

**Trong Controller:**
```csharp
public class StaffingController : Controller
{
    private readonly IStaffingService _staffingService;
    
    public StaffingController(IStaffingService staffingService)
    {
        _staffingService = staffingService;
    }
    
    public async Task<IActionResult> GetRequiredDoctors(int specialtyId)
    {
        int requiredDoctors = await _staffingService.CalculateRequiredDoctorsAsync(specialtyId);
        return Ok(new { requiredDoctors });
    }
}
```

### Database
- **Table:** Specialties
- **Cột thêm:** `MaxPatientsPerWeek` (INT, mặc định = 100)
  - Định mức bệnh nhân tối đa 1 bác sĩ có thể khám trong 1 tuần
  - Điều chỉnh giá trị này dựa trên chuyên khoa

---

## 2. Dự Đoán Chuyên Khoa với PhoBERT

### Mô Tả
Thay thế logic từ khóa cũ bằng **PhoBERT** - mô hình NLP tiếng Việt pre-trained để classification chính xác hơn.

### Setup

#### Bước 1: Cài đặt Python Dependencies
```bash
pip install flask transformers torch numpy
```

#### Bước 2: Chạy Python Flask API Server
```bash
python phobert_api.py
```
- Server sẽ chạy tại `http://localhost:5000`
- Mô hình sẽ được load tự động từ `final_model_v7/`

#### Bước 3: Kiểm Tra Connection
```bash
curl http://localhost:5000/health
```

### Cách Sử Dụng trong C#

**Dùng Async Method (Khuyến Nghị):**
```csharp
public class AppointmentController : Controller
{
    private readonly ISpecialtyPredictionService _predictionService;
    
    public AppointmentController(ISpecialtyPredictionService predictionService)
    {
        _predictionService = predictionService;
    }
    
    [HttpPost]
    public async Task<IActionResult> Create(string reasonForVisit)
    {
        var result = await _predictionService.PredictSpecialtyAsync(reasonForVisit);
        // result.PredictedSpecialty: Chuyên khoa dự đoán
        // result.MatchScore: Độ tin cậy (0-100)
        // result.Message: Thông báo chi tiết
    }
}
```

**Dùng Sync Method (For Backward Compatibility):**
```csharp
var result = _predictionService.PredictSpecialty(reasonForVisit);
```

### Kết Quả Trả Về
```json
{
  "PredictedSpecialty": "Tim mạch",
  "MatchScore": 85,
  "MatchedKeywords": [],
  "Message": "PhoBERT gợi ý chuyên khoa: Tim mạch"
}
```

### Cấu Hình
File: `appsettings.json`
```json
{
  "PhoBertApi": {
    "Url": "http://localhost:5000"
  }
}
```

### Tính Năng Fallback
Nếu:
- Python API không khả dụng
- Model load thất bại
→ Hệ thống tự động fallback về "Nội tổng quát"

---

## 3. Code Cũ - Khôi Phục

Tất cả code từ khóa cũ đã được **comment lại** trong file `SpecialtyPredictionService.cs`:
```csharp
/*
private readonly Dictionary<string, List<string>> _specialtyKeywords = new()
{
    { "Nội tổng quát", new List<string> { "sot", "ho", "dau hong", ... } },
    ...
};
*/
```

Để khôi phục logic cũ: **Uncomment** toàn bộ phần được đánh dấu.

---

## 4. Files Thêm/Sửa

### Files Tạo Mới:
- `Services/StaffingService.cs` - Tính toán nhân sự
- `Services/Interface/IStaffingService.cs` - Interface
- `Services/PhoBertInferenceService.cs` - Gọi PhoBERT API
- `Services/Interface/IPhoBertInferenceService.cs` - Interface
- `phobert_api.py` - Python API server
- `STAFFING_GUIDE.md` - (File này)

### Files Sửa:
- `Models/Specialy.cs` - Thêm `MaxPatientsPerWeek`
- `Services/SpecialtyPredictionService.cs` - Comment code cũ + dùng PhoBERT
- `Program.cs` - Đăng ký DI services
- `appsettings.json` - Thêm config PhoBertApi

### Database:
- Migration: `AddMaxPatientsPerWeekToSpecialty`
- Thêm cột `MaxPatientsPerWeek` (INT) vào bảng Specialties

---

## 5. Lưu Ý Triển Khai

### Development:
```bash
# Terminal 1: Chạy C# ASP.NET Core
dotnet watch run

# Terminal 2: Chạy Python API
python phobert_api.py
```

### Production:
- Deploy Python API trên server riêng hoặc Docker container
- Cập nhật `appsettings.json` với URL production

### Performance:
- PhoBERT inference có latency ~200-500ms
- Nên cache kết quả dự đoán
- Xem xét async/await để không block request

---

## Troubleshooting

**Lỗi: "Invalid object name 'Specialties'"**
→ Chạy migration: `dotnet ef database update`

**Lỗi: "Python API not responding"**
→ Kiểm tra Python server đang chạy: `curl http://localhost:5000/health`

**Lỗi: "Model not found"**
→ Kiểm tra đường dẫn model trong `phobert_api.py`

**PhoBERT inference quá chậm**
→ Sử dụng GPU hoặc quantized model

---

## Các Bước Tiếp Theo (Optional)

1. **Tối ưu hóa**: Thêm model quantization / ONNX export
2. **Caching**: Redis cache cho kết quả dự đoán
3. **Monitoring**: Theo dõi accuracy của predictions
4. **Retraining**: Auto-retrain model dựa trên feedback người dùng

---







# 🔄 Rollback - Loại Bỏ Các Tính Năng Mới

Nếu bạn muốn quay lại trạng thái ban đầu (trước khi thêm Staffing & PhoBERT), hãy thực hiện các bước sau:

## Bước 1: Dừng các service

```bash
# Dừng C# app
# (Ctrl+C trong terminal dotnet watch run)

# Dừng Python API
Get-Process python | Stop-Process -Force

# Xóa virtual environment (optional)
Remove-Item -Recurse .env3.11
```

## Bước 2: Xóa/Sửa Files

### 2.1 Xóa Files Thêm Mới
```bash
# Xóa Staffing Service
Remove-Item Services\StaffingService.cs
Remove-Item Services\Interface\IStaffingService.cs

# Xóa PhoBERT Service
Remove-Item Services\PhoBertInferenceService.cs
Remove-Item Services\Interface\IPhoBertInferenceService.cs

# Xóa Python API
Remove-Item phobert_api.py

# Xóa Guide
Remove-Item STAFFING_GUIDE.md
```

### 2.2 Sửa File `Program.cs`
Xóa các dòng sau từ `Program.cs`:
```csharp
// SỬA: thêm service tính toán nhân sự
builder.Services.AddScoped<IStaffingService, StaffingService>();

// SỬA: thêm service PhoBERT inference
builder.Services.AddScoped<IPhoBertInferenceService, PhoBertInferenceService>();
builder.Services.AddHttpClient<IPhoBertInferenceService, PhoBertInferenceService>();
```

Tìm kiếm và xóa, chỉ giữ lại:
```csharp
builder.Services.AddScoped<IAppointmentValidationService, AppointmentValidationService>();
builder.Services.AddScoped<IAppointmentEstimateService, AppointmentEstimateService>();
builder.Services.AddScoped<ISpecialtyPredictionService, SpecialtyPredictionService>();
builder.Services.AddScoped<IDoctorAutoAssignmentService, DoctorAutoAssignmentService>();
builder.Services.AddScoped<ISpecialtyLoadAnalysisService, SpecialtyLoadAnalysisService>();
```

### 2.3 Sửa File `SpecialtyPredictionService.cs`
**Uncomment** toàn bộ code từ khóa cũ (phần bị comment):

Thay thế nội dung file bằng:
```csharp
using System.Text;
using DoAnWeb.Services.Interface;

namespace DoAnWeb.Services
{
    public class SpecialtyPredictionService : ISpecialtyPredictionService
    {
        private readonly Dictionary<string, List<string>> _specialtyKeywords = new()
        {
            { "Nội tổng quát", new List<string> { "sot", "ho", "dau hong", "cam", "met moi", "nhuc dau", "kho tho", "viem", "nong", "lanh" } },
            { "Tiêu hóa", new List<string> { "dau bung", "tieu chay", "tao bon", "non", "oi", "da day", "kho tieu", "day hoi", "buon non" } },
            { "Tim mạch", new List<string> { "dau nguc", "hoi hop", "tim dap nhanh", "kho tho", "cao huyet ap", "tuc nguc", "met tim" } },
            { "Da liễu", new List<string> { "ngua", "noi man", "di ung", "da", "mun", "phat ban", "man do", "viem da", "nam da" } },
            { "Mắt", new List<string> { "mo mat", "do mat", "ngua mat", "cay mat", "dau mat", "chay nuoc mat", "nhin mo" } },
            { "Tai mũi họng", new List<string> { "dau hong", "so mui", "nghet mui", "ho", "viem hong", "tai", "mui", "hong", "u tai" } },
            { "Xương khớp", new List<string> { "dau lung", "dau goi", "dau khop", "nhuc xuong", "te tay", "te chan", "cot song", "vai gay" } }
        };

        public SpecialtyPredictionResult PredictSpecialty(string? reasonForVisit)
        {
            if (string.IsNullOrWhiteSpace(reasonForVisit))
            {
                return new SpecialtyPredictionResult
                {
                    PredictedSpecialty = string.Empty,
                    MatchScore = 0,
                    Message = "Chưa có mô tả triệu chứng để dự đoán chuyên khoa."
                };
            }

            var normalizedText = NormalizeText(reasonForVisit);

            string bestSpecialty = string.Empty;
            int bestScore = 0;
            List<string> bestMatchedKeywords = new();

            foreach (var item in _specialtyKeywords)
            {
                var matchedKeywords = item.Value
                    .Where(keyword => normalizedText.Contains(keyword))
                    .Distinct()
                    .ToList();

                var score = matchedKeywords.Count;

                if (score > bestScore)
                {
                    bestScore = score;
                    bestSpecialty = item.Key;
                    bestMatchedKeywords = matchedKeywords;
                }
            }

            if (bestScore == 0)
            {
                return new SpecialtyPredictionResult
                {
                    PredictedSpecialty = "Nội tổng quát",
                    MatchScore = 0,
                    MatchedKeywords = new List<string>(),
                    Message = "Không đủ từ khóa rõ ràng. Hệ thống tạm gợi ý Nội tổng quát."
                };
            }

            return new SpecialtyPredictionResult
            {
                PredictedSpecialty = bestSpecialty,
                MatchScore = bestScore,
                MatchedKeywords = bestMatchedKeywords,
                Message = $"Hệ thống gợi ý chuyên khoa: {bestSpecialty}."
            };
        }

        private static string NormalizeText(string input)
        {
            input = input.ToLowerInvariant().Trim();

            var replacements = new Dictionary<string, string>
            {
                { "á", "a" }, { "à", "a" }, { "ả", "a" }, { "ã", "a" }, { "ạ", "a" },
                { "ă", "a" }, { "ắ", "a" }, { "ằ", "a" }, { "ẳ", "a" }, { "ẵ", "a" }, { "ặ", "a" },
                { "â", "a" }, { "ấ", "a" }, { "ầ", "a" }, { "ẩ", "a" }, { "ẫ", "a" }, { "ậ", "a" },

                { "é", "e" }, { "è", "e" }, { "ẻ", "e" }, { "ẽ", "e" }, { "ẹ", "e" },
                { "ê", "e" }, { "ế", "e" }, { "ề", "e" }, { "ể", "e" }, { "ễ", "e" }, { "ệ", "e" },

                { "í", "i" }, { "ì", "i" }, { "ỉ", "i" }, { "ĩ", "i" }, { "ị", "i" },

                { "ó", "o" }, { "ò", "o" }, { "ỏ", "o" }, { "õ", "o" }, { "ọ", "o" },
                { "ô", "o" }, { "ố", "o" }, { "ồ", "o" }, { "ổ", "o" }, { "ỗ", "o" }, { "ộ", "o" },
                { "ơ", "o" }, { "ớ", "o" }, { "ờ", "o" }, { "ở", "o" }, { "ỡ", "o" }, { "ợ", "o" },

                { "ú", "u" }, { "ù", "u" }, { "ủ", "u" }, { "ũ", "u" }, { "ụ", "u" },
                { "ư", "u" }, { "ứ", "u" }, { "ừ", "u" }, { "ử", "u" }, { "ữ", "u" }, { "ự", "u" },

                { "ý", "y" }, { "ỳ", "y" }, { "ỷ", "y" }, { "ỹ", "y" }, { "ỵ", "y" },
                { "đ", "d" }
            };

            var builder = new StringBuilder(input);
            foreach (var replacement in replacements)
            {
                builder.Replace(replacement.Key, replacement.Value);
            }

            return builder.ToString();
        }
    }
}
```

### 2.4 Sửa File `Models/Specialy.cs`
Xóa dòng:
```csharp
public int MaxPatientsPerWeek { get; set; } = 100;
```

Giữ lại:
```csharp
namespace DoAnWeb.Models;

public class Specialty
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int AveragePatientLoad { get; set; } // Tải bệnh nhân trung bình hàng ngày
    public List<Doctor> Doctors { get; set; } = new();
}
```

### 2.5 Sửa File `appsettings.json`
Xóa phần:
```json
"PhoBertApi": {
    "Url": "http://localhost:5000"
}
```

## Bước 3: Rollback Database

```bash
# Xóa migration mới (ngược lại)
dotnet ef migrations remove AddMaxPatientsPerWeekToSpecialty
dotnet ef migrations remove AddSpecialtiesTable  # nếu cần

# Update database
dotnet ef database update
```

## Bước 4: Clean Build

```bash
# Clean solution
dotnet clean

# Rebuild
dotnet build

# Chạy lại
dotnet watch run
```

## ✅ Xong!
Hệ thống đã quay lại trạng thái ban đầu trước khi thêm Staffing & PhoBERT.

---

## ⚠️ Lưu Ý Quan Trọng

- **Backup dữ liệu** trước khi rollback migration database
- Nếu đã có dữ liệu trong bảng `Specialties` (MaxPatientsPerWeek), nó sẽ bị xóa
- Git history vẫn giữ lại, bạn có thể `git revert` thay vì xóa file thủ công
