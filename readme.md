## How to start

* Install `Forte.Styleguide.EPiServer` NuGet package
* In Program.cs (make sure to have `#using Forte.Styleguide.EPiServer;`)
```cs
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddStyleGuideEpiServer();

// Removed for brevity

var app = builder.Build();

// Removed for brevity

app.Run();
```

Alternatively if you prefer to use `Startup.cs` file:
```cs
public class Startup
{
    private readonly IWebHostEnvironment _webHostingEnvironment;

    public Startup(IWebHostEnvironment webHostingEnvironment)
    {
        _webHostingEnvironment = webHostingEnvironment;
    }

    public void ConfigureServices(IServiceCollection services)
    {

        services.AddStyleGuideEpiServer();
                
        // Removed for brevity
    }


    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }
    
        // Removed for brevity
    }
}
```
## Customize blocks path

By default blocks should be placed in `Features` folder.
If you want to change the folder with blocks you should write in `Program.cs`:
```cs
builder.Services.AddStyleguideEpiServer("PathToYourFolder")
```

If you use `Startup.cs` file then:
```cs
services.AddStyleguideEpiServer("PathToYourFolder")
```