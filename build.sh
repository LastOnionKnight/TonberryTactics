#!/usr/bin/env bash
# Tonberry Tactics — Cloudflare Pages build script
#
# v0.5.3: install wasm-tools workload before publish.
# Belt-and-suspenders backup for the WasmFingerprintAssets=false csproj
# property. If the property doesn't fully suppress fingerprint template
# injection in this SDK version, wasm-tools will at least resolve the
# templates to real hashes so the site still ships.
set -euo pipefail

echo "▶ Installing .NET 10 SDK..."
curl -sSL https://dot.net/v1/dotnet-install.sh > dotnet-install.sh
chmod +x dotnet-install.sh
./dotnet-install.sh --channel 10.0 --install-dir ./dotnet10
export PATH="$PWD/dotnet10:$PATH"
./dotnet10/dotnet --version

echo "▶ Installing wasm-tools workload..."
./dotnet10/dotnet workload install wasm-tools --skip-manifest-update

echo "▶ Publishing Blazor WebAssembly app..."
./dotnet10/dotnet publish -c Release -o output

echo "▶ Build complete. Cloudflare Pages will serve from output/wwwroot/"
