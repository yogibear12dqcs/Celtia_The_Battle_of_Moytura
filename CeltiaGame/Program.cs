var builder = WebApplication.CreateBuilder(args);

var app = builder.Build();

// Serve index.html as the default document at "/"
app.UseDefaultFiles();

// Serve all static assets (PNG, WAV, JS, CSS, etc.) from wwwroot/
app.UseStaticFiles();

app.Run();

// Make the implicit Program class visible to WebApplicationFactory in tests
public partial class Program { }
