using bbApi.App.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Identity.Web;
using Microsoft.Extensions.Configuration.UserSecrets;
using System.Reflection;

var initialScopes = new string[] {  };

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// builder.Services.AddAuthentication(OpenIdConnectDefaults.AuthenticationScheme)
//               .AddMicrosoftIdentityWebApp(builder.Configuration.GetSection("AzureAd"))
//                   .EnableTokenAcquisitionToCallDownstreamApi(options => builder.Configuration.Bind("AzureAd", options), initialScopes);

builder.Services.AddAuthentication()
          .AddMicrosoftIdentityWebApi(builder.Configuration.GetSection("AzureAd"),
                                      JwtBearerDefaults.AuthenticationScheme)
          .EnableTokenAcquisitionToCallDownstreamApi()
          .AddMicrosoftGraph(builder.Configuration.GetSection("GraphApi"))
          .AddInMemoryTokenCaches();

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddScoped<IADGraphService, ADGraphService>();

builder.Services.AddAutoMapper(typeof(Program));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

//app.UseAuthentication();
//app.UseAuthorization();

app.MapControllers();

app.Run();
