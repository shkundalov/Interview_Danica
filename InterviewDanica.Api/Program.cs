using InterviewDanica.Api.Data;
using InterviewDanica.Api.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<DBContext>(options =>
    options.UseSqlite(connectionString)
);

builder.Services.AddScoped<ICustomerService,CustomerService>();
builder.Services.AddScoped<ITemplateService,TemplateService>();
builder.Services.AddScoped<IIdempotencyService,IdempotencyService>();
builder.Services.AddScoped<ICommunicationService,CommunicationService>();
builder.Services.AddScoped<LoginService>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var JWTConfig = builder.Configuration.GetSection("Jwt");
builder.Services.AddAuthentication("Bearer").AddJwtBearer("Bearer",options => {
        options.TokenValidationParameters = new TokenValidationParameters {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = JWTConfig["Issuer"],
            ValidAudience = JWTConfig["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey( Encoding.UTF8.GetBytes(JWTConfig["Key"]!) )
        };
    });

builder.Services.AddAuthorization();


var app = builder.Build();

if (app.Environment.IsDevelopment()) {
    app.UseSwagger();
    app.UseSwaggerUI(options => {
        options.SwaggerEndpoint("/swagger/v1/swagger.json","Interview Danica API V1");
        options.RoutePrefix = "swagger";
    });
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

using (var scope = app.Services.CreateScope()) {
    var dbContext = scope.ServiceProvider.GetRequiredService<DBContext>();
    dbContext.Database.EnsureCreated();
    Console.WriteLine("Database connected");
}
app.Run();