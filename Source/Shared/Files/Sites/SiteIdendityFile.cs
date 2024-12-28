using System;
using System.Threading;
using static Shared.CommonEnumerators;

namespace Shared
{
    public class SiteIdendityFile
    {
        public int Tile;

        public string UID;

        public Goodwill Goodwill;

        public SiteInfoFile Type = new SiteInfoFile();

        public GuildFile File;

        [NonSerialized] public Semaphore SavingSemaphore = new Semaphore(1, 1);
    }
}
