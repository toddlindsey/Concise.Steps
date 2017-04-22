$packagePath = [System.IO.Path]::Combine($PSScriptRoot, "Concise.Steps\bin\Release\Concise.Steps.0.1.0.nupkg")

Write-Host Press any key to publish $packagePath
$host.UI.RawUI.ReadKey("NoEcho")

nuget.exe push "$packagePath" -Source https://www.nuget.org/api/v2/package
