namespace EM.Repository;

/// <summary>
/// DESATIVADO na migração para Postgres.
///
/// Esta classe fazia a auto-criação/auto-migração do banco Firebird em
/// tempo de execução, consultando tabelas de sistema específicas do
/// Firebird (RDB$RELATIONS, RDB$GENERATORS) e criando o arquivo .FB5
/// localmente quando ele não existia — conceitos que não existem em um
/// servidor Postgres gerenciado como o Neon.
///
/// Como o banco no Neon nasce vazio, o schema completo (extraído desta
/// mesma classe) foi rodado uma única vez manualmente via schema_postgres.sql
/// no editor SQL do Neon. Não há mais chamada a MigradorBanco.Executar()
/// no Program.cs.
///
/// Se no futuro quiser reativar uma verificação automática de schema no
/// Postgres, ela precisa ser reescrita usando information_schema.tables e
/// information_schema.columns (equivalentes Postgres às tabelas RDB$ do
/// Firebird) — não é uma simples troca de tipos.
/// </summary>
public static class MigradorBanco
{
}
