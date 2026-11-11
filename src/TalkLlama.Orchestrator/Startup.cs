using OllamaSharp;
using TalkLlama.Orchestrator.Services;

namespace TalkLlama.Orchestrator
{
    public class Startup
    {
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        // Configure services from Dependency Injection
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();
            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen();

            var ollamaHost = Configuration["AiSettings:OllamaHost"] ?? "http://localhost:11434";
            var modelName = Configuration["AiSettings:ModelName"] ?? "llama3.1:8b";

            // Register HttpClient for inter-service communication
            services.AddHttpClient();

            // Register Ollama client as singleton
            services.AddSingleton<IOllamaApiClient>(sp =>
            new OllamaApiClient(new Uri(ollamaHost)) { SelectedModel = modelName });

            services.AddScoped<SttClientService>();
            services.AddScoped<AIService>();
        }

        // Configure the Http request pipeline
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI();
            }
            else
            {
                app.UseHttpsRedirection();
            }

            app.UseRouting();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapGet("/healthz", async context =>
                {
                    await context.Response.WriteAsJsonAsync(new { status = "healthy", service = "talk-llama-orchestrator" });
                });
            });
        }
    }
}