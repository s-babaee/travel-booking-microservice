#!/usr/bin/env bash
set -Eeuo pipefail

create_database() {
    local database_name="$1"
    local database_user="$2"
    local database_password="$3"

    psql \
        --username "$POSTGRES_USER" \
        --dbname "$POSTGRES_DB" \
        --set ON_ERROR_STOP=on \
        --set database_name="$database_name" \
        --set database_user="$database_user" \
        --set database_password="$database_password" <<'SQL'
SELECT format(
    'CREATE ROLE %I LOGIN PASSWORD %L',
    :'database_user',
    :'database_password')
WHERE NOT EXISTS (
    SELECT 1
    FROM pg_roles
    WHERE rolname = :'database_user'
) \gexec

SELECT format(
    'CREATE DATABASE %I OWNER %I',
    :'database_name',
    :'database_user')
WHERE NOT EXISTS (
    SELECT 1
    FROM pg_database
    WHERE datname = :'database_name'
) \gexec
SQL

    psql \
        --username "$POSTGRES_USER" \
        --dbname "$POSTGRES_DB" \
        --set ON_ERROR_STOP=on \
        --set database_name="$database_name" \
        --set database_user="$database_user" <<'SQL'
SELECT format(
    'REVOKE CONNECT ON DATABASE %I FROM PUBLIC',
    :'database_name') \gexec

SELECT format(
    'GRANT CONNECT ON DATABASE %I TO %I',
    :'database_name',
    :'database_user') \gexec
SQL

    psql \
        --username "$POSTGRES_USER" \
        --dbname "$database_name" \
        --set ON_ERROR_STOP=on \
        --set database_user="$database_user" <<'SQL'
REVOKE ALL ON SCHEMA public FROM PUBLIC;

SELECT format(
    'GRANT USAGE, CREATE ON SCHEMA public TO %I',
    :'database_user') \gexec
SQL
}

create_database "$AUTH_DB_NAME" "$AUTH_DB_USER" "$AUTH_DB_PASSWORD"
create_database "$HOTEL_DB_NAME" "$HOTEL_DB_USER" "$HOTEL_DB_PASSWORD"
create_database "$BOOKING_DB_NAME" "$BOOKING_DB_USER" "$BOOKING_DB_PASSWORD"
create_database "$PAYMENT_DB_NAME" "$PAYMENT_DB_USER" "$PAYMENT_DB_PASSWORD"
create_database "$KEYCLOAK_DB_NAME" "$KEYCLOAK_DB_USER" "$KEYCLOAK_DB_PASSWORD"

# The administrative database is not a service database. Service identities
# must not be able to connect to it or to another service's database.
psql \
    --username "$POSTGRES_USER" \
    --dbname "$POSTGRES_DB" \
    --set ON_ERROR_STOP=on \
    --set database_name="$POSTGRES_DB" \
    --set auth_user="$AUTH_DB_USER" \
    --set hotel_user="$HOTEL_DB_USER" \
    --set booking_user="$BOOKING_DB_USER" \
    --set payment_user="$PAYMENT_DB_USER" \
    --set keycloak_user="$KEYCLOAK_DB_USER" <<'SQL'
SELECT format(
    'REVOKE CONNECT ON DATABASE %I FROM PUBLIC',
    :'database_name') \gexec

SELECT format(
    'REVOKE CONNECT ON DATABASE %I FROM %I, %I, %I, %I, %I',
    :'database_name',
    :'auth_user',
    :'hotel_user',
    :'booking_user',
    :'payment_user',
    :'keycloak_user') \gexec
SQL
