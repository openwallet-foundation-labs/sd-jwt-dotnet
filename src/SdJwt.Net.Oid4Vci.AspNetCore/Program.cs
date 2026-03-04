using SdJwt.Net.Oid4Vci.AspNetCore.Endpoints;
using SdJwt.Net.Oid4Vci.AspNetCore.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Bind OID4VCI issuer options from configuration
builder.Services
    .AddOid4VciIssuer(options =>
        builder.Configuration.GetSection("Oid4Vci").Bind(options))
    .UseInMemoryServices();

// Health checks
builder.Services.AddHealthChecks()
    .AddOid4VciHealthChecks();

// OpenAPI / Swagger for development
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "OID4VCI Reference Server",
        Version = "v1",
        Description = "OpenID for Verifiable Credential Issuance (OID4VCI) 1.0 reference implementation."
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "OID4VCI v1"));
}

// Map OID4VCI endpoints
app.MapOid4VciMetadata();
app.MapOid4VciToken();
app.MapOid4VciCredential();
app.MapOid4VciDeferredCredential();

// Health checks
app.MapHealthChecks("/health");

app.Run();

/// <summary>
/// Required for WebApplicationFactory in integration tests.
/// </summary>
public partial class Program
{
}
