param(
    [string]$BindHost = "127.0.0.1",
    [int]$Port = 5000,
    [string]$ModelDir = ".\datasets\phobert_runs\run_v1\best_model",
    [string]$LabelMap = ".\datasets\processed_phobert_v1\label_mapping.json",
    [int]$MaxLength = 128,
    [int]$TopK = 3
)

$ErrorActionPreference = "Stop"

$projectRoot = Split-Path -Parent $PSScriptRoot
$pythonExe = Join-Path $projectRoot ".env3.11\Scripts\python.exe"
$resolvedModelDir = [System.IO.Path]::GetFullPath((Join-Path $projectRoot $ModelDir))
$resolvedLabelMap = [System.IO.Path]::GetFullPath((Join-Path $projectRoot $LabelMap))

if (-not (Test-Path $pythonExe)) {
    throw "Khong tim thay python virtualenv tai: $pythonExe"
}

if (-not (Test-Path $resolvedModelDir)) {
    throw "Khong tim thay model PhoBERT tai: $resolvedModelDir"
}

if (-not (Test-Path $resolvedLabelMap)) {
    throw "Khong tim thay label map tai: $resolvedLabelMap"
}

$env:HF_HUB_OFFLINE = "1"
$env:TRANSFORMERS_OFFLINE = "1"

& $pythonExe `
    (Join-Path $projectRoot "datasets\phobert_api.py") `
    --host $BindHost `
    --port $Port `
    --model-dir $resolvedModelDir `
    --label-map $resolvedLabelMap `
    --max-length $MaxLength `
    --top-k $TopK `
    --local-files-only
