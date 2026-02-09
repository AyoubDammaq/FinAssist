-- Création de la base de données (optionnel si déjà créée par Docker)
-- CREATE DATABASE "TransactionDB";

-- Connexion à la base de données
\c "TransactionDB";

-- Création de la table Transactions
CREATE TABLE IF NOT EXISTS "Transactions" (
    "Id" SERIAL PRIMARY KEY,
    "Amount" DECIMAL(18,2) NOT NULL,
    "Currency" VARCHAR(3) NOT NULL,
    "Date" TIMESTAMP NOT NULL,
    "Description" TEXT,
    "Status" VARCHAR(20) NOT NULL
);

-- Création de la table Categories
CREATE TABLE IF NOT EXISTS "Categories" (
    "Id" UUID PRIMARY KEY,
    "Name" VARCHAR(100) NOT NULL,
    "UserId" UUID,
    "IsActive" BOOLEAN NOT NULL DEFAULT TRUE
);

-- Exemple d'index
CREATE INDEX IF NOT EXISTS idx_transactions_date ON "Transactions"("Date");