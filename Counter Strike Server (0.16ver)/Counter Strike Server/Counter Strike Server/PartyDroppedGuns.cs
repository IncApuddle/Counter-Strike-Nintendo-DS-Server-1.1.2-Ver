// SPDX-License-Identifier: MIT
//
// Copyright (c) 2026, Counter-Strike DS contributors
//
// Guns dropped on the floor (drop button and death), Competitive and Casual
// modes only. The server is the authority: it drops the guns, gives them to the
// players who walk on them (if the slot of the gun is free) and tells the
// games what to show.
//
// Messages sent to the games:
//   DROPGUN;netId;gunId;x;y;z   A gun is on the floor (positions * 4096)
//   REMOVEGUN;netId             The gun was taken (-1: remove all the guns)

using System;
using System.Collections.Generic;

namespace Counter_Strike_Server
{
    public class DroppedGun
    {
        public int netId;
        public int gunId;
        public int ammoCount;
        public int totalAmmo;
        public Vector3Int position; // * 4096, on the floor
        public Client owner;        // Player who dropped the gun
        public DateTime dropTime;
    }

    public partial class Party
    {
        public const int MAX_DROPPED_GUNS = 10;
        // The player who dropped a gun can't take it back during this time
        public const double DROPPED_GUN_OWNER_DELAY = 2;
        // Distance to take a gun (* 4096)
        private const int PICKUP_DISTANCE = (int)(0.8 * 4096);
        private const int PICKUP_HEIGHT = (int)(1.6 * 4096);
        // Height of the player position above the floor (* 4096)
        private const int FLOOR_OFFSET = (int)(0.845 * 4096);
        // The drop button throws the gun in front of the player
        private const float THROW_DISTANCE = 1.5f;
        private const float THROW_WALL_MARGIN = 0.4f;

        public readonly List<DroppedGun> droppedGuns = new();
        private int nextDroppedGunId = 0;
        // The party tick (PartyManager) and the position tick (PositionTicker)
        // use the guns from different threads
        private readonly object droppedGunsLock = new();

        /// <summary>
        /// Guns can be dropped in Competitive (and the warm up) and Casual
        /// </summary>
        public bool CanDropGuns()
        {
            int mode = ModeIndex;
            return mode == MODE_COMPETITIVE || mode == MODE_CASUAL;
        }

        /// <summary>
        /// Pistols and primary guns only (no knife, grenades, equipments)
        /// </summary>
        public static bool IsDroppableGun(int gunId)
        {
            return gunId >= 1 && gunId < ShopManager.allShopElements.Count && gunId <= 24 &&
                   ShopManager.allShopElements[gunId] is Gun;
        }

        /// <summary>
        /// Inventory slot of a gun (1 pistols, 2 primary guns)
        /// </summary>
        public static int GetGunSlot(int gunId)
        {
            return ((Gun)ShopManager.allShopElements[gunId]).isBigGun ? 2 : 1;
        }

        /// <summary>
        /// The client wants to drop the gun in his hands (DROPGUN request)
        /// </summary>
        public void DropCurrentGun(Client client)
        {
            if (!CanDropGuns() || client.isDead || client.team == TeamEnum.SPECTATOR)
                return;

            int slot = client.inventory.currentSlot;
            if (slot != 1 && slot != 2)
                return;

            lock (droppedGunsLock)
            {
                if (DropGunFromSlot(client, slot, true))
                {
                    SelectBestGun(client);
                    SendInventoryAndAmmo(client, false);
                }
            }
        }

        /// <summary>
        /// A player died: drop his primary gun, or his pistol
        /// </summary>
        public void DropGunOnDeath(Client client)
        {
            if (!CanDropGuns())
                return;

            lock (droppedGunsLock)
            {
                bool dropped = DropGunFromSlot(client, 2, false) || DropGunFromSlot(client, 1, false);
                if (dropped)
                {
                    SelectBestGun(client);
                    SendInventoryAndAmmo(client, false);
                }
            }
        }

