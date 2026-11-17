// SPDX-License-Identifier: MIT
//
// This file is part of the server of Counter Strike Nintendo DS Multiplayer Edition (CS:DS)

using System;
using System.Collections.Generic;

namespace Counter_Strike_Server
{
    /// <summary>
    /// Plan of the terrorist bots for a round (like ai.c of the game), on
    /// defuse maps with two bomb sites
    /// </summary>
    public enum TerroristsStrategy
    {
        NONE,     // One site or not a defuse map
        RUSH_A,   // Everyone goes to site A
        RUSH_B,   // Everyone goes to site B
        SPLIT,    // Bots spread on both sites, the bomber goes to a random site
    }

    /// <summary>
    /// Bot data shared by the bots of a party (round plan, who defuses...)
    /// </summary>
    public class PartyBotState
    {
        public RoundState lastRoundState = RoundState.END;
        public TerroristsStrategy strategy = TerroristsStrategy.NONE;
        public int splitBomberSite = 0; // Site of the bomber in the SPLIT plan (0 = A, 1 = B)
        public Client defuser = null;   // Counter-terrorist bot going to defuse the bomb
        public Client bombPicker = null; // Terrorist bot going to take the dropped bomb
    }

    public partial class BotBrain
    {
        // Objective of the bot
        private enum BotTask
        {
            NONE,
            PLANT,     // Go to a bomb site and plant the bomb
            DEFUSE,    // Go to the bomb and defuse it
            PICK_BOMB, // Go to the dropped bomb and take it
        }

        private BotTask task = BotTask.NONE;
        private float actionTime = 0;     // Time left to plant/defuse (s), 0: not doing it
        private Vector3 approachPoint = null; // Point to walk to after the path (bomb, site center)
        private RoundState lastRoundState = RoundState.END;
        private bool hasBought = false;

        // Times of the game (main.h): 210 and 600 frames
        private const float PLANT_TIME = 3.5f;
        private const float DEFUSE_TIME = 10f;
        private const float DEFUSE_TIME_WITH_KIT = 5f;
        private const int SHOP_DEFUSE_KIT = 29;

        // Shop indexes (ShopManager)
        private const int SHOP_KEVLAR = 30;
        private const int SHOP_KEVLAR_HELMET = 31;

        #region Round start and shop

        /// <summary>
        /// Called each update with the round state (detects new rounds)
        /// </summary>
        private void CheckNewRound(Party party)
        {
            if (party.roundState == RoundState.WAIT_START && lastRoundState != RoundState.WAIT_START)
            {
                hasBought = false;
                CancelTask();
            }
            lastRoundState = party.roundState;
        }

        /// <summary>
        /// Buy armor and a gun during the buy time (like CheckShopForBot() of the game)
        /// </summary>
        private void BuyEquipment(Party party)
        {
            if (hasBought || bot.isDead)
                return;
            hasBought = true;

            try
            {
                // Armor
                if (bot.armor == 0)
                {
                    if (bot.money >= ShopManager.allShopElements[SHOP_KEVLAR_HELMET].price)
                        ShopManager.Buy(SHOP_KEVLAR_HELMET, bot);
                    else if (bot.money >= ShopManager.allShopElements[SHOP_KEVLAR].price)
                        ShopManager.Buy(SHOP_KEVLAR, bot);
                }

                // Main gun: a random category (heavy, SMG, rifles), a random gun of the team
                if (bot.inventory.allSlots[2] == -1)
                {
                    int category = 1 + random.Next(3);
                    int gun = ChooseGun(category);
                    if (gun == -1)
                    {
                        // Not enough money for this category: try the others
                        for (int otherCategory = 3; otherCategory >= 1 && gun == -1; otherCategory--)
                            gun = ChooseGun(otherCategory);
                    }

                    if (gun != -1)
                        ShopManager.Buy(gun, bot);
                }

                // Defuse kit (faster defuse), one time in two
                if (bot.team == TeamEnum.COUNTERTERRORISTS && !bot.inventory.haveDefuseKit &&
                    bot.money >= ShopManager.allShopElements[SHOP_DEFUSE_KIT].price && random.Next(2) == 0)
                    ShopManager.Buy(SHOP_DEFUSE_KIT, bot);

                // Take the best gun in the hand
                if (bot.inventory.allSlots[2] != -1)
                    bot.inventory.SetCurrentSlot(2);
                else if (bot.inventory.allSlots[1] != -1)
                    bot.inventory.SetCurrentSlot(1);
            }
            catch (Exception e)
            {
                Logger.LogErrorInFile($"Bot buy error: {e.Message}");
            }
        }

        /// <summary>
        /// Random gun of a category that the bot can buy
        /// </summary>
        /// <returns>Shop index, -1 if none</returns>
        private int ChooseGun(int category)
        {
            int team = bot.team == TeamEnum.TERRORISTS ? 0 : 1;
            List<int> guns = new();
            for (int i = 0; i < BotGuns.Count; i++)
            {
                BotGuns.GunInfo gun = BotGuns.Get(i);
                if (gun.category == category && (gun.team == -1 || gun.team == team) && gun.price <= bot.money)
                    guns.Add(i);
            }
            if (guns.Count == 0)
                return -1;
            return guns[random.Next(guns.Count)];
        }

