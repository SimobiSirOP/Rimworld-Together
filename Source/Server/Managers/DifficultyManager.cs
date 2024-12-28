using GameServer.Core;
using GameServer.Misc;
using GameServer.TCP;
using Shared;
using static Shared.CommonEnumerators;

namespace GameServer.Managers
{
    public static class DifficultyManager
    {
        public static void ParsePacket(ServerClient client, Packet packet)
        {
            SetCustomDifficulty(client, Serializer.ConvertBytesToObject<DifficultyData>(packet.contents));
        }

        public static void SetCustomDifficulty(ServerClient client, DifficultyData difficultyData)
        {
            if (!client.userFile.IsAdmin)
            {
                UserManager.BanPlayerFromName(client.userFile.Uid);
                Printer.Warning($"Player {client.userFile.Uid} attempted to set the custom difficulty while not being an admin");
            }

            else
            {
                Master.difficultyValues = difficultyData._values;
                Main_.SaveValueFile(ServerFileMode.Difficulty, true);
                Printer.Warning($"[Set difficulty] > {client.userFile.Uid}");
            }
        }
    }
}
