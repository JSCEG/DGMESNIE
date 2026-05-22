param(
    [string]$ExcelPath = "c:\Proyectos\1.-DGMESNIE\Documentacion\PVIRCE2026-2040_SQ.xlsx",
    [string]$OutputSqlPath = "c:\Proyectos\1.-DGMESNIE\Documentacion\DGMESNIE_PVIRSE_CARGA_INICIAL.sql",
    [string]$SheetName = "PVIRCE2026-2040_S1-VPE"
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

function Normalize-Header {
    param([string]$Value)

    if ([string]::IsNullOrWhiteSpace($Value)) {
        return ""
    }

    $normalized = $Value.Normalize([Text.NormalizationForm]::FormD)
    $builder = New-Object System.Text.StringBuilder

    foreach ($character in $normalized.ToCharArray()) {
        $category = [Globalization.CharUnicodeInfo]::GetUnicodeCategory($character)
        if ($category -eq [Globalization.UnicodeCategory]::NonSpacingMark) {
            continue
        }

        if ([char]::IsLetterOrDigit($character)) {
            [void]$builder.Append([char]::ToUpperInvariant($character))
        }
    }

    return $builder.ToString()
}

function Parse-NullableInt {
    param([string]$Value)

    if ([string]::IsNullOrWhiteSpace($Value)) {
        return $null
    }

    $parsed = 0
    if ([int]::TryParse($Value.Trim(), [ref]$parsed)) {
        return $parsed
    }

    return $null
}

function Parse-NullableDecimal {
    param([string]$Value)

    if ([string]::IsNullOrWhiteSpace($Value)) {
        return $null
    }

    $sanitized = $Value.Replace(',', '').Trim()
    $parsed = 0.0
    if ([decimal]::TryParse($sanitized, [Globalization.NumberStyles]::Number, [Globalization.CultureInfo]::InvariantCulture, [ref]$parsed)) {
        return [decimal]::Round($parsed, 2)
    }

    return $null
}

function Sql-String {
    param([string]$Value)

    if ([string]::IsNullOrWhiteSpace($Value)) {
        return "NULL"
    }

    $escaped = $Value.Trim().Replace("'", "''")
    return "N'$escaped'"
}

function Sql-Int {
    param($Value)

    if ($null -eq $Value) {
        return "NULL"
    }

    return [string]$Value
}

function Sql-Decimal {
    param($Value)

    if ($null -eq $Value) {
        return "NULL"
    }

    return ([decimal]$Value).ToString('0.00', [Globalization.CultureInfo]::InvariantCulture)
}

$headerMap = @{
    'STATUS'                  = 'Status'
    'STATUSVF'                = 'StatusVf'
    'NOMBREREAL'              = 'NombreReal'
    'NOCONSIDERAR'            = 'NoConsiderar'
    'ANO'                     = 'Anio'
    'ADICIONESOSUSTITUCIONES' = 'AdicionesOSustituciones'
    'CONTRATOOUNIDAD'         = 'ContratoOUnidad'
    'TIPO'                    = 'Tipo'
    'TIPOVF'                  = 'TipoVf'
    'RENOVABLE'               = 'Renovable'
    'MW'                      = 'Mw'
    'MES'                     = 'Mes'
    'GERENCIADECONTROL'       = 'GerenciaDeControl'
    'REGIONDETRANSMISION'     = 'RegionDeTransmision'
    'ENTIDADFEDERATIVA'       = 'EntidadFederativa'
    'MUNICIPIO'               = 'Municipio'
    'FIRMES'                  = 'Firmes'
}

$sqlColumns = @(
    'Status', 'StatusVf', 'NombreReal', 'NoConsiderar', 'Anio', 'AdicionesOSustituciones', 'ContratoOUnidad',
    'Tipo', 'TipoVf', 'Renovable', 'Mw', 'Mes', 'GerenciaDeControl', 'RegionDeTransmision', 'EntidadFederativa',
    'Municipio', 'Firmes', 'FuenteArchivo', 'FechaCarga', 'Activo'
)

$excel = $null
$workbook = $null
$worksheet = $null

try {
    if (-not (Test-Path -LiteralPath $ExcelPath)) {
        throw "No se encontró el archivo Excel: $ExcelPath"
    }

    $excel = New-Object -ComObject Excel.Application
    $excel.Visible = $false
    $excel.DisplayAlerts = $false

    $workbook = $excel.Workbooks.Open($ExcelPath)
    $worksheet = $workbook.Worksheets.Item($SheetName)
    $range = $worksheet.UsedRange
    $rowCount = $range.Rows.Count
    $columnCount = $range.Columns.Count

    $headerRowNumber = $null
    $headers = @{}

    for ($row = 1; $row -le [Math]::Min($rowCount, 10); $row++) {
        $candidateHeaders = @{}
        $normalizedValues = @()

        for ($column = 1; $column -le $columnCount; $column++) {
            $normalized = Normalize-Header -Value ([string]$range.Item($row, $column).Text)
            $candidateHeaders[$column] = $normalized
            $normalizedValues += $normalized
        }

        if ($normalizedValues -contains 'STATUS' -and $normalizedValues -contains 'ENTIDADFEDERATIVA' -and $normalizedValues -contains 'MW') {
            $headerRowNumber = $row
            $headers = $candidateHeaders
            break
        }
    }

    if ($null -eq $headerRowNumber) {
        throw 'No fue posible localizar la fila de encabezados en el Excel de PVIRSE.'
    }

    $builder = New-Object System.Text.StringBuilder
    [void]$builder.AppendLine('SET NOCOUNT ON;')
    [void]$builder.AppendLine('SET XACT_ABORT ON;')
    [void]$builder.AppendLine('')
    [void]$builder.AppendLine("IF OBJECT_ID('dgmesnie.PvirseCentralesElectricas', 'U') IS NULL")
    [void]$builder.AppendLine('BEGIN')
    [void]$builder.AppendLine("    THROW 50000, 'La tabla dgmesnie.PvirseCentralesElectricas no existe. Ejecuta primero el script base.', 1;")
    [void]$builder.AppendLine('END')
    [void]$builder.AppendLine('GO')
    [void]$builder.AppendLine('')
    [void]$builder.AppendLine('BEGIN TRY')
    [void]$builder.AppendLine('    BEGIN TRANSACTION;')
    [void]$builder.AppendLine('')
    [void]$builder.AppendLine('    DELETE FROM dgmesnie.PvirseCentralesElectricas;')
    [void]$builder.AppendLine('')

    $inserted = 0

    for ($row = $headerRowNumber + 1; $row -le $rowCount; $row++) {
        $record = @{}
        $hasData = $false

        for ($column = 1; $column -le $columnCount; $column++) {
            $header = $headers[$column]
            if (-not $headerMap.ContainsKey($header)) {
                continue
            }

            $text = [string]$range.Item($row, $column).Text
            if (-not [string]::IsNullOrWhiteSpace($text)) {
                $hasData = $true
            }

            $record[$headerMap[$header]] = $text.Trim()
        }

        if (-not $hasData) {
            continue
        }

        if ([string]::IsNullOrWhiteSpace($record['NombreReal'])) {
            continue
        }

        $values = @(
            (Sql-String -Value $record['Status']),
            (Sql-String -Value $record['StatusVf']),
            (Sql-String -Value $record['NombreReal']),
            (Sql-String -Value $record['NoConsiderar']),
            (Sql-Int -Value (Parse-NullableInt -Value $record['Anio'])),
            (Sql-String -Value $record['AdicionesOSustituciones']),
            (Sql-String -Value $record['ContratoOUnidad']),
            (Sql-String -Value $record['Tipo']),
            (Sql-String -Value $record['TipoVf']),
            (Sql-String -Value $record['Renovable']),
            (Sql-Decimal -Value (Parse-NullableDecimal -Value $record['Mw'])),
            (Sql-String -Value $record['Mes']),
            (Sql-String -Value $record['GerenciaDeControl']),
            (Sql-String -Value $record['RegionDeTransmision']),
            (Sql-String -Value $record['EntidadFederativa']),
            (Sql-String -Value $record['Municipio']),
            (Sql-String -Value $record['Firmes']),
            (Sql-String -Value ([IO.Path]::GetFileName($ExcelPath))),
            'SYSUTCDATETIME()',
            '1'
        )

        [void]$builder.AppendLine('    INSERT INTO dgmesnie.PvirseCentralesElectricas')
        [void]$builder.AppendLine('    (')
        [void]$builder.AppendLine('        ' + ($sqlColumns -join ', '))
        [void]$builder.AppendLine('    )')
        [void]$builder.AppendLine('    VALUES')
        [void]$builder.AppendLine('    (')
        [void]$builder.AppendLine('        ' + ($values -join ', '))
        [void]$builder.AppendLine('    );')
        [void]$builder.AppendLine('')

        $inserted++
    }

    [void]$builder.AppendLine("    PRINT 'Registros cargados: $inserted';")
    [void]$builder.AppendLine('    COMMIT TRANSACTION;')
    [void]$builder.AppendLine('END TRY')
    [void]$builder.AppendLine('BEGIN CATCH')
    [void]$builder.AppendLine('    IF @@TRANCOUNT > 0')
    [void]$builder.AppendLine('        ROLLBACK TRANSACTION;')
    [void]$builder.AppendLine('')
    [void]$builder.AppendLine('    DECLARE @ErrorMessage NVARCHAR(4000) = ERROR_MESSAGE();')
    [void]$builder.AppendLine('    DECLARE @ErrorSeverity INT = ERROR_SEVERITY();')
    [void]$builder.AppendLine('    DECLARE @ErrorState INT = ERROR_STATE();')
    [void]$builder.AppendLine('')
    [void]$builder.AppendLine('    RAISERROR(@ErrorMessage, @ErrorSeverity, @ErrorState);')
    [void]$builder.AppendLine('END CATCH;')
    [void]$builder.AppendLine('GO')

    [IO.File]::WriteAllText($OutputSqlPath, $builder.ToString(), [Text.Encoding]::UTF8)
    Write-Host "Script SQL generado en: $OutputSqlPath"
    Write-Host "Registros preparados: $inserted"
}
finally {
    if ($worksheet) { [void][System.Runtime.InteropServices.Marshal]::ReleaseComObject($worksheet) }
    if ($workbook) {
        $workbook.Close($false)
        [void][System.Runtime.InteropServices.Marshal]::ReleaseComObject($workbook)
    }
    if ($excel) {
        $excel.Quit()
        [void][System.Runtime.InteropServices.Marshal]::ReleaseComObject($excel)
    }

    [GC]::Collect()
    [GC]::WaitForPendingFinalizers()
}