// SPDX-License-Identifier: MIT
//
// This file is part of the server of Counter Strike Nintendo DS Multiplayer Edition (CS:DS)

namespace Counter_Strike_Server
{
    /// <summary>
    /// Gun values used by the bots (fire rate, reload time, accuracy...), the
    /// same as the game. Data in BotGuns.Generated.cs.
    /// </summary>
    public static partial class BotGuns
    {
        public class GunInfo
        {
            public string name;
            public int category;    // 0 pistols, 1 heavy, 2 SMG, 3 rifles (-1 knife)
            public int team;        // -1 both teams, 0 terrorists, 1 counter-terrorists
            public int price;
            public int fireRate;    // Frames (60 per second) between two shots
            public int reloadTime;  // Frames (for one shell if reloadOneByOne)
            public int magazineCapacity;
            public int maxBotShootDistance;
            public float inaccuracyReductionForBot; // [0;1]
            public int bulletCountPerShoot;
            public bool isKnife;
            public bool reloadOneByOne;
        }

        private static GunInfo[] guns;

        static BotGuns()
        {
            LoadGenerated();
        }

        public static int Count => guns.Length;

        /// <summary>
        /// Gun info of a shop index (null if it is not a gun)
        /// </summary>
        public static GunInfo Get(int shopIndex)
        {
            if (shopIndex < 0 || shopIndex >= guns.Length)
                return null;
            return guns[shopIndex];
        }
    }
}
