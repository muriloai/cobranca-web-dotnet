var builder = WebApplication.CreateBuilder(args);

// Registrando os controllers.
builder.Services.AddControllers();

// Descrição dos endpoints para a documentação.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    // Exibe a documentação swagger.
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Aplica as políticas de autorização definidas nos endpoints.
app.UseAuthorization();

// Associa as rotas dos controllers ao pipeline HTTP.
app.MapControllers();

app.Run();
