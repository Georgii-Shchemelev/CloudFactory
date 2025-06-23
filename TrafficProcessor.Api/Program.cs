using TrafficProcessor;
using TrafficProcessor.Data;
using TrafficProcessor.Infrastructure;
using TrafficProcessor.Processors;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

if (builder.Configuration.GetValue<bool>("UseAdvancedImplementation"))
    builder.Services.AddTransient<IHttpTrafficProcessor, AdvancedHttpTrafficProcessor>();
else
    builder.Services.AddTransient<IHttpTrafficProcessor, PrimitiveHttpTrafficProcessor>();


builder.Services.AddTransient<Func<IMessageHandler>>(x => () => x.GetRequiredService<IHttpTrafficProcessor>());

builder.Services.AddSingleton<IMessageManager<Response>, MessageManager<Response>>();
builder.Services.AddSingleton<StubMessageBroker>();
builder.Services.AddSingleton<IMessageBroker>(x => x.GetRequiredService<StubMessageBroker>());
builder.Services.AddHostedService<StubMessageBroker>(x => x.GetRequiredService<StubMessageBroker>());

builder.Services.AddOptions<MessageBrokerOptions>()
    .Bind(builder.Configuration.GetSection("MessageBroker"));


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.UseRouting();

app.Use(async (context, next) =>
{
    if (context.Request.Path.Value == "/favicon.ico")
    {
        // Favicon request, return 404
        context.Response.StatusCode = StatusCodes.Status404NotFound;
        return;
    }

    // No favicon, call next middleware
    await next.Invoke();
});

app.Run();
