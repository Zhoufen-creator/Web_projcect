"""
Python Flask API Server để inference PhoBERT model

Cài đặt dependencies:
pip install flask transformers torch numpy

Hướng dẫn:
1. Đặt model file vào thư mục models/
2. Chạy script: python phobert_api.py
3. Server sẽ chạy tại http://localhost:5000
"""

from flask import Flask, request, jsonify
import numpy as np
import torch
from transformers import AutoTokenizer, AutoModelForSequenceClassification
import json
from pathlib import Path

app = Flask(__name__)

# Mapping chuyên khoa
SPECIALTY_MAPPING = {
    0: "Khoa tiêu hóa",
    1: "Khoa thần kinh",
    2: "Khoa hô hấp",
    3: "Khoa cơ-xương khớp",
    4: "Khoa da liễu",
    5: "Khoa tim mạch",
    6: "Khoa tai mũi họng",
    7: "Khoa mắt",
    8: "Khoa răng hàm mặt",
    9: "Khoa nội tiết",
    10: "Khoa thận - tiết niệu",
    11: "Khoa phụ sản",
    12: "Khoa truyền nhiễm",
    13: "Khoa tâm thần kinh /  tâm lý",
    14: "Khoa nhi"
}

# Load model (điều chỉnh đường dẫn phù hợp)
MODEL_PATH = "final_model_v7-20260403T134236Z-1-001/final_model_v7"  # Đường dẫn tới model folder
TOKENIZER = None
MODEL = None

def load_model():
    global TOKENIZER, MODEL
    try:
        TOKENIZER = AutoTokenizer.from_pretrained(MODEL_PATH)
        # Sử dụng AutoModelForSequenceClassification để load classifier head
        MODEL = AutoModelForSequenceClassification.from_pretrained(MODEL_PATH)
        MODEL.eval()  # Đặt model ở chế độ eval (không train)
        print(f"✓ Model loaded successfully from {MODEL_PATH}")
        print(f"✓ Model type: {type(MODEL)}")
        print(f"✓ Number of labels: {MODEL.num_labels}")
    except Exception as e:
        print(f"✗ Error loading model: {e}")
        TOKENIZER = None
        MODEL = None

@app.route('/predict', methods=['POST'])
def predict():
    """
    Endpoint dự đoán chuyên khoa
    Request: {"text": "Tôi bị đau ngực và khó thở"}
    Response: {"specialty": "Khoa Tim mạch", "confidence": 95, "message": "..."}
    """
    try:
        data = request.get_json()
        text = data.get('text', '').strip()
        
        if not text:
            return jsonify({
                "specialty": "Khoa nội",
                "confidence": 0,
                "message": "Chưa có mô tả triệu chứng"
            }), 400
        
        if TOKENIZER is None or MODEL is None:
            return jsonify({
                "specialty": "Khoa nội",
                "confidence": 0,
                "message": "Model chưa được load"
            }), 500
        
        # Tokenize input text
        inputs = TOKENIZER(text, return_tensors="pt", padding=True, truncation=True)
        
        # Forward pass để lấy logits
        with torch.no_grad():
            outputs = MODEL(**inputs)
        
        # Nếu là RobertaForSequenceClassification, nó sẽ có logits
        # Forward pass để lấy logits
        with torch.no_grad():
            outputs = MODEL(**inputs)
        
        if hasattr(outputs, 'logits') and outputs.logits is not None:
            logits = outputs.logits[0]  # Shape: (num_labels,)
            
            # 1. SỬA LỖI TOÁN HỌC: Dùng Sigmoid cho Multi-label thay vì Softmax
            probabilities = torch.sigmoid(logits)
            
            # Lấy vị trí khoa có xác suất cao nhất
            predicted_idx = torch.argmax(probabilities).item()
            confidence = int(probabilities[predicted_idx].item() * 100)
            
            # 2. SỬA LỖI LẤY TÊN KHOA: Ưu tiên dùng SPECIALTY_MAPPING của bạn
            predicted_specialty = "Không xác định" # Đổi từ "Khoa nội" sang "Không xác định"
            
            # Thử lấy từ config của model trước
            if hasattr(MODEL, 'config') and hasattr(MODEL.config, 'id2label') and MODEL.config.id2label:
                # Kiểm tra cả key dạng string và dạng int
                if str(predicted_idx) in MODEL.config.id2label:
                    predicted_specialty = MODEL.config.id2label[str(predicted_idx)]
                elif int(predicted_idx) in MODEL.config.id2label:
                    predicted_specialty = MODEL.config.id2label[int(predicted_idx)]
            
            # Nếu model config trả về dạng "LABEL_0" hoặc không có, ép dùng MAPPING của bạn
            if predicted_specialty == "Không xác định" or predicted_specialty.startswith("LABEL_"):
                predicted_specialty = SPECIALTY_MAPPING.get(predicted_idx, "Không xác định")
            
            return jsonify({
                "specialty": predicted_specialty,
                "confidence": confidence,
                "message": f"PhoBERT gợi ý: {predicted_specialty} (độ tin cậy: {confidence}%)"
            })
        else:
            return jsonify({
                "specialty": "Không xác định",
                "confidence": 0,
                "message": "Model không trả về logits. Liên hệ admin."
            }), 500
            
    except Exception as e:
        import traceback
        traceback.print_exc()
        return jsonify({
            "specialty": "Lỗi Hệ Thống", # Bỏ chữ Khoa nội đi
            "confidence": 0,
            "message": f"Lỗi chi tiết: {str(e)}"
        }), 500
@app.route('/health', methods=['GET'])
def health():
    """Health check endpoint"""
    return jsonify({"status": "ok", "model_loaded": TOKENIZER is not None})

if __name__ == '__main__':
    load_model()
    app.run(debug=True, host='0.0.0.0', port=5000)
