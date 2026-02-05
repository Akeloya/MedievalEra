
using MedievalEra.Server.Core.Game.Common;
using MedievalEra.Server.Core.Game.Dice;
using MedievalEra.Server.Core.Game.Interfaces;

using System.Text.Json;
using System.Text.Json.Serialization;

namespace MedievalEra.Server
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllers().AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
                // или с настройками:
                options.JsonSerializerOptions.Converters.Add(
                    new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));
            }); ;
            // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
            builder.Services.AddOpenApi();

            builder.Services.AddSingleton<IRandomProvider, DefaultRandomProvider>();
            builder.Services.AddSingleton<DiceFactory>();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
            }

            app.UseHttpsRedirection();

            app.UseDefaultFiles();
            app.MapStaticAssets();
            app.UseRouting();
            //app.UseAuthorization();
            
            app.MapControllers();

            app.MapFallbackToFile("/index.html");

            app.Run();
        }
    }
}
