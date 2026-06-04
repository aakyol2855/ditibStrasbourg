# Script de préparation des données pour la carte interactive
# Télécharge et filtre le GeoJSON des départements français

$targetCodes = @('55', '52', '70', '25', '90', '68', '88', '54', '57', '67')
$url = "https://raw.githubusercontent.com/gregoiredavid/france-geojson/master/departements-version-simplifiee.geojson"
$outputFile = Join-Path $PSScriptRoot "departements.js"

Write-Host "Téléchargement du GeoJSON des départements..."
try {
    # Augmenter le timeout ou forcer TLS 1.2
    [Net.ServicePointManager]::SecurityProtocol = [Net.SecurityProtocolType]::Tls12
    $response = Invoke-WebRequest -Uri $url -UseBasicParsing
    $data = $response.Content | ConvertFrom-Json
    
    Write-Host "Filtrage des départements demandés..."
    $filteredFeatures = @()
    foreach ($feature in $data.features) {
        $code = $feature.properties.code
        if ($targetCodes -contains $code) {
            $filteredFeatures += $feature
        }
    }
    
    # Création du nouvel objet GeoJSON
    $filteredGeoJson = [PSCustomObject]@{
        type = "FeatureCollection"
        features = $filteredFeatures
    }
    
    # Conversion en JSON et écriture avec variable JS
    Write-Host "Sauvegarde dans $outputFile..."
    $jsonString = ConvertTo-Json -InputObject $filteredGeoJson -Depth 100
    $jsContent = "const DEPARTEMENTS_GEOJSON = $jsonString;"
    [System.IO.File]::WriteAllText($outputFile, $jsContent, [System.Text.Encoding]::UTF8)
    Write-Host "Succès ! Le fichier departements.js contenant la variable a été créé."
}
catch {
    Write-Error "Erreur lors du traitement : $_"
}
