var builder = WebApplication.CreateBuilder(args);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.Map("/appDllLocation", async (HttpContext context) =>
{
    await context.Response.WriteAsync("Location: " + typeof(Program).Assembly.Location);
});

app.MapGet("/", async (HttpContext context) =>
{
    await context.Response.WriteAsync(Greeter.Greeting.Get() + " from WebApp1");
});

app.Run();
