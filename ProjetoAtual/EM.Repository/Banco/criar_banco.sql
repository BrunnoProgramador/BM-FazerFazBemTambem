-- =============================================================
-- Script de criação do banco de dados - ProjetoEM (Firebird 5)
-- Execute este script após criar um banco vazio com isql:
--   isql -user SYSDBA -password masterkey
--   SQL> CREATE DATABASE 'caminho/PROJETOEM.FB5';
--   SQL> INPUT 'criar_banco.sql';
-- =============================================================

-- -------------------------------------------------------------
-- Tabela: TBCIDADE
-- -------------------------------------------------------------
CREATE TABLE TBCIDADE (
    CIDACODIGO   INTEGER      NOT NULL,
    CIDADESCRICAO VARCHAR(100) NOT NULL,
    CIDAUF       CHAR(2)      NOT NULL,
    CONSTRAINT PK_TBCIDADE PRIMARY KEY (CIDACODIGO)
);

-- Sequence para auto-incremento de TBCIDADE
CREATE SEQUENCE GEN_TBCIDADE;
ALTER SEQUENCE GEN_TBCIDADE RESTART WITH 1;

-- -------------------------------------------------------------
-- Tabela: TBALUNO
-- -------------------------------------------------------------
CREATE TABLE TBALUNO (
    ALUNMATRICULA INTEGER       NOT NULL,
    ALUNNOME      VARCHAR(150)  NOT NULL,
    ALUNCPF       VARCHAR(14),
    ALUNDTNASC    INTEGER       NOT NULL,  -- formato YYYYMMDD
    ALUNSEXO      INTEGER       NOT NULL,  -- 1 = Masculino, 2 = Feminino
    ALUNCIDADE    INTEGER       NOT NULL,
    CONSTRAINT PK_TBALUNO PRIMARY KEY (ALUNMATRICULA),
    CONSTRAINT FK_ALUNO_CIDADE FOREIGN KEY (ALUNCIDADE) REFERENCES TBCIDADE (CIDACODIGO)
);

-- Sequence para auto-incremento de TBALUNO
CREATE SEQUENCE GEN_TBALUNO;
ALTER SEQUENCE GEN_TBALUNO RESTART WITH 1;

-- -------------------------------------------------------------
-- Tabela: TBFREQUENCIA
-- -------------------------------------------------------------
CREATE TABLE TBFREQUENCIA (
    FREQCODIGO     INTEGER      NOT NULL,
    FREQALUNO      INTEGER      NOT NULL,
    FREQDATA       INTEGER      NOT NULL,  -- formato YYYYMMDD
    FREQPRESENTE   INTEGER      NOT NULL,  -- 1 = Presente, 0 = Ausente
    FREQOBSERVACAO VARCHAR(200),
    CONSTRAINT PK_TBFREQUENCIA PRIMARY KEY (FREQCODIGO),
    CONSTRAINT FK_FREQ_ALUNO FOREIGN KEY (FREQALUNO) REFERENCES TBALUNO (ALUNMATRICULA)
);

-- Sequence para auto-incremento de TBFREQUENCIA
CREATE SEQUENCE GEN_TBFREQUENCIA;
ALTER SEQUENCE GEN_TBFREQUENCIA RESTART WITH 1;

COMMIT;
