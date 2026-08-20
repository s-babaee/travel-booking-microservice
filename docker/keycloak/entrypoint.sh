#!/usr/bin/env sh
set -eu

: "${KEYCLOAK_CLIENT_SECRET:?KEYCLOAK_CLIENT_SECRET is required}"
: "${TRAVEL_ADMIN_PASSWORD:?TRAVEL_ADMIN_PASSWORD is required}"

template="/opt/keycloak/data/import-template/travel-realm.template.json"
realm_file="/opt/keycloak/data/import/travel-realm.json"

mkdir -p "$(dirname "$realm_file")"

escape_sed_replacement() {
  printf '%s' "$1" | sed 's/[\\&|]/\\&/g'
}

client_secret="$(escape_sed_replacement "$KEYCLOAK_CLIENT_SECRET")"
travel_admin_password="$(escape_sed_replacement "$TRAVEL_ADMIN_PASSWORD")"

sed \
  -e "s|__KEYCLOAK_CLIENT_SECRET__|${client_secret}|g" \
  -e "s|__TRAVEL_ADMIN_PASSWORD__|${travel_admin_password}|g" \
  "$template" > "$realm_file"

exec /opt/keycloak/bin/kc.sh start-dev --import-realm
