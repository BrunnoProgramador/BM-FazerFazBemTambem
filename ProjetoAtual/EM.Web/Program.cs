using EM.Domain.Fachadas;
using EM.Domain.Interfaces;
using EM.Repository;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc.Authorization;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("Postgres");

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

// Add services to the container.
builder.Services.AddControllersWithViews(opcoes =>
{
    // Exige usuário autenticado em todos os controllers por padrão
    opcoes.Filters.Add(new AuthorizeFilter());
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
