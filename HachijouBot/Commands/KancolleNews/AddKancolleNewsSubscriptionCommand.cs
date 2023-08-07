using Discord;
using Discord.WebSocket;
using HachijouBot.KancolleNews;

namespace HachijouBot.Commands.KancolleNews
{
    public class AddKancolleNewsSubscriptionCommand : Command
    {
        public override string Name => "addkancolleupdates";
        public override string Description => "Subscribe to kancolle update news";

        public override GuildPermission? GuildPermission => Discord.GuildPermission.Administrator;
        
        public override async Task CommandHandler(SocketSlashCommand command)
        {
            KancolleNewsSubscriptionModel model = new KancolleNewsSubscriptionModel
            {
                ChannelId = command.ChannelId ?? 0,
                GuildId = command.GuildId ?? 0
            };

            KancolleNewsDatabase.AddSubscription(model);

            await command.RespondAsync("Subscribed !");

            await Hachijou.GetInstance().KancolleNewsService.CheckNews(model);
        }
    }
}
