-- Script d'initialisation de la base de données UserDB
-- Ce script est exécuté automatiquement au premier démarrage du conteneur PostgreSQL

-- La base UserDB est déjà créée par POSTGRES_DB dans docker-compose.yml
-- Connexion à UserDB
\c UserDB;

-- Création de la table Users
CREATE TABLE IF NOT EXISTS "Users" (
    "Id" UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    "Email" VARCHAR(256) NOT NULL,
    "PasswordHash" VARCHAR(512) NOT NULL,
    "FirstName" VARCHAR(100),
    "LastName" VARCHAR(100),
    "Role" VARCHAR(50) DEFAULT 'user',
    "CreatedAt" TIMESTAMP(3) NOT NULL DEFAULT NOW(),
    "UpdatedAt" TIMESTAMP(3),
    CONSTRAINT "Users_Email_key" UNIQUE ("Email")
);

-- Création des index pour améliorer les performances
CREATE INDEX IF NOT EXISTS "IX_Users_Email" ON "Users"("Email");
CREATE INDEX IF NOT EXISTS "IX_Users_Role" ON "Users"("Role");

-- Message de confirmation
DO $$
BEGIN
    RAISE NOTICE 'Database initialization completed successfully!';
END $$;