var builder = WebApplication.CreateBuilder(args);

// Registrando os controllers.
builder.Services.AddControllers();

// Descrição dos endpoints para a documentação.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    // Swagger.
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Endpoints
app.UseAuthorization();
app.MapControllers();

app.Run();
