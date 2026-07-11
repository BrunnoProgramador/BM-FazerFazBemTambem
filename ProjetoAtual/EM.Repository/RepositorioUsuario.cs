using EM.Domain;
using EM.Domain.Interfaces;
using Npgsql;

namespace EM.Repository;

public class RepositorioUsuario : RepositorioAbstrato<Usuario>, IRepositorioUsuario
{
    private const string Colunas = "USUCODIGO, USULOGIN, USUNOME, USUHASH, USUADMIN, USUTROCARSENHA";

    private static bool _migrado;
    private static readonly object _travaMigracao = new();

    public RepositorioUsuario(string connectionString) : base(connectionString)
    {
        GarantirColunasNovas();
    }

    /// <summary>
    /// Automigração (roda uma única vez por processo): adiciona as colunas
    /// de administrador e troca de senha, e promove o usuário mais antigo a
    /// administrador caso nenhum exista. Nenhum script manual no Neon.
    /// </summary>
    private void GarantirColunasNovas()
    {
        if (_migrado) return;
        lock (_travaMigracao)
        {
            if (_migrado) return;

            using var conexao = CriarConexao();
            using var comando = conexao.CreateCommand();
            comando.CommandText = @"
                ALTER TABLE TBUSUARIO ADD COLUMN IF NOT EXISTS USUADMIN BOOLEAN NOT NULL DEFAULT FALSE;
                ALTER TABLE TBUSUARIO ADD COLUMN IF NOT EXISTS USUTROCARSENHA BOOLEAN NOT NULL DEFAULT FALSE;
                UPDATE TBUSUARIO SET USUADMIN = TRUE
                 WHERE USUCODIGO = (SELECT MIN(USUCODIGO) FROM TBUSUARIO)
                   AND NOT EXISTS (SELECT 1 FROM TBUSUARIO WHERE USUADMIN);";
            comando.ExecuteNonQuery();

            _migrado = true;
        }
    }

    public Usuario? GetByLogin(string login)
    {
        return Consultar(
            $"SELECT {Colunas} FROM TBUSUARIO WHERE USULOGIN = @login",
            Mapear,
            p => p.Add(new NpgsqlParameter("@login", login))).FirstOrDefault();
    }

    public Usuario? GetByCodigo(int codigo)
    {
        return Consultar(
            $"SELECT {Colunas} FROM TBUSUARIO WHERE USUCODIGO = @codigo",
            Mapear,
            p => p.Add(new NpgsqlParameter("@codigo", codigo))).FirstOrDefault();
    }

    public Usuario? ObterAdministrador()
    {
        return Consultar(
            $"SELECT {Colunas} FROM TBUSUARIO WHERE USUADMIN ORDER BY USUCODIGO LIMIT 1",
            Mapear).FirstOrDefault();
    }

    public int Contar()
    {
        return Convert.ToInt32(ExecutarEscalar("SELECT COUNT(*) FROM TBUSUARIO", _ => { }));
    }

    public int Adicionar(Usuario entidade)
    {
        var codigo = ExecutarEscalar(
            "INSERT INTO TBUSUARIO (USUCODIGO, USULOGIN, USUNOME, USUHASH, USUADMIN, USUTROCARSENHA) " +
            "VALUES (nextval('gen_tbusuario'), @login, @nome, @hash, @admin, @trocar) RETURNING USUCODIGO",
            p =>
            {
                p.Add(new NpgsqlParameter("@login",  entidade.Login));
                p.Add(new NpgsqlParameter("@nome",   entidade.Nome));
                p.Add(new NpgsqlParameter("@hash",   entidade.Hash));
                p.Add(new NpgsqlParameter("@admin",  entidade.EhAdministrador));
                p.Add(new NpgsqlParameter("@trocar", entidade.DeveTrocarSenha));
            });
        return Convert.ToInt32(codigo);
    }

    public override void Atualizar(Usuario entidade)
    {
        ExecutarComando(
            "UPDATE TBUSUARIO SET USUNOME = @nome, USUHASH = @hash, USUTROCARSENHA = @trocar " +
            "WHERE USUCODIGO = @codigo",
            p =>
            {
                p.Add(new NpgsqlParameter("@nome",   entidade.Nome));
                p.Add(new NpgsqlParameter("@hash",   entidade.Hash));
                p.Add(new NpgsqlParameter("@trocar", entidade.DeveTrocarSenha));
                p.Add(new NpgsqlParameter("@codigo", entidade.Codigo));
            });
    }

    public override void Excluir(int codigo)
    {
        ExecutarComando(
            "DELETE FROM TBUSUARIO WHERE USUCODIGO = @codigo",
            p => p.Add(new NpgsqlParameter("@codigo", codigo)));
    }

    public override IEnumerable<Usuario> ObterTodos()
    {
        return Consultar(
            $"SELECT {Colunas} FROM TBUSUARIO ORDER BY USUNOME",
            Mapear);
    }

    private static Usuario Mapear(NpgsqlDataReader reader) => new()
    {
        Codigo          = reader.GetInt32(0),
        Login           = reader.GetString(1),
        Nome            = reader.GetString(2),
        Hash            = reader.GetString(3),
        EhAdministrador = reader.GetBoolean(4),
        DeveTrocarSenha = reader.GetBoolean(5)
    };
}
