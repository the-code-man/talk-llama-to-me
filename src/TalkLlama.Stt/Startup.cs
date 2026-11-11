using TalkLlama.Stt.Services;

namespace TalkLlama.Stt
{
    public class Startup
    {
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();
            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen();

            // Register transcription service as a singleton to keep model context ready
            services.AddSingleton<TranscriptionService>();
        }

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
            app.UseEndpoints(endpoints => 
            {
                endpoints.MapControllers();
                endpoints.MapGet("/healthz", async context => 
                {
                    await context.Response.WriteAsJsonAsync(new { status = "healthy", service = "talk-llama-stt" });
                });
            });
        }
    }
}
