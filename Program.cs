using System.Diagnostics;
using System.Web.Http.Results;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using TodoApi;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;


var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<ToDoDbContext>();
builder.Services.AddDbContext<usersContext>();


builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", builder =>
        {
            builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
        });
});
var app = builder.Build();
app.UseCors("AllowAll");

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("http://localhost:5094/swagger/v1/swagger.json", "v1");
    options.RoutePrefix = string.Empty;
});

app.MapGet("/", () => "Hello World!");

app.MapGet("/item", async (usersContext context) =>
{
    var items = await context.Items.ToListAsync();
    return items;
});
app.MapPost("/item", async (usersContext context, Item newItem) =>
{
    context.Items.Add(newItem);
    await context.SaveChangesAsync();
    return newItem;

    // return Results.Created($"/item/{newItem.Id}", newItem);
});
app.MapPost("/register/{username}/{password}", async (usersContext context, string username, string password) =>
{
    var lastId = context.Users?.Max(u => u.Id) ?? 0;
    var user = new User { Id = lastId + 1, Username = username, Password = password };
    context.Users.Add(user);
    await context.SaveChangesAsync();
    return user;
});
app.MapPost("/login/{username}/{password}", async (usersContext context, string username, string password) =>
{
    var user = await context.Users.FirstOrDefaultAsync(u => u.Username == username && u.Password == password);
    if (user is not null)
        return Results.Ok();
    else
        return Results.NotFound();
    //  {
    //      var jwt = CreateJWT(user);
    //      AddSession(user);
    //      return Ok(jwt);
    //  }
    //  return Unauthorized();
});

app.MapPut("/item/{id}", async (int id, ToDoDbContext context, Item updatedItem) =>
{
    var Item = await context.Items.FindAsync(id);
    if (Item is null)
        return Results.NotFound();

    Item.Name = updatedItem.Name;
    Item.IsComplete = updatedItem.IsComplete;

    await context.SaveChangesAsync();

    return Results.Ok();
});
app.MapDelete("/item/{id}", async (int id, ToDoDbContext context) =>
{
    var Item = await context.Items.FindAsync(id);
    if (Item is null)
        return Results.NotFound();

    context.Items.Remove(Item);
    await context.SaveChangesAsync();

    return Results.NoContent();
});
//  private object CreateJWT(User user)
//  {
//      var claims = new List<Claim>()
//          {
//              new Claim("id", user.Id.ToString()),
//              new Claim("name", user.Username),
//              new Claim("pass", user.Password),
//          };

//      var secretKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration.GetValue<string>("JWT:Key")));
//      var signinCredentials = new SigningCredentials(secretKey, SecurityAlgorithms.HmacSha256);
//      var tokeOptions = new JwtSecurityToken(
//          issuer: _configuration.GetValue<string>("JWT:Issuer"),
//          audience: _configuration.GetValue<string>("JWT:Audience"),
//          claims: claims,
//          expires: DateTime.Now.AddDays(30),
//          signingCredentials: signinCredentials
//      );
//      var tokenString = new JwtSecurityTokenHandler().WriteToken(tokeOptions);
//      return new { Token = tokenString };
//  }

//  private void AddSession(User user)
//  {
//      var lastId = _dataContext.Sessions?.Max(u => u.Id) ?? 0;
//      _dataContext.Sessions?.Add(new Session { Id = lastId + 1, UserId = user.Id, DateTime = DateTime.Now.ToString(), IP = Request.HttpContext.Connection.RemoteIpAddress.ToString(), IsValid = true });
//  }
//------------------------------------------------------


// public void ConfigureServices(IServiceCollection services)
// {
//     // Configure JWT authentication
//     var key = Encoding.ASCII.GetBytes("your-secret-key"); // Replace with your own secret key
//     services.AddAuthentication(options =>
//     {
//         options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
//         options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
//     })
//     .AddJwtBearer(options =>
//     {
//         options.RequireHttpsMetadata = false; // Change to true in production
//         options.SaveToken = true;
//         options.TokenValidationParameters = new TokenValidationParameters
//         {
//             ValidateIssuer = true,
//             ValidateAudience = true,
//             ValidateIssuerSigningKey = true,
//             ValidIssuer = "your-issuer", // Replace with your own issuer
//             ValidAudience = "your-audience", // Replace with your own audience
//             IssuerSigningKey = new SymmetricSecurityKey(key)
//         };
//     });

//     // Other service configurations...
// }

// public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
// {
//     // Other app configurations...

//     // Add JWT authentication middleware
//     app.UseAuthentication();

//     // Other middleware configurations...
// }

//------------------------------------------------------
app.Run();
