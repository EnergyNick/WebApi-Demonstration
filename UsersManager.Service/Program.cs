using FluentValidation;
using Microsoft.AspNetCore.Authentication;
using UsersManager.Domain;
using UsersManager.Domain.Extensions;
using UsersManager.Infrastructure;
using UsersManager.Infrastructure.Extensions;
using UsersManager.Service.Extensions;
using UsersManager.Service.HostUtilities;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDomainServices();
builder.Services.AddOptionWithValidate<ServicesSettings>(ServicesSettings.SectionName);

builder.Services.AddDataBaseInfrastructure();
builder.Services.AddOptionWithValidate<DataBaseSettings>(DataBaseSettings.SectionName);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c => c.UseInlineDefinitionsForEnums());

builder.Services.AddValidatorsFromAssemblyContaining(typeof(Program));
builder.Services.AddAutoMapper(typeof(Program), typeof(DataBaseSettings), typeof(ServicesSettings));
builder.AddLoggingProvider();


var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<ExceptionHandlerMiddleware>();

app.UseHttpsRedirection();

app.UseCors();

app.MapControllers();

app.Run();