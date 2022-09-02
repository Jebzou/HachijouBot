using Discord;
using Discord.Rest;
using Discord.WebSocket;
using HachijouBot.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HachijouBot.Commands
{
    public class ScrapCommand : Command
    {
        public override string Name => "scrap";

        public override string Description => "The command to scrap things";

        public ScrapCommand()
        {
            Options.Add(new SlashCommandOptionBuilder()
                .WithType(ApplicationCommandOptionType.String)
                .WithRequired(true)
                .WithName("what-to-scrap")
                .WithDescription("What do you want to scrap today ?"));
        }

        public override async Task CommandHandler(SocketSlashCommand command)
        {
            string material = command.Data.Options.First().Value?.ToString() ?? "";

            int fuel = (int)Math.Floor((decimal)(Random.Shared.NextSingle() * material.Length));
            int ammo = (int)Math.Floor((decimal)(Random.Shared.NextSingle() * material.Length));
            int steel =(int) Math.Floor((decimal)(Random.Shared.NextSingle() * material.Length));
            int baux = (int)Math.Floor((decimal)(Random.Shared.NextSingle() * material.Length));

            await command.DeferAsync();

            await command.ModifyOriginalResponseAsync((message) =>
            {
                message.Content = $"Scrapping ...";
            });

            await Task.Delay(2000);

            await command.ModifyOriginalResponseAsync((message) =>
            {
                message.Content = $"{material} scrapped for {fuel} {EmoteDataBase.Emotes["kcfuel"]} {ammo} {EmoteDataBase.Emotes["kcammo"]} {steel} {EmoteDataBase.Emotes["kcsteel"]} {baux} {EmoteDataBase.Emotes["kcbauxite"]}";
            });
        }
    }
}
