using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.Filters;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
    {
        Description = "Standard Authorization header using the Bearer Scheme (\"bearer {token}\")",
        In = ParameterLocation.Header,
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey
    });

    options.OperationFilter<SecurityRequirementsOperationFilter>(); //Install the nugret package Swashbuckle.AspNetCore.Filters: developed by Matt Frear (Kudos to Matt)
});
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme) //This is the Authentication Scheme, Add/reference: using Microsoft.AspNet.Core.Authentication.JwtBearer package
    .AddJwtBearer(options => {
        options.TokenValidationParameters = new TokenValidationParameters  //Make sure and add/reference: using Microsoft.IdentityModel.Tokens...
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8
                .GetBytes(builder.Configuration.GetSection("AppSettings:Token").Value)),
            ValidateIssuer = false,
            ValidateAudience = false

        };
    });

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();        // Add this middleware. Also make sure this is above the app.UseAuthorization();

app.UseAuthorization();

app.MapControllers();

app.Run();
