using Discord;
using Discord.WebSocket;
using HachijouBot.BooruManager;
using HachijouBot.Common;
using HachijouBot.Models;
using System.Data;

namespace HachijouBot.Commands.ManageDatabase
{
    public class ManageDataBaseEmbedManager
    {
        private string EmbedId { get; set; }

        private string NextId { get; set; }

        private string PreviousId { get; set; }

        private int RecordId { get; set; } = 0;

        IDataBase? DataBase { get; set; }

        public ManageDataBaseEmbedManager()
        {
            EmbedId = Guid.NewGuid().ToString();
        }

        public MessageComponent Build()
        {
            List<SelectMenuOptionBuilder> selectMenuBuilder = new List<SelectMenuOptionBuilder>
            {
                BuildDataBaseComponent("Map infos", "mapInfos"),
                BuildDataBaseComponent("Danbooru watchers", "danbooruWatchers")
            };

            Hachijou.GetInstance().Client.SelectMenuExecuted += SelectMenuHandler;
            Hachijou.GetInstance().Client.ButtonExecuted += ButtonClicked;

            return new ComponentBuilder()
                .WithSelectMenu(EmbedId.ToString(), selectMenuBuilder)
                .Build();
        }

        private async Task ButtonClicked(SocketMessageComponent arg)
        {
            if (arg.Data.CustomId == PreviousId)
            {
                RecordId--;
            }
            else if (arg.Data.CustomId == NextId)
            {
                RecordId++;
            }
            else
            {
                return;
            }

            await arg.UpdateAsync(message => SetDataBaseResponseDisplay(arg, message));
        }

        private async Task SelectMenuHandler(SocketMessageComponent arg)
        {
            if (arg.Data.CustomId != EmbedId) return;

            switch (arg.Data.Values.FirstOrDefault())
            {
                case "danbooruWatchers":
                    DataBase = new DanbooruImageWatcherDataBase();
                    break;
                case "mapInfos":
                    DataBase = new MapInfoDatabase();
                    break;
            }

            await arg.UpdateAsync(message => SetDataBaseResponseDisplay(arg, message));
        }

        private SelectMenuOptionBuilder BuildDataBaseComponent(string label, string customId)
        {
            SelectMenuOptionBuilder builder = new SelectMenuOptionBuilder()
                .WithLabel(label)
                .WithValue(customId);

            return builder;
        }

        public void SetDataBaseResponseDisplay(SocketMessageComponent arg, MessageProperties message)
        {
            if (DataBase is null)
            {
                message.Content = "Database not valid";
                return;
            }

            DataTable dt = DataBase.GetData();

            EmbedBuilder embedBuilder = new EmbedBuilder()
                .WithTitle($"Displaying database ({RecordId + 1}/{dt.Rows.Count})");

            if (dt.Rows.Count > 0)
            {
                DataRow row = dt.Rows[RecordId];
                foreach (DataColumn dataColumn in dt.Columns)
                {
                    embedBuilder.AddField(dataColumn.ColumnName, row[dataColumn.ColumnName]);
                }
            }
            else
            {
                embedBuilder.WithDescription("No record");
            }

            message.Embed = embedBuilder.Build();

            ComponentBuilder builder = new ComponentBuilder();

            PreviousId = Guid.NewGuid().ToString();
            builder.WithButton("Previous", style: ButtonStyle.Primary, customId: PreviousId, disabled: RecordId == 0);

            NextId = Guid.NewGuid().ToString();
            builder.WithButton("Next", style: ButtonStyle.Primary, customId: NextId, disabled: RecordId == dt.Rows.Count - 1);

            message.Components = builder.Build();
        }


        #region Events


        ~ManageDataBaseEmbedManager()
        {
            Hachijou.GetInstance().Client.SelectMenuExecuted -= SelectMenuHandler;
        }

        #endregion
    }
}
