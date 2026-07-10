-- =============================================================
-- Migração: adiciona tabela TBFREQUENCIA ao banco existente
-- Execute com isql no banco PROJETOEM.FB5 já criado:
--   isql -user SYSDBA -password masterkey "caminho/PROJETOEM.FB5"
--   SQL> INPUT 'migrar_frequencia.sql';
-- =============================================================

CREATE TABLE TBFREQUENCIA (
    FREQCODIGO     INTEGER      NOT NULL,
    FREQALUNO      INTEGER      NOT NULL,
    FREQDATA       INTEGER      NOT NULL,
    FREQPRESENTE   INTEGER      NOT NULL,
    FREQOBSERVACAO VARCHAR(200),
    CONSTRAINT PK_TBFREQUENCIA PRIMARY KEY (FREQCODIGO),
    CONSTRAINT FK_FREQ_ALUNO FOREIGN KEY (FREQALUNO) REFERENCES TBALUNO (ALUNMATRICULA)
);

CREATE SEQUENCE GEN_TBFREQUENCIA;
ALTER SEQUENCE GEN_TBFREQUENCIA RESTART WITH 1;

COMMIT;
