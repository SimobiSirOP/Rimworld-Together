using Shared;
using static Shared.CommonEnumerators;
using static GameServer.Commands.ChatCommandActions;
using static GameServer.Commands.ChatCommands;
using GameServer.Managers;
using TCPNetwork.Server;
using TCPNetwork.Packets;

namespace GameServer.Commands
{
    public static class ChatCommands
    {
        private static readonly CommandBase HelpCommand = new CommandBase("/help", 0,
            "Shows a list of all available commands",
            HelpCommandAction);

        private static readonly CommandBase ToolsCommand = new CommandBase("/tools", 0,
            "Shows a list of all available chat tools",
            ToolsCommandAction);

        private static readonly CommandBase PingCommand = new CommandBase("/ping", 0,
            "Checks if the connection to the server is working",
            PingCommandAction);

        private static readonly CommandBase DisconnectCommand = new CommandBase("/dc", 0,
            "Forcefully disconnects you from the server",
            DisconnectCommandAction);

        private static readonly CommandBase PMCommand = new CommandBase("/w", 0,
            "Sends a private message to a specific user",
            PrivateMessageCommandAction);

        private static readonly CommandBase KickCommand = new CommandBase("/kick", 0,
            "Kicks player out of the server", KickCommandAction, true);

        private static readonly CommandBase BanCommand = new CommandBase("/ban", 0,
            "Bans player", BanCommandAction, true);

        private static readonly CommandBase ListCommand = new CommandBase("/list", 0,
            "Shows a list of all connected players with corresponding UID's", ListCommandAction, true);

        public static readonly CommandBase[] commands = new CommandBase[]
        {
            HelpCommand,
            ToolsCommand,
            PingCommand,
            DisconnectCommand,
            PMCommand,
            KickCommand,
            BanCommand,
            ListCommand,
        };
    }

    public static class ChatCommandActions
    {
        public static ServerClient TargetClient { get; set; }

        public static string[] Command { get; set; }

        public static void HelpCommandAction()
        {
            if (TargetClient == null) return;
            else
            {
                List<string> messagesToSend = new List<string> { "List of available commands:" };
                foreach (CommandBase command in commands.Where(c => !c.IsAdminOnly))
                    messagesToSend.Add($"{command.Prefix} - {command.Description}");
                if (TargetClient.UserFile.IsAdmin)
                {
                    foreach (CommandBase command in commands.Where(c => c.IsAdminOnly))
                        messagesToSend.Add($"{command.Prefix} - {command.Description}");
                }

                foreach (string str in messagesToSend) ChatManager.SendConsoleMessage(TargetClient, str);
            }
        }

        public static void ToolsCommandAction()
        {
            if (TargetClient == null) return;
            else
            {
                foreach (string str in ChatManager.defaultTextTools)
                {
                    ChatManager.SendConsoleMessage(TargetClient, str);
                }
            }
        }

        public static void PingCommandAction()
        {
            if (TargetClient == null) return;
            else ChatManager.SendConsoleMessage(TargetClient, "Pong!");
        }

        public static void DisconnectCommandAction()
        {
            if (TargetClient == null) return;
            else TargetClient.Listener.DisconnectFlag = true;
        }

        public static void PrivateMessageCommandAction()
        {
            if (TargetClient == null) return;
            else
            {
                string message = "";
                for (int i = 2; i < Command.Length; i++) message += Command[i] + " ";

                if (string.IsNullOrWhiteSpace(message))
                    ChatManager.SendConsoleMessage(TargetClient, "Message was empty.");
                else
                {
                    ServerClient toFind =
                        ChatManagerHelper.GetUserFromName(ChatManagerHelper.GetUsernameFromMention(Command[1]));
                    if (toFind == null) ChatManager.SendConsoleMessage(TargetClient, "User was not found.");
                    else
                    {
                        //Don't allow players to send wispers to themselves
                        if (toFind == TargetClient)
                            ChatManager.SendConsoleMessage(TargetClient, "Can't send a whisper to yourself.");
                        else
                        {
                            ChatData chatData = new ChatData();
                            chatData._message = message;
                            chatData._usernameColor = UserColor.Private;
                            chatData._messageColor = MessageColor.Private;

                            //Send to sender
                            chatData._username = $">> {toFind.UserFile.Label}";
                            TargetClient.Listener.EnqueuePacket(PacketHeader.ChatManager, chatData);

                            //Send to recipient
                            chatData._username = $"<< {TargetClient.UserFile.Label}";
                            toFind.Listener.EnqueuePacket(PacketHeader.ChatManager, chatData);

                            ChatManagerHelper.ShowChatInConsole(chatData._username, message);
                        }
                    }
                }
            }
        }

        public static void KickCommandAction()
        {
            if (TargetClient == null) return;

            if (!TargetClient.UserFile.IsAdmin)
            {
                ChatManager.SendConsoleMessage(TargetClient, "Unsufficient permissions.");
                return;
            }

            string UidToKick = Command[1];

            if (TargetClient.UserFile.Uid == Command[1])
            {
                ChatManager.SendConsoleMessage(TargetClient, "You can not kick yourself.");
                return;
            }

            ServerClient foundUser = ChatManagerHelper.GetUserFromName(UidToKick);
            if (foundUser == null)
            {
                ChatManager.SendConsoleMessage(TargetClient, "UID was not found or user is offline.");
                return;
            }

            foundUser.Listener.DisconnectFlag = true;
            ChatManager.SendConsoleMessage(TargetClient,
                $"Kicked {foundUser.UserFile.Label} ({foundUser.UserFile.Uid}).");
        }

        public static void BanCommandAction()
        {
            if (TargetClient == null) return;

            if (!TargetClient.UserFile.IsAdmin)
            {
                ChatManager.SendConsoleMessage(TargetClient, "Unsufficient permissions.");
                return;
            }

            if (String.IsNullOrEmpty(Command[1]))
            {
                ChatManager.SendConsoleMessage(TargetClient, "Unknown arguments, enter UID of user.");
            }

            string UidToBan = Command[1];

            if (TargetClient.UserFile.Uid == Command[1])
            {
                ChatManager.SendConsoleMessage(TargetClient, "You can not ban yourself.");
                return;
            }

            UserFile foundUser = UserManagerH.GetUserFileFromName(UidToBan);
            if (foundUser == null)
            {
                ChatManager.SendConsoleMessage(TargetClient, "User with this UID was not found");
            }
            UserManager.BanPlayerFromName(foundUser.Uid);
            ChatManager.SendConsoleMessage(TargetClient,
                $"Banned {foundUser.Label} ({foundUser.Uid}).");
        }
        public static void ListCommandAction()
        {
            if (TargetClient == null) return;
            else
            {
                if (!TargetClient.UserFile.IsAdmin)
                {
                    ChatManager.SendConsoleMessage(TargetClient, "Unsufficient permissions.");
                    return;
                }
                ServerNetwork.Instance.GetConnectedClientsSafe();
                List<string> messagesToSend = new List<string> { "List of online players:" };
                foreach (ServerClient client in ServerNetwork.Instance.GetConnectedClientsSafe())
                    messagesToSend.Add($"{client.UserFile.Label} - {client.UserFile.Uid}");
                foreach (string str in messagesToSend) ChatManager.SendConsoleMessage(TargetClient, str);
            }
        }
    }
}