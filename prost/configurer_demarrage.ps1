# Configurer le démarrage automatique de CartoBureau

$TargetFile = "C:\Users\Ditib.2026\OneDrive\Desktop\carte-departements\demarrer_serveur.bat"
$ShortcutFile = "$env:APPDATA\Microsoft\Windows\Start Menu\Programs\Startup\CartoBureau.lnk"

Write-Host "Configuration du demarrage automatique..." -ForegroundColor Cyan

if (Test-Path $TargetFile) {
    try {
        $WScriptShell = New-Object -ComObject WScript.Shell
        $Shortcut = $WScriptShell.CreateShortcut($ShortcutFile)
        $Shortcut.TargetPath = $TargetFile
        $Shortcut.WorkingDirectory = "C:\Users\Ditib.2026\OneDrive\Desktop\carte-departements"
        # 7 = Fenetre minimisee au demarrage de Windows
        $Shortcut.WindowStyle = 7
        $Shortcut.Save()
        Write-Host "Le raccourci de demarrage automatique a ete cree avec succes !" -ForegroundColor Green
        Write-Host "Emplacement : $ShortcutFile" -ForegroundColor Green
    }
    catch {
        Write-Error "Impossible de creer le raccourci : $_"
    }
} else {
    Write-Error "Fichier cible introuvable a l'emplacement : $TargetFile"
}
