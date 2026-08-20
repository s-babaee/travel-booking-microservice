-- =========================================================
-- Keycloak database
-- =========================================================

CREATE USER keycloak WITH PASSWORD 'keycloak_password';
CREATE DATABASE keycloak OWNER keycloak;

\connect keycloak

GRANT ALL ON SCHEMA public TO keycloak;


-- =========================================================
-- Hotel database
-- =========================================================

CREATE DATABASE hotel OWNER auth;

\connect hotel

GRANT ALL ON SCHEMA public TO auth;
