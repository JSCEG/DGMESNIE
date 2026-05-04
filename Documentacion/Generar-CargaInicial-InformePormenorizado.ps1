param(
    [string]$ExcelPath = "c:\Proyectos\1.-DGMESNIE\Documentacion\INF Pormenorizado Tablas Rev 251125 SENER.xlsx",
    [string]$OutputSqlPath = "c:\Proyectos\1.-DGMESNIE\Documentacion\DGMESNIE_INFORME_PORMENORIZADO_MODERNIZACION_CARGA_INICIAL.sql",
    [string]$SheetName = "BASE"
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

    $sanitized = $Value.Replace(',', '').Trim()
    $parsed = 0
    if ([int]::TryParse($sanitized, [ref]$parsed)) {
        return $parsed
    }

    return $null
}

function Parse-NullableDecimal {
    param([string]$Value)

    if ([string]::IsNullOrWhiteSpace($Value)) {
        return $null
    }

    $sanitized = $Value.Replace('$', '').Replace('%', '').Replace(' ', '').Trim()
    $styles = [Globalization.NumberStyles]::Number -bor [Globalization.NumberStyles]::AllowCurrencySymbol
    $parsed = 0.0

    if ([decimal]::TryParse($sanitized, $styles, [Globalization.CultureInfo]::InvariantCulture, [ref]$parsed)) {
        return [decimal]::Round($parsed, 2)
    }

    if ([decimal]::TryParse($sanitized, $styles, [Globalization.CultureInfo]::GetCultureInfo('es-MX'), [ref]$parsed)) {
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
    'NO'                                                                 = 'Numero'
    'NOORIGINAL'                                                         = 'NumeroOriginal'
    'GRT'                                                                = 'GRT'
    'NOMBREDELPROYECTO'                                                  = 'NombreProyecto'
    'TIPODEFINANCIAMIENTO'                                               = 'TipoFinanciamiento'
    'ANODEINSTRUCCION'                                                   = 'AnioInstruccion'
    'ETAPADELPROYECTO'                                                   = 'EtapaProyecto'
    'MONTODELPROYECTOMDP'                                                = 'MontoProyectoMdp'
    'ELEMENTOSYEQUIPOSASOCIADOS'                                         = 'ElementosEquiposAsociados'
    'FECHAESTIMADADEINICIO'                                              = 'FechaEstimadaInicio'
    'FEOINDICADAENOFICIOSENER'                                           = 'FeoIndicadaOficioSener'
    'FEOFACTIBLE'                                                        = 'FeoFactible'
    'AVANCEDEEJECUCION'                                                  = 'PorcentajeAvanceEjecucion'
    'CIRCUNSTANCIASQUEHAYANOCASIONADOATRASOSENLASOBRAS'                  = 'CircunstanciasAtrasos'
    'ACCIONESDEMITIGACIONOCORRECCIONLLEVADASACABOPARACORREGIRLOSATRASOS' = 'AccionesMitigacionCorreccion'
    'ESTADOREALQUEGUARDAELPROYECTO'                                      = 'EstadoRealProyecto'
    'COMENTARIOSSOBREELNIVELDEPRIORIZACION'                              = 'ComentariosNivelPriorizacion'
    'CLAVEPEM'                                                           = 'ClavePem'
    'CLASIFICACIONSENER'                                                 = 'ClasificacionSener'
    'FECHADEPROGRAMACIONTRIMESTRE'                                       = 'FechaProgramacionTrimestre'
    'QUINCENADEPUBLICACIONQUINCENA'                                      = 'QuincenaPublicacion'
    'UNIVERSOPRESENTACIONPRESIDENCIA'                                    = 'UniversoPresentacionPresidencia'
    'MVA'                                                                = 'Mva'
    'MVAR'                                                               = 'Mvar'
    'KMC'                                                                = 'KmC'
}

$sqlColumns = @(
    'Numero', 'NumeroOriginal', 'GRT', 'NombreProyecto', 'TipoFinanciamiento', 'AnioInstruccion', 'EtapaProyecto',
    'MontoProyectoMdp', 'ElementosEquiposAsociados', 'FechaEstimadaInicio', 'FeoIndicadaOficioSener', 'FeoFactible',
    'PorcentajeAvanceEjecucion', 'CircunstanciasAtrasos', 'AccionesMitigacionCorreccion', 'EstadoRealProyecto',
    'ComentariosNivelPriorizacion', 'ClavePem', 'ClasificacionSener', 'FechaProgramacionTrimestre', 'QuincenaPublicacion',
    'UniversoPresentacionPresidencia', 'Mva', 'Mvar', 'KmC', 'FuenteArchivo', 'Activo'
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

    $headers = @{}
    for ($column = 1; $column -le $columnCount; $column++) {
        $headers[$column] = Normalize-Header -Value ([string]$range.Item(1, $column).Text)
    }

    $builder = New-Object System.Text.StringBuilder
    [void]$builder.AppendLine('SET NOCOUNT ON;')
    [void]$builder.AppendLine('SET XACT_ABORT ON;')
    [void]$builder.AppendLine('')
    [void]$builder.AppendLine("IF OBJECT_ID('dgmesnie.InformePormenorizadoModernizacion', 'U') IS NULL")
    [void]$builder.AppendLine('BEGIN')
    [void]$builder.AppendLine("    THROW 50000, 'La tabla dgmesnie.InformePormenorizadoModernizacion no existe. Ejecuta primero el script de creación.', 1;")
    [void]$builder.AppendLine('END')
    [void]$builder.AppendLine('GO')
    [void]$builder.AppendLine('')
    [void]$builder.AppendLine('BEGIN TRY')
    [void]$builder.AppendLine('    BEGIN TRANSACTION;')
    [void]$builder.AppendLine('')
    [void]$builder.AppendLine('    DELETE FROM dgmesnie.InformePormenorizadoModernizacion;')
    [void]$builder.AppendLine('')

    $inserted = 0

    for ($row = 2; $row -le $rowCount; $row++) {
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

        $numero = Parse-NullableInt -Value $record['Numero']
        $nombreProyecto = $record['NombreProyecto']
        if ($null -eq $numero -or [string]::IsNullOrWhiteSpace($nombreProyecto)) {
            continue
        }

        $values = @(
            (Sql-Int -Value $numero),
            (Sql-Int -Value (Parse-NullableInt -Value $record['NumeroOriginal'])),
            (Sql-String -Value $record['GRT']),
            (Sql-String -Value $record['NombreProyecto']),
            (Sql-String -Value $record['TipoFinanciamiento']),
            (Sql-Int -Value (Parse-NullableInt -Value $record['AnioInstruccion'])),
            (Sql-String -Value $record['EtapaProyecto']),
            (Sql-Decimal -Value (Parse-NullableDecimal -Value $record['MontoProyectoMdp'])),
            (Sql-String -Value $record['ElementosEquiposAsociados']),
            (Sql-String -Value $record['FechaEstimadaInicio']),
            (Sql-String -Value $record['FeoIndicadaOficioSener']),
            (Sql-String -Value $record['FeoFactible']),
            (Sql-Decimal -Value (Parse-NullableDecimal -Value $record['PorcentajeAvanceEjecucion'])),
            (Sql-String -Value $record['CircunstanciasAtrasos']),
            (Sql-String -Value $record['AccionesMitigacionCorreccion']),
            (Sql-String -Value $record['EstadoRealProyecto']),
            (Sql-String -Value $record['ComentariosNivelPriorizacion']),
            (Sql-String -Value $record['ClavePem']),
            (Sql-String -Value $record['ClasificacionSener']),
            (Sql-String -Value $record['FechaProgramacionTrimestre']),
            (Sql-String -Value $record['QuincenaPublicacion']),
            (Sql-String -Value $record['UniversoPresentacionPresidencia']),
            (Sql-Decimal -Value (Parse-NullableDecimal -Value $record['Mva'])),
            (Sql-Decimal -Value (Parse-NullableDecimal -Value $record['Mvar'])),
            (Sql-Decimal -Value (Parse-NullableDecimal -Value $record['KmC'])),
            (Sql-String -Value 'INF Pormenorizado Tablas Rev 251125 SENER.xlsx'),
            '1'
        )

        [void]$builder.AppendLine('    INSERT INTO dgmesnie.InformePormenorizadoModernizacion')
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

    [System.IO.File]::WriteAllText($OutputSqlPath, $builder.ToString(), [System.Text.UTF8Encoding]::new($false))

    Write-Output "Archivo generado: $OutputSqlPath"
    Write-Output "Registros exportados: $inserted"
}
finally {
    if ($workbook) {
        $workbook.Close($false)
    }

    if ($worksheet) {
        [System.Runtime.InteropServices.Marshal]::ReleaseComObject($worksheet) | Out-Null
    }

    if ($workbook) {
        [System.Runtime.InteropServices.Marshal]::ReleaseComObject($workbook) | Out-Null
    }

    if ($excel) {
        $excel.Quit() | Out-Null
        [System.Runtime.InteropServices.Marshal]::ReleaseComObject($excel) | Out-Null
    }

    [GC]::Collect()
    [GC]::WaitForPendingFinalizers()
}