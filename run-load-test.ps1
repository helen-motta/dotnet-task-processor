param(
    [string]$ApiUrl = "http://localhost:5000",
    [switch]$Headless,
    [int]$Users = 50,
    [int]$SpawnRate = 10,
    [string]$Duration = "1m"
)

$locust = Join-Path $PSScriptRoot ".venv\Scripts\locust.exe"
$locustFile = Join-Path $PSScriptRoot "Tests\load\locustfile.py"

if (-not (Test-Path $locust)) {
    throw "Locust não encontrado. Instale as dependências na .venv primeiro."
}

$locustArguments = @(
    "-f", $locustFile,
    "--host", $ApiUrl
)

if ($Headless) {
    $locustArguments += @(
        "--headless",
        "-u", $Users,
        "-r", $SpawnRate,
        "-t", $Duration
    )
}

& $locust @locustArguments