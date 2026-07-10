using EM.Domain;
using EM.Domain.Interfaces;
using Npgsql;

namespace EM.Repository;

public class RepositorioUsuario : RepositorioAbstrato<Usuario>, IRepositorioUsuario
{
    public RepositorioUsuario(string connectionString) : base(connectionString) { }

    public Usuario? GetByLogin(string login)
    {
        return Consultar(
            "SELECT USUCODIGO, USULOGIN, USUNOME, USUHASH FROM TBUSUARIO WHERE USULOGIN = @login",
            Mapear,
            p => p.Add(new NpgsqlParameter("@login", login))).FirstOrDefault();
    }

    public int Contar()
    {
        return Convert.ToInt32(ExecutarEscalar("SELECT COUNT(*) FROM TBUSUARIO", _ => { }));
    }

    public int Adicionar(Usuario entidade)
    {
        var codigo = ExecutarEscalar(
            "INSERT INTO TBUSUARIO (USUCODIGO, USULOGIN, USUNOME, USUHASH) " +
            "VALUES (nextval('gen_tbusuario'), @login, @nome, @hash) RETURNING USUCODIGO",
            p =>
            {
                p.Add(new NpgsqlParameter("@login", entidade.Login));
                p.Add(new NpgsqlParameter("@nome",  entidade.Nome));
                p.Add(new NpgsqlParameter("@hash",  entidade.Hash));
            });
        return Convert.ToInt32(codigo);
    }

    public override void Atualizar(Usuario entidade)
    {
        ExecutarComando(
            "UPDATE TBUSUARIO SET USUNOME = @nome, USUHASH = @hash WHERE USUCODIGO = @codigo",
            p =>
            {
                p.Add(new NpgsqlParameter("@nome",   entidade.Nome));
                p.Add(new NpgsqlParameter("@hash",   entidade.Hash));
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
            "SELECT USUCODIGO, USULOGIN, USUNOME, USUHASH FROM TBUSUARIO ORDER BY USUNOME",
            Mapear);
    }

    private static Usuario Mapear(NpgsqlDataReader reader) => new()
    {
        Codigo = reader.GetInt32(0),
        Login  = reader.GetString(1),
        Nome   = reader.GetString(2),
        Hash   = reader.GetString(3)
    };
}
