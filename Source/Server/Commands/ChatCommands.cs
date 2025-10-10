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

        private static readonly CommandBase GiveCommand = new CommandBase("/give", 0,
            "Gives a Thing to specified player (Syntax: player, ThingDef, amount)", GiveCommandAction, true);

        public static readonly CommandBase[] commands = new CommandBase[]
        {
            HelpCommand,
            ToolsCommand,
            PingCommand,
            DisconnectCommand,
            PMCommand,
            KickCommand,
            BanCommand,
            
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
                foreach (CommandBase command in commands.Where(c => !c.IsAdminOnly)) messagesToSend.Add($"{command.Prefix} - {command.Description}");
                if (TargetClient.UserFile.IsAdmin)
                {
                    foreach (CommandBase command in commands.Where(c => c.IsAdminOnly)) messagesToSend.Add($"{command.Prefix} - {command.Description}");
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

                if (string.IsNullOrWhiteSpace(message)) ChatManager.SendConsoleMessage(TargetClient, "Message was empty.");
                else
                {
                    ServerClient toFind = ChatManagerHelper.GetUserFromName(ChatManagerHelper.GetUsernameFromMention(Command[1]));
                    if (toFind == null) ChatManager.SendConsoleMessage(TargetClient, "User was not found.");
                    else
                    {
                        //Don't allow players to send wispers to themselves
                        if (toFind == TargetClient) ChatManager.SendConsoleMessage(TargetClient, "Can't send a whisper to yourself.");
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
            
            string userLabelToKick = Command[1];
            
            if (TargetClient.UserFile.Label == userLabelToKick)
            {
                ChatManager.SendConsoleMessage(TargetClient, "You can't kick yourself.");
                return;
            }

            ServerClient[] foundUsers = ChatCommandsHelper.GetConnectedClientsFromLabel(userLabelToKick);

            
            if (!foundUsers.Any())
            {
                ChatManager.SendConsoleMessage(TargetClient, "User is offline or doesn't exist");
                return;
            }

            if (foundUsers.Length > 1 && Command[2].ToLower() != "any")
            {
                ChatManager.SendConsoleMessage(TargetClient, $"Found multiple users with nickname {userLabelToKick},\n" +
                    $"type \"any\" after username if you want to kick them all");
                return;
            }

            if (Command[2].ToLower() != "any")
            {
                foreach(ServerClient client in foundUsers)
                {
                    ResponseShortcutManager.SendIllegalPacket(client, "kicked from the server", false);
                }
                ChatManager.SendConsoleMessage(TargetClient, $"Kicked {foundUsers.Length} users.");
            }
            else
            {
                ResponseShortcutManager.SendIllegalPacket(foundUsers[0], "kicked from the server", false);
                ChatManager.SendConsoleMessage(TargetClient, $"Kicked {userLabelToKick}.");
            }
        }
        
        public static void BanCommandAction()
        {
            if (TargetClient == null) return;

            string userLabelToBan = Command[1];
            if (userLabelToBan == null)
            {
                ChatManager.SendConsoleMessage(TargetClient, "Enter username of the user");
            }
            
            if (TargetClient.UserFile.Label == userLabelToBan)
            {
                ChatManager.SendConsoleMessage(TargetClient, "You can't ban yourself.");
                return;
            }

            ServerClient[] foundUsers = ChatCommandsHelper.GetConnectedClientsFromLabel(userLabelToBan);

            
            if (!foundUsers.Any())
            {
                ChatManager.SendConsoleMessage(TargetClient, "User is offline or doesn't exist");
                return;
            }

            if (foundUsers.Length > 1 && Command[2].ToLower() != "any")
            {
                ChatManager.SendConsoleMessage(TargetClient, $"Found multiple users with nickname {userLabelToKick},\n" +
                                                             $"type \"any\" after username if you want to ban them all");
                return;
            }

            if (Command[2].ToLower() != "any")
            {
                foreach(ServerClient client in foundUsers)
                {
                    UserManager.BanPlayerFromName(client.UserFile.Uid);
                }
                ChatManager.SendConsoleMessage(TargetClient, $"Banned {foundUsers.Length} users.");
            }
            else
            {
                UserManager.BanPlayerFromName(foundUsers[0].UserFile.Uid);
                ChatManager.SendConsoleMessage(TargetClient, $"Banned {userLabelToKick}.");
            }
        }
    }

    public static class ChatCommandsHelper
    {
        public static ServerClient[] GetConnectedClientsFromLabel(string label)
        {
            return ServerNetwork.Instance.GetConnectedClientsSafe().Where(c => c.UserFile.Label == label).ToArray();
        }
        
        public static ServerClient[] ValidateInputOfUser(string label)[]
    }
}