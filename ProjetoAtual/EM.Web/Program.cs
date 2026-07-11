using EM.Domain.Fachadas;
using EM.Domain.Interfaces;
using EM.Repository;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc.Authorization;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("Postgres");

// Apara espaços, quebras de linha e aspas que vêm de acidentes de
// copiar/colar no painel de variáveis de ambiente.
connectionString = connectionString?.Trim().Trim('"', '\'').Trim();

// Aceita tanto o formato keyword ("Host=...;Username=...") quanto a URL
// que o Neon/Vercel fornecem ("postgresql://usuario:senha@host/banco?...").
// O Npgsql só entende o primeiro; aqui convertemos quando vier URL.
if (connectionString != null &&
    (connectionString.StartsWith("postgres://", StringComparison.OrdinalIgnoreCase) ||
     connectionString.StartsWith("postgresql://", StringComparison.OrdinalIgnoreCase)))
{
    var uri = new Uri(connectionString);
    var dadosUsuario = uri.UserInfo.Split(':', 2);

    connectionString = new Npgsql.NpgsqlConnectionStringBuilder
    {
        Host = uri.Host,
        Port = uri.Port > 0 ? uri.Port : 5432,
        Database = uri.AbsolutePath.TrimStart('/'),
        Username = Uri.UnescapeDataString(dadosUsuario[0]),
        Password = dadosUsuario.Length > 1 ? Uri.UnescapeDataString(dadosUsuario[1]) : "",
        SslMode = Npgsql.SslMode.Require
    }.ConnectionString;
}

// Chaves de criptografia de cookie persistidas no Postgres: sem isso,
// cada instância serverless do Vercel gera chaves próprias e os logins
// caem aleatoriamente (a tabela é criada sozinha na subida).
builder.Services.AddDataProtection()
    .SetApplicationName("FazerBemFazBemTambem")
    .AddKeyManagementOptions(opcoes =>
        opcoes.XmlRepository = new EM.Web.Infraestrutura.RepositorioChavesDataProtection(connectionString!));

// Autenticação por cookie: toda tela exige login, exceto as marcadas
// com [AllowAnonymous] (login e configuração inicial).
builder.Services
    .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(opcoes =>
    {
        opcoes.LoginPath = "/Conta/Login";
        opcoes.AccessDeniedPath = "/Conta/Login";
        opcoes.ExpireTimeSpan = TimeSpan.FromHours(8);
        opcoes.SlidingExpiration = true;
        opcoes.Cookie.Name = "FazerBemFazBemTambem.Auth";
        opcoes.Cookie.HttpOnly = true;
        opcoes.Cookie.SameSite = SameSiteMode.Lax;
    });

// Política de administrador (gestão de usuários nas Configurações)
builder.Services.AddAuthorization(opcoes =>
{
    opcoes.AddPolicy("SomenteAdministrador", politica => politica.RequireClaim("admin", "1"));
});

// Add services to the container.
builder.Services.AddControllersWithViews(opcoes =>
{
    // Exige usuário autenticado em todos os controllers por padrão
    opcoes.Filters.Add(new AuthorizeFilter());

    // Senha provisória: trava a navegação até o usuário trocar a senha
    opcoes.Filters.Add(new EM.Web.Infraestrutura.FiltroTrocaSenhaObrigatoria());
});
builder.Services.AddScoped<IRepositorioAluno>(sp => new RepositorioAluno(connectionString));
builder.Services.AddScoped<IRepositorioTurma>(sp => new RepositorioTurma(connectionString));
builder.Services.AddScoped<IRepositorioEncontro>(sp => new RepositorioEncontro(connectionString));
builder.Services.AddScoped<IRepositorioEvento>(sp => new RepositorioEvento(connectionString));
builder.Services.AddScoped<IRepositorioVoluntario>(sp => new RepositorioVoluntario(connectionString));
builder.Services.AddScoped<IRepositorioDoacao>(sp => new RepositorioDoacao(connectionString));
builder.Services.AddScoped<IRepositorioUsuario>(sp => new RepositorioUsuario(connectionString));
builder.Services.AddScoped<FachadaAluno>();
builder.Services.AddScoped<FachadaTurma>();
builder.Services.AddScoped<FachadaEncontro>();
builder.Services.AddScoped<FachadaEvento>();
builder.Services.AddScoped<FachadaVoluntario>();
builder.Services.AddScoped<FachadaDoacao>();
builder.Services.AddScoped<FachadaUsuario>();

var app = builder.Build();

// NOTA: a auto-migração de schema (MigradorBanco) era específica do Firebird
// (consultava RDB$RELATIONS/RDB$GENERATORS, que não existem no Postgres).
// Como o banco no Neon nasce vazio, o schema foi criado uma única vez rodando
// schema_postgres.sql direto no editor SQL do Neon. Se no futuro quiser
// reativar uma verificação automática de schema, ela precisa ser reescrita
// usando information_schema.tables / information_schema.columns (Postgres).

// Atrás de um proxy reverso (Caddy/Nginx), respeita X-Forwarded-Proto/For
// para que cookies e redirecionamentos enxerguem HTTPS corretamente.
var opcoesProxy = new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
};
opcoesProxy.KnownNetworks.Clear();
opcoesProxy.KnownProxies.Clear();
app.UseForwardedHeaders(opcoesProxy);

// Cabeçalhos de segurança em todas as respostas
app.Use(async (contexto, proximo) =>
{
    contexto.Response.Headers["X-Content-Type-Options"] = "nosniff";
    contexto.Response.Headers["X-Frame-Options"] = "DENY";
    contexto.Response.Headers["Referrer-Policy"] = "same-origin";
    await proximo();
});

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Dashboard/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Dashboard}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
