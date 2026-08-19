CREATE USER keycloak WITH PASSWORD 'keycloak_password';
CREATE DATABASE keycloak OWNER keycloak;

\connect keycloak

GRANT ALL ON SCHEMA public TO keycloak;
