-- =============================================================
-- Migração: adiciona professor, dias da semana e horário à TBTURMA
-- Execute com isql no banco PROJETOEM.FB5 já criado:
--   isql -user SYSDBA -password masterkey "caminho/PROJETOEM.FB5"
--   SQL> INPUT 'migrar_turma_detalhes.sql';
-- =============================================================

ALTER TABLE TBTURMA ADD TURMPROFESSOR VARCHAR(100);
ALTER TABLE TBTURMA ADD TURMDIAS      VARCHAR(60);
ALTER TABLE TBTURMA ADD TURMHORARIO   VARCHAR(20);

COMMIT;
