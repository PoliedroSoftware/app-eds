@echo off
REM Instalar dotnet-sonarscanner globalmente (omite si ya está instalado)
dotnet tool install --global dotnet-sonarscanner

REM Iniciar análisis SonarCloud
dotnet sonarscanner begin /o:"poliedrocloud" /k:"poliedrocloud_APP.EDS" /d:sonar.host.url="https://sonarcloud.io" /d:sonar.token="0e5e0509be4953a4f21d4ba569f7994e8b2fd797"

REM Construir el proyecto
dotnet build

REM Finalizar análisis SonarCloud
dotnet sonarscanner end /d:sonar.token="0e5e0509be4953a4f21d4ba569f7994e8b2fd797"

pause
