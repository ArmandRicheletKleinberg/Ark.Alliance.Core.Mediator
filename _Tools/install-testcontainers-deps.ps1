# Install Testcontainers dependencies for local test execution
# See _SDLC/04_Testing/testcontainers_dotnet_9.Md for background
# This script installs Docker and the .NET 9 SDK if missing and pulls
# the RabbitMQ image used by Ark.Alliance.Core.Mq.RabbitMq.Test.

function Install-DotNet {
    if (Get-Command dotnet -ErrorAction SilentlyContinue) {
        $sdks = dotnet --list-sdks
        if ($sdks -match '^9\.0') { return }
    }
    Write-Host "Installing .NET 9 SDK..."
    $script = Join-Path $env:TEMP "dotnet-install.ps1"
    Invoke-WebRequest "https://dot.net/v1/dotnet-install.ps1" -OutFile $script
    & $script -Channel 9.0 -InstallDir "$env:USERPROFILE\.dotnet"
    $env:PATH += ";$env:USERPROFILE\.dotnet"
}

function Install-Docker-Windows {
    if (Get-Command docker -ErrorAction SilentlyContinue) { return }
    if (Get-Command winget -ErrorAction SilentlyContinue) {
        winget install -e --id Docker.DockerDesktop -h
    } elseif (Get-Command choco -ErrorAction SilentlyContinue) {
        choco install docker-desktop -y
    } else {
        Write-Warning "Please install Docker Desktop manually: https://www.docker.com/products/docker-desktop"
    }
}

function Install-Docker-Linux {
    if (Get-Command docker -ErrorAction SilentlyContinue) { return }
    sudo apt-get update
    sudo apt-get install -y docker.io
    sudo systemctl enable --now docker
}

function Install-Docker-Mac {
    if (Get-Command docker -ErrorAction SilentlyContinue) { return }
    if (Get-Command brew -ErrorAction SilentlyContinue) {
        brew install --cask docker
    } else {
        Write-Warning "Homebrew not found. Install Docker Desktop from https://www.docker.com/products/docker-desktop"
    }
}

if ([System.Runtime.InteropServices.RuntimeInformation]::IsOSPlatform([System.Runtime.InteropServices.OSPlatform]::Windows)) {
    Install-DotNet
    Install-Docker-Windows
} elseif ([System.Runtime.InteropServices.RuntimeInformation]::IsOSPlatform([System.Runtime.InteropServices.OSPlatform]::Linux)) {
    Install-DotNet
    Install-Docker-Linux
} elseif ([System.Runtime.InteropServices.RuntimeInformation]::IsOSPlatform([System.Runtime.InteropServices.OSPlatform]::OSX)) {
    Install-DotNet
    Install-Docker-Mac
}

docker pull rabbitmq:3-management
Write-Host "Environment ready. Run 'dotnet test Ark.Alliance.Core.Mediator.sln'"
