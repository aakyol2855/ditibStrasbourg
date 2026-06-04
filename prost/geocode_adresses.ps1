# Script de géocodage d'adresses précises pour la carte interactive
# Lit 'adresses_input.csv', interroge l'API Adresse et génère 'adresses.js'

$targetDepartments = @('55', '52', '70', '25', '90', '68', '88', '54', '57', '67')
$inputFile = Join-Path $PSScriptRoot "adresses_input.csv"
$outputFile = Join-Path $PSScriptRoot "adresses.js"

# Noms des départements pour l'enrichissement
$depNames = @{
    '25' = 'Doubs'
    '52' = 'Haute-Marne'
    '54' = 'Meurthe-et-Moselle'
    '55' = 'Meuse'
    '57' = 'Moselle'
    '67' = 'Bas-Rhin'
    '68' = 'Haut-Rhin'
    '70' = ("Haute-Sa" + [char]0xF4 + "ne")
    '88' = 'Vosges'
    '90' = 'Territoire de Belfort'
}

if (-not (Test-Path $inputFile)) {
    Write-Host "Fichier d'entrée 'adresses_input.csv' introuvable. Veuillez le créer."
    exit
}

Write-Host "Lecture des adresses depuis le fichier CSV : adresses_input.csv..."
$firstLine = Get-Content $inputFile -Head 1
$delimiter = ","
if ($firstLine -match ";") { $delimiter = ";" }

$csvData = Import-Csv -Path $inputFile -Delimiter $delimiter
$results = @()
$errors = @()

Write-Host "Début du géocodage de $($csvData.Count) adresses..."

foreach ($row in $csvData) {
    $adresse = ""
    $nom = ""
    
    if ($row.Adresse) {
        $adresse = $row.Adresse.Trim()
        if ($row.Nom) { $nom = $row.Nom.Trim() }
    }
    elseif ($row.Address) {
        $adresse = $row.Address.Trim()
        if ($row.Name) { $nom = $row.Name.Trim() }
    }
    else {
        # S'il n'y a pas de colonne explicite, on prend la première colonne (ou toute la ligne)
        $props = $row | Get-Member -MemberType NoteProperty
        if ($props.Count -gt 0) {
            $adresse = $row.($props[0].Name).Trim()
            # Si une deuxième colonne existe, on l'utilise pour le nom
            if ($props.Count -gt 1) {
                $nom = $row.($props[1].Name).Trim()
            }
        }
    }
    
    if (-not $adresse -or $adresse -eq "") { continue }
    
    # Si le nom est vide, on utilisera par défaut le nom nettoyé de l'adresse renvoyé par l'API
    $isDefaultNom = $false
    if (-not $nom -or $nom -eq "") {
        $nom = "Adresse"
        $isDefaultNom = $true
    }
    
    $query = $adresse
    Write-Host "Géocodage de : '$nom' -> '$query'..."
    
    $encodedQuery = [uri]::EscapeDataString($query)
    $url = "https://api-adresse.data.gouv.fr/search/?q=$encodedQuery&limit=3"
    
    try {
        [Net.ServicePointManager]::SecurityProtocol = [Net.SecurityProtocolType]::Tls12
        $response = Invoke-RestMethod -Uri $url
        
        $found = $false
        if ($response.features -and $response.features.Count -gt 0) {
            foreach ($feature in $response.features) {
                $postcode = $feature.properties.postcode
                $depCode = $postcode.Substring(0, 2)
                
                # S'assurer que le résultat correspond à notre zone d'intérêt
                if ($targetDepartments -contains $depCode) {
                    $lon = $feature.geometry.coordinates[0]
                    $lat = $feature.geometry.coordinates[1]
                    $label = $feature.properties.label
                    
                    $finalNom = $nom
                    if ($isDefaultNom) {
                        $finalNom = $feature.properties.name
                        if (-not $finalNom) { $finalNom = $label }
                    }
                    
                    $results += [PSCustomObject]@{
                        id = "addr-" + $results.Count
                        name = $finalNom
                        query = $query
                        resolvedAddress = $label
                        postcode = $postcode
                        departmentCode = $depCode
                        departmentName = $depNames[$depCode]
                        lat = $lat
                        lon = $lon
                    }
                    Write-Host " -> Trouvé : $label ($postcode, $($depNames[$depCode]))"
                    $found = $true
                    break
                }
            }
        }
        
        if (-not $found) {
            # Si aucune correspondance dans nos départements, essayer sans restreindre
            if ($response.features -and $response.features.Count -gt 0) {
                $first = $response.features[0]
                $postcode = $first.properties.postcode
                $depCode = $postcode.Substring(0, 2)
                $lon = $first.geometry.coordinates[0]
                $lat = $first.geometry.coordinates[1]
                $label = $first.properties.label
                
                $dName = "Autre ($depCode)"
                if ($depNames.ContainsKey($depCode)) {
                    $dName = $depNames[$depCode]
                }
                
                $finalNom = $nom
                if ($isDefaultNom) {
                    $finalNom = $first.properties.name
                    if (-not $finalNom) { $finalNom = $label }
                }
                
                $results += [PSCustomObject]@{
                    id = "addr-" + $results.Count
                    name = $finalNom
                    query = $query
                    resolvedAddress = $label
                    postcode = $postcode
                    departmentCode = $depCode
                    departmentName = $dName
                    lat = $lat
                    lon = $lon
                }
                Write-Host " -> Trouvé (hors zone cible !) : $label ($postcode)"
            } else {
                $finalNom = $nom
                Write-Warning " -> Non trouvé : $query"
                $errors += "$finalNom ($query)"
            }
        }
    }
    catch {
        $finalNom = $nom
        Write-Error "Erreur lors du géocodage de $finalNom : $_"
        $errors += "$finalNom ($query)"
    }
    
    Start-Sleep -Milliseconds 150
}

# Conversion en JSON et écriture dans adresses.js
$jsonString = ConvertTo-Json -InputObject $results -Depth 100
$jsContent = "const ADRESSES_DATA = $jsonString;"
[System.IO.File]::WriteAllText($outputFile, $jsContent, [System.Text.Encoding]::UTF8)

Write-Host "`nGéocodage des adresses terminé !"
Write-Host "Succès : $($results.Count) adresses enregistrées."
if ($errors.Count -gt 0) {
    Write-Warning "Échecs ($($errors.Count)) : $($errors -join ', ')"
}