        #endregion

        #region Objectives

        /// <summary>
        /// Objective the bot should have now
        /// </summary>
        private BotTask GetWantedTask(Party party, PartyBotState state)
        {
            if (party.roundState != RoundState.PLAYING)
                return BotTask.NONE;

            if (bot.team == TeamEnum.TERRORISTS && bot.inventory.haveBomb && !party.bombSet)
                return BotTask.PLANT;
            if (bot.team == TeamEnum.TERRORISTS && party.bombDropped && !party.bombSet && state.bombPicker == bot)
                return BotTask.PICK_BOMB;
            if (bot.team == TeamEnum.COUNTERTERRORISTS && party.bombSet && state.defuser == bot)
                return BotTask.DEFUSE;
            return BotTask.NONE;
        }

        private void CancelTask()
        {
            if (task != BotTask.NONE || actionTime > 0)
            {
                task = BotTask.NONE;
                actionTime = 0;
                approachPoint = null;
                ResetPath();
            }
        }

        /// <summary>
        /// Update the objective. Returns true if the bot is planting or
        /// defusing (it doesn't move or shoot).
        /// </summary>
        private bool UpdateObjective(Party party, BotNavData.MapNav nav, float deltaTime)
        {
            PartyBotState state = BotManager.GetPartyState(party);
            BotTask wanted = GetWantedTask(party, state);

            if (wanted != task)
            {
                // New objective: new path
                task = wanted;
                actionTime = 0;
                approachPoint = null;
                ResetPath();
            }

            if (actionTime <= 0)
                return false;

            // Planting or defusing
            actionTime -= deltaTime;
            if (actionTime > 0)
                return true;

            actionTime = 0;
            if (task == BotTask.PLANT)
            {
                // Same position as the game: on the ground under the player
                bot.PlaceBomb(bot.position.x, bot.position.y - (int)(0.845 * 4096), bot.position.z, bot.angle, false);
            }
            else if (task == BotTask.DEFUSE)
            {
                bot.DefuseBomb();
            }
            task = BotTask.NONE;
            ResetPath();
            return false;
        }