        /// <summary>
        /// Remove a gun from the inventory and put it on the floor
        /// </summary>
        /// <param name="throwGun">True: in front of the player (drop button), false: at his position (death)</param>
        private bool DropGunFromSlot(Client client, int slot, bool throwGun)
        {
            Inventory inventory = client.inventory;
            int gunId = inventory.allSlots[slot];
            if (!IsDroppableGun(gunId))
                return false;

            Inventory.AmmoMagazine magazine = inventory.AllAmmoMagazine[slot - 1];
            DroppedGun gun = new()
            {
                netId = nextDroppedGunId++,
                gunId = gunId,
                ammoCount = magazine.AmmoCount,
                totalAmmo = magazine.TotalAmmoCount,
                position = GetDropPosition(client, throwGun),
                owner = client,
                dropTime = DateTime.Now,
            };
            inventory.allSlots[slot] = -1;

            // Too many guns: remove the oldest one
            if (droppedGuns.Count >= MAX_DROPPED_GUNS)
            {
                DroppedGun oldest = droppedGuns[0];
                droppedGuns.RemoveAt(0);
                Call.Create($"REMOVEGUN;{oldest.netId}", allConnectedClients);
            }

            droppedGuns.Add(gun);
            SendDroppedGun(gun, allConnectedClients);
            return true;
        }

        /// <summary>
        /// Position of a dropped gun on the floor (* 4096)
        /// </summary>
        private Vector3Int GetDropPosition(Client client, bool throwGun)
        {
            int x = client.position.x;
            int y = client.position.y - FLOOR_OFFSET;
            int z = client.position.z;

            if (throwGun)
            {
                // Direction of the player (same angle as the game, 512 = 360 degrees)
                float angle = client.angle / 512f * MathF.PI * 2f;
                float dirX = -MathF.Sin(angle);
                float dirZ = -MathF.Cos(angle);

                // Not through the walls (ray at 0.5 above the floor)
                float ox = x / 4096f;
                float oy = y / 4096f + 0.5f;
                float oz = z / 4096f;
                float wallT = BotWalls.NearestWall(mapId, ox, oy, oz, dirX * THROW_DISTANCE, 0, dirZ * THROW_DISTANCE);
                float distance = MathF.Min(THROW_DISTANCE, wallT * THROW_DISTANCE - THROW_WALL_MARGIN);
                if (distance > 0)
                {
                    x += (int)(dirX * distance * 4096);
                    z += (int)(dirZ * distance * 4096);
                }
            }

            return new Vector3Int(x, y, z);
        }

        /// <summary>
        /// Take the primary gun, the pistol or the knife after a drop
        /// </summary>
        private static void SelectBestGun(Client client)
        {
            Inventory inventory = client.inventory;
            if (inventory.allSlots[inventory.currentSlot] != -1)
                return;

            if (inventory.allSlots[2] != -1)
                inventory.currentSlot = 2;
            else if (inventory.allSlots[1] != -1)
                inventory.currentSlot = 1;
            else
                inventory.currentSlot = 0;
        }

        /// <summary>
        /// Send the inventory, the current gun and the ammo of a client to all
        /// the clients (the client too)
        /// </summary>
        /// <param name="gunAdded">True: a gun was picked up, false: a gun was dropped</param>
        private void SendInventoryAndAmmo(Client client, bool gunAdded)
        {
            Inventory inventory = client.inventory;
            // The game must never have an empty slot in the hands:
            // picked up gun: inventory, then the gun in the hands
            // dropped gun: gun in the hands, then the inventory
            if (gunAdded)
            {
                client.communicator.SendClientInventoryToClients();
                Call.Create($"CURGUN;{client.id};{inventory.currentSlot}", allConnectedClients);
            }
            else
            {
                Call.Create($"CURGUN;{client.id};{inventory.currentSlot}", allConnectedClients);
                client.communicator.SendClientInventoryToClients();
            }
            // After the inventory: the game resets the ammo when it gets the inventory
            Call.Create($"AMMO;{client.id};{inventory.AllAmmoMagazine[0].AmmoCount};{inventory.AllAmmoMagazine[0].TotalAmmoCount};{inventory.AllAmmoMagazine[1].AmmoCount};{inventory.AllAmmoMagazine[1].TotalAmmoCount}", allConnectedClients);
        }

