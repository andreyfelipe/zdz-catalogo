using Microsoft.EntityFrameworkCore;
using CatalogoApi.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

//Configuração do entity framework com SQL Server
builder.Services.AddDbContext<ContextoBancoDados>(opcoes =>
    opcoes.UseSqlServer(
        builder.Configuration.GetConnectionString("ConexaoPadrao")
    )
);




// Configuração do CORS para permitir requisições de qualquer origem
builder.Services.AddCors(opcoes =>
    opcoes.AddPolicy("PoliticaFrontend", politica =>
        politica.WithOrigins("http://localhost:3000", "http://localhost:3001")
                .AllowAnyHeader().AllowAnyMethod()));

// Configuração do MVC para permitir o uso de controllers
builder.Services.AddControllers().AddJsonOptions(opcoes =>
    opcoes.JsonSerializerOptions.ReferenceHandler =
        System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();



//pipeline de requisições
var app = builder.Build();
app.UseSwagger();
app.UseSwaggerUI();
app.UseCors("PoliticaFrontend");
app.UseAuthorization();
app.MapControllers();
app.Run();