        /// <summary>
        /// Choose a path for the objective
        /// </summary>
        /// <returns>False if the bot has no objective</returns>
        private bool ChooseObjectivePath(Party party, BotNavData.MapNav nav)
        {
            if (task == BotTask.PLANT)
            {
                int site = GetBomberSite(party, nav);
                int waypoint = site == 1 ? nav.siteBPoint : nav.siteAPoint;
                if (waypoint < 0 && nav.sites.Length != 0)
                    waypoint = nav.sites[0].waypoint;
                if (waypoint < 0)
                    return false;

                GoTo(nav, waypoint);
                // Then walk to the center of the site if the waypoint is not in the zone
                if (site >= 0 && site < nav.sites.Length)
                    approachPoint = new Vector3(nav.sites[site].x, 0, nav.sites[site].z);
                return true;
            }

            if (task == BotTask.DEFUSE || task == BotTask.PICK_BOMB)
            {
                float bombX = party.bombPosition.x / 4096f;
                float bombY = party.bombPosition.y / 4096f;
                float bombZ = party.bombPosition.z / 4096f;
                GoTo(nav, BotNavData.GetNearestWaypoint(nav, bombX, bombY, bombZ));
                approachPoint = new Vector3(bombX, 0, bombZ);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Site where the bomber goes (0 = A, 1 = B)
        /// </summary>
        private int GetBomberSite(Party party, BotNavData.MapNav nav)
        {
            PartyBotState state = BotManager.GetPartyState(party);
            switch (state.strategy)
            {
                case TerroristsStrategy.RUSH_A:
                    return 0;
                case TerroristsStrategy.RUSH_B:
                    return 1;
                case TerroristsStrategy.SPLIT:
                    return state.splitBomberSite;
                default:
                    // One site: the nearest one
                    if (nav.siteAPoint >= 0 && nav.siteBPoint >= 0)
                    {
                        BotNavData.Waypoint a = nav.waypoints[nav.siteAPoint];
                        BotNavData.Waypoint b = nav.waypoints[nav.siteBPoint];
                        float distanceA = (a.x - x) * (a.x - x) + (a.z - z) * (a.z - z);
                        float distanceB = (b.x - x) * (b.x - x) + (b.z - z) * (b.z - z);
                        return distanceA <= distanceB ? 0 : 1;
                    }
                    return 0;
            }
        }

        /// <summary>
        /// Site index for a random walk (strategy and planted bomb), -1 for a random site
        /// </summary>
        private int GetRoamingSite(Party party, BotNavData.MapNav nav)
        {
            if (nav.sites == null || nav.sites.Length < 2)
                return -1;

            // Bomb planted: everyone goes there (guard or retake)
            if (party.bombSet)
                return GetNearestSite(nav, party.bombPosition.x / 4096f, party.bombPosition.z / 4096f);

            // Terrorists follow the plan of the round
            if (bot.team == TeamEnum.TERRORISTS && party.roundState == RoundState.PLAYING)
            {
                PartyBotState state = BotManager.GetPartyState(party);
                if (state.strategy == TerroristsStrategy.RUSH_A)
                    return 0;
                if (state.strategy == TerroristsStrategy.RUSH_B)
                    return 1;
            }
            return -1;
        }

        private static int GetNearestSite(BotNavData.MapNav nav, float posX, float posZ)
        {
            int nearest = 0;
            float nearestDistance = float.MaxValue;
            for (int i = 0; i < nav.sites.Length; i++)
            {
                float distance = (nav.sites[i].x - posX) * (nav.sites[i].x - posX) + (nav.sites[i].z - posZ) * (nav.sites[i].z - posZ);
                if (distance < nearestDistance)
                {
                    nearestDistance = distance;
                    nearest = i;
                }
            }
            return nearest;
        }

        /// <summary>
        /// Called when the path and the approach point are reached
        /// </summary>
        private void OnDestinationReached(Party party)
        {
            if (task == BotTask.PLANT)
            {
                if (PhysicsManager.CheckBombZone(bot.position, MapManager.allMaps[(int)party.mapId]))
                {
                    actionTime = PLANT_TIME;
                    bot.communicator.SendBombPlacing(); // Planting sound for the players
                    if (BotManager.debugLog)
                        BotManager.Print($"[bot] {bot.name} plants the bomb");
                }
            }
            else if (task == BotTask.DEFUSE)
            {
                if (PhysicsManager.CheckBombDefuseZone(bot.position, party))
                {
                    actionTime = bot.inventory.haveDefuseKit ? DEFUSE_TIME_WITH_KIT : DEFUSE_TIME;
                    bot.communicator.SendBombPlacing(); // Defusing sound for the players
                    if (BotManager.debugLog)
                        BotManager.Print($"[bot] {bot.name} defuses the bomb ({actionTime} s)");
                }
            }
            else if (task == BotTask.PICK_BOMB)
            {
                bot.GetBomb();
            }
        }

        /// <summary>
        /// Walk straight to the approach point (after the path)
        /// </summary>
        /// <returns>True when the point is reached</returns>
        private bool WalkToApproachPoint(float deltaTime)
        {
            float step = SPEED_PER_SECOND * deltaTime;
            float dx = approachPoint.x - x;
            float dz = approachPoint.z - z;
            float distance = MathF.Sqrt(dx * dx + dz * dz);

            if (distance > 0.001f)
                bot.angle = (int)(MathF.Atan2(dx, dz) * 512f / (MathF.PI * 2f) + 256f) & 511;

            // Close enough (zones are about 1 unit wide)
            if (distance <= MathF.Max(step, 0.3f))
            {
                if (distance <= step)
                {
                    x = approachPoint.x;
                    z = approachPoint.z;
                }
                approachPoint = null;
                ApplyPosition();
                return true;
            }

            x += dx / distance * step;
            z += dz / distance * step;
            ApplyPosition();
            return false;
        }

        #endregion

        #region Smoke

        /// <summary>
        /// Is a smoke cloud between two points? (bots can't see through smoke)
        /// </summary>
        private static bool IsBlockedBySmoke(Party party, float x1, float y1, float z1, float x2, float y2, float z2)
        {
            DateTime now = DateTime.Now;
            float dx = x2 - x1, dy = y2 - y1, dz = z2 - z1;
            float lengthSquared = dx * dx + dy * dy + dz * dz;

            foreach (Physics.Grenade grenade in party.allGrenades)
            {
                if (grenade.id != SMOKE_GRENADE_ID)
                    continue;

                // The smoke appears when the grenade stops (about 2.5 s after the
                // throw, the timer is the throw + 4 s) and stays 18 s (game:
                // 1080 frames, with the fade at the end)
                if (now < grenade.timer.AddSeconds(-1.5) || now > grenade.timer.AddSeconds(16))
                    continue;

                // Grenade physics: 8192 units, z inverted
                float cx = grenade.physics.x / 8192f - x1;
                float cy = grenade.physics.y / 8192f - y1;
                float cz = -grenade.physics.z / 8192f - z1;

                float t = lengthSquared > 0 ? (cx * dx + cy * dy + cz * dz) / lengthSquared : 0;
                t = Math.Clamp(t, 0, 1);
                float px = cx - dx * t, py = cy - dy * t, pz = cz - dz * t;
                if (px * px + py * py + pz * pz < SMOKE_RADIUS * SMOKE_RADIUS)
                    return true;
            }
            return false;
        }

        private const int SMOKE_GRENADE_ID = 1;
        // Radius of the smoke sphere of the game (4.65), a bit smaller for the soft edges
        private const float SMOKE_RADIUS = 4.0f;

        #endregion
    }
}
