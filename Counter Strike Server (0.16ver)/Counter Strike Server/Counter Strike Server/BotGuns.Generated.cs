// SPDX-License-Identifier: MIT
//
// This file is part of the server of Counter Strike Nintendo DS Multiplayer Edition (CS:DS)
//
// GENERATED from the game sources (weapons/data/gun_data.c) by gen_guns.py.
// Do not edit by hand. Same order as the shop (ShopManager.allShopElements).

namespace Counter_Strike_Server
{
    public static partial class BotGuns
    {
        private static void LoadGenerated()
        {
            guns = new GunInfo[]
            {
                new GunInfo { name = "Knife", category = -1, team = -1, price = 0, fireRate = 24, reloadTime = 0, magazineCapacity = 0, maxBotShootDistance = 3, inaccuracyReductionForBot = 0f, bulletCountPerShoot = 1, isKnife = true, reloadOneByOne = false },
                new GunInfo { name = "Glock-18", category = 0, team = -1, price = 400, fireRate = 9, reloadTime = 136, magazineCapacity = 20, maxBotShootDistance = 20, inaccuracyReductionForBot = 0f, bulletCountPerShoot = 1, isKnife = false, reloadOneByOne = false },
                new GunInfo { name = "USP", category = 0, team = -1, price = 500, fireRate = 9, reloadTime = 162, magazineCapacity = 12, maxBotShootDistance = 20, inaccuracyReductionForBot = 0f, bulletCountPerShoot = 1, isKnife = false, reloadOneByOne = false },
                new GunInfo { name = "P228", category = 0, team = -1, price = 600, fireRate = 9, reloadTime = 162, magazineCapacity = 13, maxBotShootDistance = 20, inaccuracyReductionForBot = 0f, bulletCountPerShoot = 1, isKnife = false, reloadOneByOne = false },
                new GunInfo { name = "Desert Eagle", category = 0, team = -1, price = 650, fireRate = 13, reloadTime = 132, magazineCapacity = 7, maxBotShootDistance = 20, inaccuracyReductionForBot = 0f, bulletCountPerShoot = 1, isKnife = false, reloadOneByOne = false },
                new GunInfo { name = "Dual Berettas", category = 0, team = 0, price = 800, fireRate = 5, reloadTime = 226, magazineCapacity = 30, maxBotShootDistance = 20, inaccuracyReductionForBot = 0f, bulletCountPerShoot = 1, isKnife = false, reloadOneByOne = false },
                new GunInfo { name = "Five SeveN", category = 0, team = 1, price = 750, fireRate = 9, reloadTime = 192, magazineCapacity = 20, maxBotShootDistance = 20, inaccuracyReductionForBot = 0f, bulletCountPerShoot = 1, isKnife = false, reloadOneByOne = false },
                new GunInfo { name = "M3", category = 1, team = -1, price = 1700, fireRate = 53, reloadTime = 30, magazineCapacity = 8, maxBotShootDistance = 14, inaccuracyReductionForBot = 0f, bulletCountPerShoot = 6, isKnife = false, reloadOneByOne = true },
                new GunInfo { name = "XM1014", category = 1, team = -1, price = 3000, fireRate = 15, reloadTime = 27, magazineCapacity = 7, maxBotShootDistance = 14, inaccuracyReductionForBot = 0f, bulletCountPerShoot = 6, isKnife = false, reloadOneByOne = true },
                new GunInfo { name = "M249", category = 1, team = -1, price = 5750, fireRate = 5, reloadTime = 342, magazineCapacity = 100, maxBotShootDistance = 20, inaccuracyReductionForBot = 0f, bulletCountPerShoot = 1, isKnife = false, reloadOneByOne = false },
                new GunInfo { name = "MAC-10", category = 2, team = 0, price = 1400, fireRate = 4, reloadTime = 156, magazineCapacity = 30, maxBotShootDistance = 17, inaccuracyReductionForBot = 0f, bulletCountPerShoot = 1, isKnife = false, reloadOneByOne = false },
                new GunInfo { name = "TMP", category = 2, team = 1, price = 1250, fireRate = 4, reloadTime = 126, magazineCapacity = 30, maxBotShootDistance = 17, inaccuracyReductionForBot = 0f, bulletCountPerShoot = 1, isKnife = false, reloadOneByOne = false },
                new GunInfo { name = "MP5", category = 2, team = -1, price = 1500, fireRate = 5, reloadTime = 180, magazineCapacity = 30, maxBotShootDistance = 17, inaccuracyReductionForBot = 0f, bulletCountPerShoot = 1, isKnife = false, reloadOneByOne = false },
                new GunInfo { name = "UMP-45", category = 2, team = -1, price = 1250, fireRate = 6, reloadTime = 210, magazineCapacity = 25, maxBotShootDistance = 17, inaccuracyReductionForBot = 0f, bulletCountPerShoot = 1, isKnife = false, reloadOneByOne = false },
                new GunInfo { name = "P90", category = 2, team = -1, price = 2350, fireRate = 4, reloadTime = 198, magazineCapacity = 50, maxBotShootDistance = 17, inaccuracyReductionForBot = 0f, bulletCountPerShoot = 1, isKnife = false, reloadOneByOne = false },
                new GunInfo { name = "FAMAS", category = 3, team = 1, price = 2250, fireRate = 6, reloadTime = 228, magazineCapacity = 25, maxBotShootDistance = 23, inaccuracyReductionForBot = 0f, bulletCountPerShoot = 1, isKnife = false, reloadOneByOne = false },
                new GunInfo { name = "Galil", category = 3, team = 0, price = 2000, fireRate = 6, reloadTime = 198, magazineCapacity = 35, maxBotShootDistance = 23, inaccuracyReductionForBot = 0f, bulletCountPerShoot = 1, isKnife = false, reloadOneByOne = false },
                new GunInfo { name = "AK-47", category = 3, team = 0, price = 2500, fireRate = 6, reloadTime = 205, magazineCapacity = 30, maxBotShootDistance = 23, inaccuracyReductionForBot = 0f, bulletCountPerShoot = 1, isKnife = false, reloadOneByOne = false },
                new GunInfo { name = "Scout", category = 3, team = -1, price = 2750, fireRate = 75, reloadTime = 174, magazineCapacity = 10, maxBotShootDistance = 32, inaccuracyReductionForBot = 0.6f, bulletCountPerShoot = 1, isKnife = false, reloadOneByOne = false },
                new GunInfo { name = "M4A1", category = 3, team = 1, price = 3100, fireRate = 6, reloadTime = 184, magazineCapacity = 30, maxBotShootDistance = 23, inaccuracyReductionForBot = 0f, bulletCountPerShoot = 1, isKnife = false, reloadOneByOne = false },
                new GunInfo { name = "AUG", category = 3, team = 1, price = 3500, fireRate = 6, reloadTime = 228, magazineCapacity = 30, maxBotShootDistance = 23, inaccuracyReductionForBot = 0.5f, bulletCountPerShoot = 1, isKnife = false, reloadOneByOne = false },
                new GunInfo { name = "SG 552", category = 3, team = 0, price = 3500, fireRate = 6, reloadTime = 174, magazineCapacity = 30, maxBotShootDistance = 32, inaccuracyReductionForBot = 0.6f, bulletCountPerShoot = 1, isKnife = false, reloadOneByOne = false },
                new GunInfo { name = "SG 550", category = 3, team = 1, price = 4200, fireRate = 15, reloadTime = 228, magazineCapacity = 30, maxBotShootDistance = 32, inaccuracyReductionForBot = 0.6f, bulletCountPerShoot = 1, isKnife = false, reloadOneByOne = false },
                new GunInfo { name = "AWP", category = 3, team = -1, price = 4750, fireRate = 88, reloadTime = 222, magazineCapacity = 10, maxBotShootDistance = 36, inaccuracyReductionForBot = 0.7f, bulletCountPerShoot = 1, isKnife = false, reloadOneByOne = false },
                new GunInfo { name = "G3SG1", category = 3, team = 0, price = 5000, fireRate = 15, reloadTime = 282, magazineCapacity = 20, maxBotShootDistance = 32, inaccuracyReductionForBot = 0.6f, bulletCountPerShoot = 1, isKnife = false, reloadOneByOne = false },
            };
        }
    }
}
