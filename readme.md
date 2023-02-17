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

## Configure RazorViewEngine
To let Styleguide work properly you have to configure `RazorViewEngine`.
Assuming your blocks are placed in `~/Features/Parials/BlockName` path, 
for example: `Features/Partials/BarChart` then your controller should be named `BarChartController` and view name should be `BarChart.cshtml`.
To make it work well you should write in `Program.cs`(make sure you have `#using Microsoft.AspNetCore.Mvc.Razor;`):
```cs
builder.Services.Configure<RazorViewEngineOpions>(options => 
{
    options.ViewLocationFormats.Add("~/Features/Partials/{0}/{0}.cshtml");
});
```

If you use `Startup.cs`, then it would be:
```cs
services.Configure<RazorViewEngineOpions>(options => 
{
    options.ViewLocationFormats.Add("~/Features/Partials/{0}/{0}.cshtml");
});
```

## Customize blocks path

By default blocks should be placed in `~/Features` folder.
If you want to change the folder with blocks you should write in `Program.cs`:
```cs
builder.Services.AddStyleguideEpiServer("PathToYourFolder");
```

If you use `Startup.cs` file then:
```cs
services.AddStyleguideEpiServer("PathToYourFolder");
```

## Usage

### Tags
Styleguide's left menu contains groups of blocks that are configured with `.styleguide.json` file.
In default case groups are created from parent directory names of blocks. 
However there is a possibility to customize group names - or even define groups from the scratch. 
```csharp
services.AddStyleGuideEpiServer(useTags: true);
```
With just `useTags` flag enabled all blocks with be put into group called `Common`. 
Then, each block can be given multiple tags (that will create unique groups). 
After assigning block to tag they will be removed from `Common` group.
```json
{
  "tags": ["Article", "News"],
  "model": {
    "title": "Lorem ipsum dolor sit amet",
    "text": "Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua.",
    "articleUrl": "#"
  },
  "variants": [
    {
      "name": "Without article image",
      "model": {}
    }
  ]
}
```

### Custom block names
Default behaviour is to display block name (in Styleguide left menu) as original model class is named. 
In order to override this label please add property `displayName` in json configuration for the block, as shown below:
```json
{
  "displayName": "Article teaser",
  "model": {
    "title": "Lorem ipsum dolor sit amet",
    "text": "Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua.",
    "articleUrl": "#"
  },
  "variants": [
    {
      "name": "Without article image",
      "model": {}
    }
  ]
}
```