        /// <summary>
        /// Give the guns on the floor to the players who walk on them
        /// (called by PositionTicker)
        /// </summary>
        public void CheckDroppedGunsPickup(List<Client> clients)
        {
            lock (droppedGunsLock)
            {
                if (droppedGuns.Count == 0)
                    return;

                CheckPickup(clients);
            }
        }

        private void CheckPickup(List<Client> clients)
        {
            foreach (Client client in clients)
            {
                if (client.isDead || client.removed || client.team == TeamEnum.SPECTATOR)
                    continue;

                for (int i = 0; i < droppedGuns.Count; i++)
                {
                    DroppedGun gun = droppedGuns[i];

                    if (gun.owner == client && (DateTime.Now - gun.dropTime).TotalSeconds < DROPPED_GUN_OWNER_DELAY)
                        continue;

                    int slot = GetGunSlot(gun.gunId);
                    if (client.inventory.allSlots[slot] != -1)
                        continue;

                    int floorY = client.position.y - FLOOR_OFFSET;
                    if (Math.Abs(client.position.x - gun.position.x) > PICKUP_DISTANCE ||
                        Math.Abs(client.position.z - gun.position.z) > PICKUP_DISTANCE ||
                        Math.Abs(floorY - gun.position.y) > PICKUP_HEIGHT)
                        continue;

                    TakeDroppedGun(client, gun, slot);
                    i--;
                }
            }
        }

        /// <summary>
        /// Give a gun on the floor to a client
        /// </summary>
        private void TakeDroppedGun(Client client, DroppedGun gun, int slot)
        {
            droppedGuns.Remove(gun);
            Call.Create($"REMOVEGUN;{gun.netId}", allConnectedClients);

            Inventory inventory = client.inventory;
            inventory.allSlots[slot] = gun.gunId;
            inventory.AllAmmoMagazine[slot - 1].AmmoCount = gun.ammoCount;
            inventory.AllAmmoMagazine[slot - 1].TotalAmmoCount = gun.totalAmmo;

            //Pick up sound for the player (same sound as in single player)
            if (!client.isBot)
                Call.Create("PICKUP", client);

            // Take a primary gun in the hands, a pistol only if the player has the knife
            if (slot == 2 || inventory.currentSlot == 0)
                inventory.currentSlot = slot;

            SendInventoryAndAmmo(client, true);
        }

        /// <summary>
        /// Send a gun on the floor
        /// </summary>
        private static void SendDroppedGun(DroppedGun gun, List<Client> clients)
        {
            Call.Create(GetDroppedGunRequest(gun), clients);
        }

        private static void SendDroppedGun(DroppedGun gun, Client client)
        {
            Call.Create(GetDroppedGunRequest(gun), client);
        }

        private static string GetDroppedGunRequest(DroppedGun gun)
        {
            return $"DROPGUN;{gun.netId};{gun.gunId};{gun.position.x};{gun.position.y};{gun.position.z}";
        }

        /// <summary>
        /// Send all the guns on the floor to a client who joins the party
        /// </summary>
        public void SendDroppedGunsTo(Client client)
        {
            lock (droppedGunsLock)
            {
                foreach (DroppedGun gun in droppedGuns)
                    SendDroppedGun(gun, client);
            }
        }

        /// <summary>
        /// Remove all the guns on the floor (new round, new game)
        /// </summary>
        public void ClearDroppedGuns()
        {
            lock (droppedGunsLock)
            {
                bool hadGuns = droppedGuns.Count != 0;
                droppedGuns.Clear();
                if (hadGuns)
                    Call.Create("REMOVEGUN;-1", allConnectedClients);
            }
        }
    }
}
