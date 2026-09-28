-- Rodar scripts em ordem.
IF DB_ID(N'CobrancaWeb') IS NULL
    EXEC(N'CREATE DATABASE CobrancaWeb');
GO
USE CobrancaWeb;
GO
