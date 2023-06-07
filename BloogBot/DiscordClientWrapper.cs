using BloogBot.Game.Enums;
using Discord;
using Discord.WebSocket;
using System;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace BloogBot
{
    public class DiscordClientWrapper
    {
        static DiscordSocketClient client;
        static SocketGuild guild;
        static SocketRole botsmithsRole;
        static SocketTextChannel channel;
        
        static ulong bloogsMinionsGuildId;
        static ulong botsmithsRoleId;
        static ulong bloogBotChannelId;

        static bool discordBotEnabled;

        static internal void Initialize(BotSettings botSettings)
        {
            discordBotEnabled = botSettings.DiscordBotEnabled;
            if (discordBotEnabled)
            { 
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

                bloogsMinionsGuildId = Convert.ToUInt64(botSettings.DiscordGuildId);
                botsmithsRoleId = Convert.ToUInt64(botSettings.DiscordGuildId);
                bloogBotChannelId = Convert.ToUInt64(botSettings.DiscordChannelId);
                client = new DiscordSocketClient();

                client.Log += Log;
                client.Ready += ClientReady;

                Task.Run(async () =>
                {
                    try
                    {
                        await client.LoginAsync(TokenType.Bot, botSettings.DiscordBotToken);
                        await client.StartAsync();
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine($"Discord connection failed with exception: {e}");
                    }
                });
            }
        }

        static Task Log(LogMessage msg)
        {
            Logger.Log(msg.ToString());
            return Task.CompletedTask;
        }

        static Task ClientReady()
        {
            guild = client.GetGuild(bloogsMinionsGuildId);
            botsmithsRole = guild.GetRole(botsmithsRoleId);
            channel = client.GetChannel(bloogBotChannelId) as SocketTextChannel;

            return Task.CompletedTask;
        }

        static internal void KillswitchAlert(string playerName)
        {
            if (discordBotEnabled)
                Task.Run(async () => 
                await channel.SendMessageAsync($"{botsmithsRole.Mention} \uD83D\uDEA8 ALERT ALERT! {playerName} has arrived in GM Island! Stopping for now. \uD83D\uDEA8")
            );
        }

        static internal void TeleportAlert(string playerName)
        {
            if (discordBotEnabled)
                Task.Run(async () => 
                await channel.SendMessageAsync($"{botsmithsRole.Mention} \uD83D\uDEA8 ALERT ALERT! {playerName} has been teleported! Stopping for now. \uD83D\uDEA8")
            );
        }

        static public void SendMessage(string message)
        {
            if (discordBotEnabled)
                Task.Run(async () => 
                await channel.SendMessageAsync(message)
            );
        }

        static public void SendItemNotification(string playerName, ItemQuality quality, int itemId)
        {
            if (discordBotEnabled)
            {
                var sb = new StringBuilder();
                var article = quality == ItemQuality.Rare ? "a" : "an";
                sb.Append($"{playerName} here! I just found {article} {quality} item!\n");
                sb.Append($"https://classic.wowhead.com/item={itemId}");

                Task.Run(async () =>
                    await channel.SendMessageAsync(sb.ToString())
                );
            }
        }
    }
}
