using Discord;
using Discord.WebSocket;
using HachijouBot.BooruManager;
using HachijouBot.BooruManager.Models;
using HachijouBot.Commands.Reminder;
using HachijouBot.Commands.Roles;
using HachijouBot.Common;
using HachijouBot.Extensions;
using System.Data;
using System.Threading.Channels;

namespace HachijouBot.Commands.ManageDatabase
{
    public class ManageDanbooruWatcherEmbedManager
    {
        private string EmbedId { get; set; }

        private string DeleteButtonId { get; set; } = "";
        private string BackButtonId { get; set; } = "";

        public ulong UserId { get; set; } = 0;
        public ulong ChannelId { get; set; } = 0;

        private Dictionary<string, DanbooruWatcherTagModel> DictionaryOfTags { get; set; } = new Dictionary<string, DanbooruWatcherTagModel>();
        private DanbooruWatcherChannelModel ChannelModel { get; set; } = new();
        private DanbooruWatcherTagModel SelectedTag { get; set; } = new();

        public ManageDanbooruWatcherEmbedManager()
        {
            EmbedId = Guid.NewGuid().ToString();
        }

        public MessageComponent Build()
        {
            DanbooruWatcherChannelModel? channel = DanbooruImageWatcherDataBase.Watchers.FirstOrDefault(data => data.ChannelId == ChannelId);
            if (channel is null) throw new Exception("No data for this channel");
            ChannelModel = channel;

            Hachijou.GetInstance().Client.SelectMenuExecuted += SelectMenuHandler;
            Hachijou.GetInstance().Client.ButtonExecuted += ButtonClicked;

            return BuildSelectMenu(channel);
        }

        private MessageComponent BuildSelectMenu(DanbooruWatcherChannelModel channel)
        {
            List<SelectMenuOptionBuilder> selectMenuBuilder = channel.Tags.Select(tags => BuildDataBaseComponent(tags)).ToList();

            return new ComponentBuilder()
                .WithSelectMenu(EmbedId.ToString(), selectMenuBuilder)
                .Build();
        }

        private async Task ButtonClicked(SocketMessageComponent arg)
        {
            if (arg.GuildId is null) return;
            if (arg.User.Id != UserId) return;
            if (!arg.User.IsAdmin((ulong)arg.GuildId)) return;

            // reload channel data
            DanbooruWatcherChannelModel? channel = DanbooruImageWatcherDataBase.Watchers.FirstOrDefault(data => ChannelModel.ChannelId == data.ChannelId);
            if (channel is null) throw new Exception("No data for this channel");
            ChannelModel = channel;

            if (arg.Data.CustomId == DeleteButtonId)
            {
                DanbooruImageWatcherDataBase.DeleteWatcherTag(ChannelModel, SelectedTag);
                await arg.UpdateAsync(message => {
                    message.Content = "Tag deleted";
                    message.Components = BuildSelectMenu(ChannelModel);
                    message.Embed = null;
                    message.Embeds = null;
                });
                return;
            }
            if (arg.Data.CustomId == BackButtonId)
            {
                await arg.UpdateAsync(message => {
                    message.Content = "Select tags to manage";
                    message.Components = BuildSelectMenu(ChannelModel);
                    message.Embed = null;
                    message.Embeds = null;
                });
                return;
            }
            else
            {
                return;
            }
        }

        private async Task SelectMenuHandler(SocketMessageComponent arg)
        {
            if (arg.GuildId is null) return;
            if (!arg.User.IsAdmin((ulong)arg.GuildId)) return;
            if (arg.User.Id != UserId) return;
            if (arg.Data.CustomId != EmbedId) return;

            string? idChoosed = arg.Data.Values.FirstOrDefault();
            if (idChoosed is null) return;
            if (!DictionaryOfTags.ContainsKey(idChoosed)) return;
            SelectedTag = DictionaryOfTags[idChoosed];

            await arg.UpdateAsync(message => SetDataBaseResponseDisplay(arg, message));
        }

        private SelectMenuOptionBuilder BuildDataBaseComponent(DanbooruWatcherTagModel model)
        {
            string id = Guid.NewGuid().ToString();
            DictionaryOfTags.Add(id, model);

            SelectMenuOptionBuilder builder = new SelectMenuOptionBuilder()
                .WithLabel(model.TagDisplay)
                .WithValue(id);

            return builder;
        }

        public void SetDataBaseResponseDisplay(SocketMessageComponent arg, MessageProperties message)
        {
            EmbedBuilder embedBuilder = new EmbedBuilder()
                .WithTitle($"Displaying tag {SelectedTag.TagDisplay}");

            message.Embed = embedBuilder.Build();

            ComponentBuilder builder = new ComponentBuilder();

            BackButtonId = Guid.NewGuid().ToString();
            builder.WithButton("Back", style: ButtonStyle.Primary, customId: BackButtonId);

            DeleteButtonId = Guid.NewGuid().ToString();
            builder.WithButton("Delete", style: ButtonStyle.Danger, customId: DeleteButtonId);

            message.Components = builder.Build();
        }


        #region Events


        ~ManageDanbooruWatcherEmbedManager()
        {
            Hachijou.GetInstance().Client.SelectMenuExecuted -= SelectMenuHandler;
        }

        #endregion
    }
}
