#!/usr/bin/env bash
# Updates the installed Security Monitor service from the latest code on GitHub.
# Run it as your normal user from anywhere:  ./deploy/linux/update.sh
# It asks for your sudo password only for the steps that need root.

set -euo pipefail

SERVICE=securitymonitor
INSTALL_DIR=/opt/securitymonitor
UNIT_FILE=/etc/systemd/system/$SERVICE.service

# Work from the repository root, wherever the script was started from.
cd "$(dirname "$0")/../.."

if [[ $EUID -eq 0 ]]; then
    echo "Run this as your normal user, not with sudo. It uses sudo itself where needed." >&2
    exit 1
fi

echo "==> Pulling the latest code"
git pull --ff-only

echo "==> Running the tests"
dotnet test SecurityMonitor.Linux.slnf --nologo --verbosity quiet

echo "==> Publishing"
rm -rf ./publish
dotnet publish src/SecurityMonitor.Host.Linux -c Release -o ./publish --nologo --verbosity quiet

echo "==> Stopping the service"
sudo systemctl stop "$SERVICE"

echo "==> Installing to $INSTALL_DIR"
sudo cp -r ./publish/. "$INSTALL_DIR/"

if ! sudo cmp -s deploy/linux/$SERVICE.service "$UNIT_FILE"; then
    echo "==> The systemd unit changed, installing it"
    sudo cp deploy/linux/$SERVICE.service "$UNIT_FILE"
    sudo systemctl daemon-reload
fi

echo "==> Starting the service"
sudo systemctl start "$SERVICE"
sleep 5

if systemctl is-active --quiet "$SERVICE"; then
    echo "==> Done. The service is running."
else
    echo "==> The service did not start. Last log lines:" >&2
fi

journalctl -u "$SERVICE" -n 10 --no-pager
systemctl is-active --quiet "$SERVICE"