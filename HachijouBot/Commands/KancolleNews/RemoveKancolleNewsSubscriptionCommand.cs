using Discord;
using Discord.WebSocket;
using HachijouBot.KancolleNews;

namespace HachijouBot.Commands.KancolleNews
{
    public class RemoveKancolleNewsSubscriptionCommand : Command
    {
        public override string Name => "removekancolleupdates";
        public override string Description => "Unsubscribe to kancolle update news";

        public override GuildPermission? GuildPermission => Discord.GuildPermission.Administrator;
        
        public override async Task CommandHandler(SocketSlashCommand command)
        {
            KancolleNewsSubscriptionModel? model =
                KancolleNewsDatabase.SubscriptionsLoaded.FirstOrDefault(sub => sub.ChannelId == command.ChannelId);

            if (model is null)
            {
                await command.RespondAsync("No subscription has been found");
                return;
            }

            KancolleNewsDatabase.RemoveSubscription(model);

            await command.RespondAsync("Unsubscribed !");
        }
    }
}
