// SPDX-License-Identifier: MIT
//
// This file is part of the server of Counter Strike Nintendo DS Multiplayer Edition (CS:DS)

using System;
using System.Collections.Generic;

namespace Counter_Strike_Server
{
    /// <summary>
    /// Behaviour of one server bot. Same rules as the bots of the single
    /// player mode (player/movements.c and ai/ai.c of the game):
    /// the bot walks in a straight line from waypoint to waypoint.
    /// </summary>
    public partial class BotBrain
    {
        /// <summary>
        /// Speed of the bots in the game: 0.087 unit per frame (60 fps)
        /// </summary>
        private const float SPEED_PER_SECOND = 0.087f * 60f;

        private readonly Client bot;
        private static readonly Random random = new();

        // Precise position (Client.position is in 1/4096 units)
        private float x, y, z;
        // Last position given to Client.position (to detect teleportations)
        private Vector3Int lastPosition;
        private bool hasPosition = false;

        // Current path
        private List<int> path = null;
        private int pathIndex = 0;
        private int lastWaypoint = -1;

        // Wait before the next path (seconds)
        private float waitTime = 0;

        // Combat (like ai.c of the game)
        private Client target = null;
        private Vector3 lastSeenTargetPosition = null;
        private float checkTimer = (float)random.NextDouble() * 0.5f; // Next visibility check (s)
        private float gunWaitCount = 0; // Frames since the last shot (negative: reaction time)
        private bool isReloading = false;
        private float reloadWaitCount = 0; // Frames left for the reload

        /// <summary>
        /// Time between two visibility checks (the game checks one bot every
        /// 5 frames, so each bot about every 0.5 s)
        /// </summary>
        private const float CHECK_INTERVAL = 0.25f;

        // Player who shot the bot (the bot turns to him)
        private Client attacker = null;

        // Failed visibility checks of the current target in a row (a target is
        // lost after 2 failed checks, so a short miss doesn't stop the shots)
        private int targetMissCount = 0;
        // Max distance to keep the current target (a far player who shot the
        // bot is kept even if he is farther than the gun distance)
        private float targetKeepDistance = 0;

        /// <summary>
        /// Called when the bot is hit by a player
        /// </summary>
        public void OnDamaged(Client shooter)
        {
            if (shooter == null || shooter == bot || shooter.team == bot.team)
                return;
            if (BotManager.debugLog)
                BotManager.Print($"[bot] {bot.name} hit by {shooter.name}, dist={DistanceTo(shooter):0.0} sees={CanSee(shooter, false)} target={(target != null ? target.name : "none")}");
            attacker = shooter;
            // Look for him now
            checkTimer = 0;
        }

        public BotBrain(Client bot)
        {
            this.bot = bot;
        }

        /// <summary>
        /// Forget the current path (new round, respawn...)
        /// </summary>
        public void ResetPath()
        {
            path = null;
            pathIndex = 0;
        }

        /// <summary>
        /// Update the bot
        /// </summary>
        /// <param name="deltaTime">Time since the last update (seconds)</param>
        public void Update(float deltaTime)
        {
            Party party = bot.party;
            if (party == null || bot.removed)
                return;

            SyncPosition();
            CheckNewRound(party);

            if (bot.isDead || bot.team == TeamEnum.SPECTATOR)
            {
                ResetPath();
                ResetCombat();
                CancelTask();
                return;
            }

            // Bots wait at the spawn during the buy time (like in the game) and buy
            if (party.roundState == RoundState.WAIT_START)
            {
                ResetPath();
                ResetCombat();
                BuyEquipment(party);
                return;
            }

            BotNavData.MapNav nav = BotNavData.Get(party.mapId);
            if (nav == null)
                return;

            // Planting or defusing: the bot doesn't move or shoot
            if (UpdateObjective(party, nav, deltaTime))
            {
                ApplyPosition();
                return;
            }

            float frames = deltaTime * 60f;
            UpdateGun(frames);

            // Look for enemies
            checkTimer -= deltaTime;
            if (checkTimer <= 0)
            {
                checkTimer = CHECK_INTERVAL;
                UpdateTarget(nav);
            }

            // A bot with a target stops and shoots
            if (target != null)
            {
                if (target.isDead || target.removed || target.party != party)
                {
                    target = null;
                }
                else
                {
                    LookAt(target.position.x / 4096f, target.position.z / 4096f);
                    Shoot(party);
                    ApplyPosition();
                    return;
                }
            }

            if (path == null)
            {
                // End of an objective path: walk to the bomb / the site center
                if (approachPoint != null)
                {
                    if (WalkToApproachPoint(deltaTime))
                        OnDestinationReached(party);
                    return;
                }

                if (waitTime > 0 && task == BotTask.NONE)
                {
                    waitTime -= deltaTime;
                    return;
                }
                waitTime = 0;
                if (!ChooseObjectivePath(party, nav))
                    ChooseNewPath(nav, party);
                if (path == null)
                    return;
            }

            Walk(nav, deltaTime);
        }

        /// <summary>
        /// State of the bot for the bot_debug command
        /// </summary>
        public string GetDebugText()
        {
            Inventory inventory = bot.inventory;
            string ammo = inventory.currentSlot >= 1 && inventory.currentSlot <= 2
                ? $"{inventory.AllAmmoMagazine[inventory.currentSlot - 1].AmmoCount}/{inventory.AllAmmoMagazine[inventory.currentSlot - 1].TotalAmmoCount}"
                : "-";
            string text = $"{bot.name}: pos=({x:0.0};{y:0.0};{z:0.0}) dead={bot.isDead} team={bot.team} slot={inventory.currentSlot} gun={inventory.GetCurrentSlotValue()} ammo={ammo} " +
                          $"target={(target != null ? target.name : "none")} wait={gunWaitCount:0} reload={isReloading} path={(path != null ? path.Count - pathIndex : 0)}";
            // Humans: can the bot see them?
            foreach (Client client in bot.party.allConnectedClients)
            {
                if (client.isBot || client.team == bot.team || client.team == TeamEnum.SPECTATOR)
                    continue;
                text += $"\n    -> {client.name}: dist={DistanceTo(client):0.0} dead={client.isDead} fov+walls={CanSee(client, true)} walls={CanSee(client, false)}";
            }
            return text;
        }

        private void ResetCombat()
        {
            target = null;
            lastSeenTargetPosition = null;
            attacker = null;
            isReloading = false;
            gunWaitCount = 0;
        }

        #region Combat

        /// <summary>
        /// Gun in the hand (null if not a gun). Takes the main gun, or the
        /// pistol, if the bot has a knife or a grenade in the hand.
        /// </summary>
        private BotGuns.GunInfo GetGun()
        {
            Inventory inventory = bot.inventory;
            int slot = inventory.currentSlot;
            bool usable = (slot == 1 || slot == 2) && inventory.allSlots[slot] != -1 &&
                          (inventory.AllAmmoMagazine[slot - 1].AmmoCount > 0 || inventory.AllAmmoMagazine[slot - 1].TotalAmmoCount > 0);
            if (!usable)
            {
                int bestSlot = -1;
                for (int i = 2; i >= 1; i--)
                {
                    if (inventory.allSlots[i] != -1 && (inventory.AllAmmoMagazine[i - 1].AmmoCount > 0 || inventory.AllAmmoMagazine[i - 1].TotalAmmoCount > 0))
                    {
                        bestSlot = i;
                        break;
                    }
                }
                if (bestSlot == -1)
                    return null; // No ammo at all
                if (bestSlot != slot)
                {
                    inventory.SetCurrentSlot(bestSlot); // Sends the gun to the players
                    isReloading = false;
                }
            }
            return BotGuns.Get(inventory.GetCurrentSlotValue());
        }

        /// <summary>
        /// Gun timers (fire rate, reload)
        /// </summary>
        private void UpdateGun(float frames)
        {
            // Warm up: bots never run out of ammo
            if (bot.party.roundState == RoundState.TRAINING)
            {
                Inventory inventory = bot.inventory;
                for (int i = 1; i <= 2; i++)
                {
                    if (inventory.allSlots[i] != -1 && inventory.AllAmmoMagazine[i - 1].TotalAmmoCount == 0)
                    {
                        inventory.ResetGunAmmo(i);
                        SendAmmo();
                    }
                }
            }

            BotGuns.GunInfo gun = GetGun();
            if (gun == null)
                return;

            if (gunWaitCount < gun.fireRate)
                gunWaitCount += frames;

            if (isReloading)
            {
                reloadWaitCount -= frames;
                if (reloadWaitCount <= 0)
                {
                    Inventory.AmmoMagazine magazine = bot.inventory.AllAmmoMagazine[bot.inventory.currentSlot - 1];
                    bot.inventory.ReloadGun();
                    SendAmmo();

                    // Shotguns: one shell at a time until the magazine is full
                    if (gun.reloadOneByOne && magazine.AmmoCount < gun.magazineCapacity && magazine.TotalAmmoCount > 0)
                        reloadWaitCount = gun.reloadTime;
                    else
                        isReloading = false;
                }
            }
        }

        /// <summary>
        /// Send the exact ammo of the bot to the players (after a reload)
        /// </summary>
        private void SendAmmo()
        {
            Inventory inventory = bot.inventory;
            Call.Create($"AMMO;{bot.id};{inventory.AllAmmoMagazine[0].AmmoCount};{inventory.AllAmmoMagazine[0].TotalAmmoCount};{inventory.AllAmmoMagazine[1].AmmoCount};{inventory.AllAmmoMagazine[1].TotalAmmoCount}", bot.party.allConnectedClients, bot);
        }

        /// <summary>
        /// Distance between the bot and a player (world units)
        /// </summary>
        private float DistanceTo(Client client)
        {
            float dx = client.position.x / 4096f - x;
            float dy = client.position.y / 4096f - y;
            float dz = client.position.z / 4096f - z;
            return MathF.Sqrt(dx * dx + dy * dy + dz * dz);
        }

        /// <summary>
        /// Angle (game units, 512 = full turn) to look at a point
        /// </summary>
        private float AngleTo(float targetX, float targetZ)
        {
            return MathF.Atan2(targetX - x, targetZ - z) * 512f / (MathF.PI * 2f) + 256f;
        }

        private void LookAt(float targetX, float targetZ)
        {
            bot.angle = (int)AngleTo(targetX, targetZ) & 511;
        }

        /// <summary>
        /// Look for a target, check if the current target is still visible
        /// (like AiCheckForAction() of the game)
        /// </summary>
        private void UpdateTarget(BotNavData.MapNav nav)
        {
            Party party = bot.party;
            BotGuns.GunInfo gun = GetGun();
            float shootDistance = gun != null ? gun.maxBotShootDistance : 20;

            // No ammo at all: no target, the bot walks
            if (gun == null)
            {
                target = null;
                return;
            }

            // The player who shot the bot comes first (even behind the bot)
            Client shooter = attacker;
            attacker = null;
            if (shooter != null && !shooter.isDead && !shooter.removed && shooter.party == party)
            {
                if (shooter == target)
                {
                    targetMissCount = 0;
                    RememberTarget(target);
                    return;
                }
                if (CanSee(shooter, false))
                {
                    // Fight back now (no reaction time if already shooting)
                    SetNewTarget(shooter);
                    return;
                }
                // Not visible: turn to him if the bot has nothing else to do
                if (target == null)
                    LookAt(shooter.position.x / 4096f, shooter.position.z / 4096f);
            }

            // The current target is still visible?
            if (target != null)
            {
                bool alive = !target.isDead && !target.removed && target.party == party;
                // A bit more distance to keep a target than to take it
                float targetDistance = DistanceTo(target);
                if (alive && targetDistance < MathF.Max(shootDistance * 1.2f, targetKeepDistance) && CanSee(target, false))
                {
                    targetMissCount = 0;
                    RememberTarget(target);

                    // A visible enemy much nearer is more dangerous: take it
                    Client nearer = FindVisibleEnemy(party, targetDistance * 0.6f);
                    if (nearer != null && nearer != target)
                        SetNewTarget(nearer);
                    return;
                }

                targetMissCount++;
                if (alive && targetMissCount < 2)
                    return; // Keep it for now

                // Target lost: reaction time before the next shots (40 to 60 frames, like the game)
                target = null;
                targetMissCount = 0;
                gunWaitCount = -(40 + random.Next(20));
            }

            // Nearest visible enemy
            Client enemy = FindVisibleEnemy(party, shootDistance);
            if (enemy != null)
            {
                SetNewTarget(enemy);
                return;
            }

            // Go where the last target was seen (not during an objective)
            if (lastSeenTargetPosition != null && task == BotTask.NONE)
            {
                int waypoint = BotNavData.GetNearestWaypoint(nav, lastSeenTargetPosition.x, lastSeenTargetPosition.y, lastSeenTargetPosition.z);
                GoTo(nav, waypoint);
                lastSeenTargetPosition = null;
            }
        }

        /// <summary>
        /// Nearest visible enemy (field of view and walls)
        /// </summary>
        /// <param name="maxDistance">Only enemies nearer than this distance</param>
        private Client FindVisibleEnemy(Party party, float maxDistance)
        {
            List<Client> enemies = new();
            foreach (Client client in party.allConnectedClients)
            {
                if (client != bot && !client.isDead && !client.removed && client.team != TeamEnum.SPECTATOR && client.team != bot.team && DistanceTo(client) < maxDistance)
                    enemies.Add(client);
            }
            enemies.Sort((a, b) => DistanceTo(a).CompareTo(DistanceTo(b)));
            foreach (Client enemy in enemies)
            {
                if (CanSee(enemy, true))
                    return enemy;
            }
            return null;
        }

        /// <summary>
        /// Start shooting a new target, after a short reaction time
        /// </summary>
        private void SetNewTarget(Client enemy)
        {
            target = enemy;
            targetMissCount = 0;
            targetKeepDistance = DistanceTo(enemy) * 1.3f;
            RememberTarget(enemy);
            // Reaction time (0.1 to 0.3 s) if the bot was not already shooting
            float reaction = -(6 + random.Next(12));
            if (gunWaitCount > reaction)
                gunWaitCount = reaction;
        }

        private void RememberTarget(Client enemy)
        {
            lastSeenTargetPosition = new Vector3(enemy.position.x / 4096f, enemy.position.y / 4096f, enemy.position.z / 4096f);
        }

        /// <summary>
        /// Can the bot see this player? (field of view and walls)
        /// </summary>
        /// <param name="useFieldOfView">False: the bot can turn (it was shot)</param>
        private bool CanSee(Client client, bool useFieldOfView)
        {
            float targetX = client.position.x / 4096f;
            float targetY = client.position.y / 4096f;
            float targetZ = client.position.z / 4096f;

            if (useFieldOfView && DistanceTo(client) >= 4)
            {
                // Field of view (about 110 degrees), like the game
                float angleDifference = MathF.Abs(AngleDifference(bot.angle, AngleTo(targetX, targetZ)));
                if (angleDifference > 80)
                    return false;
            }

            // Body or head visible (a player behind a box is still visible),
            // walls and smoke
            float eyeY = y + EYE_HEIGHT;
            Party party = bot.party;
            bool bodyVisible = !BotWalls.IsBlocked(party.mapId, x, eyeY, z, targetX, targetY, targetZ) &&
                               !IsBlockedBySmoke(party, x, eyeY, z, targetX, targetY, targetZ);
            if (bodyVisible)
                return true;
            return !BotWalls.IsBlocked(party.mapId, x, eyeY, z, targetX, targetY + EYE_HEIGHT, targetZ) &&
                   !IsBlockedBySmoke(party, x, eyeY, z, targetX, targetY + EYE_HEIGHT, targetZ);
        }

        private static float AngleDifference(float a, float b)
        {
            float difference = (b - a) % 512f;
            if (difference > 256)
                difference -= 512;
            else if (difference < -256)
                difference += 512;
            return difference;
        }

        /// <summary>
        /// Height of the eyes from the position of the player
        /// </summary>
        private const float EYE_HEIGHT = 0.7f;

        /// <summary>
        /// Shoot the target (like checkAiShoot() of the game)
        /// </summary>
        private void Shoot(Party party)
        {
            if (party.roundState != RoundState.PLAYING && party.roundState != RoundState.TRAINING && party.roundState != RoundState.END_ROUND)
                return;

            BotGuns.GunInfo gun = GetGun();
            if (gun == null || gun.isKnife || isReloading || gunWaitCount < gun.fireRate)
                return;

            int slot = bot.inventory.currentSlot;
            Inventory.AmmoMagazine magazine = bot.inventory.AllAmmoMagazine[slot - 1];
            if (magazine.AmmoCount <= 0)
            {
                // Reload (GetGun() already took another gun if there is no ammo)
                isReloading = true;
                reloadWaitCount = gun.reloadTime;
                return;
            }

            gunWaitCount = 0;

            // Shot sound and ammo (like a shot of a DS)
            bot.communicator.SendShoot();
            if (bot.cancelNextHit)
                return;

            for (int pellet = 0; pellet < gun.bulletCountPerShoot; pellet++)
                ShootRay(party, gun);
        }

        /// <summary>
        /// One bullet (like prepareAiRaycast() and Raycast() of the game)
        /// </summary>
        private void ShootRay(Party party, BotGuns.GunInfo gun)
        {
            float eyeY = y + EYE_HEIGHT;
            float targetX = target.position.x / 4096f;
            float targetY = target.position.y / 4096f;
            float targetZ = target.position.z / 4096f;

            // Direction to the target (angles of the game)
            float dx = targetX - x, dy = targetY - eyeY, dz = targetZ - z;
            float distance2D = MathF.Sqrt(dx * dx + dz * dz);
            float horizontalAngle = MathF.Atan2(dx, dz) * 512f / (MathF.PI * 2f) + 256f;
            float verticalAngle = MathF.Atan2(distance2D, dy) * 512f / (MathF.PI * 2f);

            // Shotguns: big spread
            if (gun.bulletCountPerShoot != 1)
            {
                horizontalAngle += (random.Next(100) - 50) / 3f;
                verticalAngle += (random.Next(100) - 50) / 3f;
            }

            float s = horizontalAngle / 512f * MathF.PI * 2f;
            float t = (384f - verticalAngle) / 512f * MathF.PI * 2f;
            float rayX = MathF.Sin(s) * MathF.Cos(t);
            float rayY = -MathF.Sin(t);
            float rayZ = MathF.Cos(s) * MathF.Cos(t);

            // Inaccuracy: better if the target doesn't move
            bool targetIsStill = (DateTime.Now - target.lastMoveTime).TotalMilliseconds > 200;
            float divider = targetIsStill ? 1300f : 700f;
            float reduction = 1 - gun.inaccuracyReductionForBot;
            rayX += (random.Next(150) - 75) / divider * reduction;
            rayY += (random.Next(220) - 110) / divider * reduction;
            rayZ += (random.Next(150) - 75) / divider * reduction;

            // Nearest wall
            float wallDistance = BotWalls.NearestWall(party.mapId, x, eyeY, z, rayX, rayY, rayZ);

            // Nearest player
            Client hitClient = null;
            float playerDistance = float.MaxValue;
            foreach (Client client in party.allConnectedClients)
            {
                if (client == bot || client.isDead || client.removed || client.team == TeamEnum.SPECTATOR)
                    continue;

                float px = client.position.x / 4096f, py = client.position.y / 4096f, pz = client.position.z / 4096f;
                float hit = BotWalls.RayBox(x, eyeY, z, rayX, rayY, rayZ,
                                            px - PLAYER_HALF_WIDTH, py - PLAYER_HALF_HEIGHT, pz - PLAYER_HALF_WIDTH,
                                            px + PLAYER_HALF_WIDTH, py + PLAYER_HALF_HEIGHT, pz + PLAYER_HALF_WIDTH);
                if (hit < 0)
                    continue;

                // Bots don't shoot a team mate who is very near
                if (client.team == bot.team && hit < 1)
                    continue;

                if (hit < playerDistance)
                {
                    playerDistance = hit;
                    hitClient = client;
                }
            }

            if (hitClient != null && playerDistance < wallDistance)
            {
                if (hitClient.team != bot.team || party.partyMode.teamDamage)
                {
                    // Head or legs?
                    float hitY = eyeY + rayY * playerDistance;
                    float heightOnPlayer = hitY - hitClient.position.y / 4096f;
                    bool isHeadShot = heightOnPlayer >= 0.64f;
                    bool isLegShot = !isHeadShot && heightOnPlayer <= 0.2f;

                    float damageDistance = Math.Max(0, (playerDistance - 0.2f) * 50f);
                    bot.MakeHit(hitClient.id, isHeadShot, isLegShot, damageDistance);
                }
            }
            else if (wallDistance != float.MaxValue)
            {
                // Bullet hole flash on the wall
                int hitX = (int)((x + rayX * wallDistance) * 4096);
                int hitYInt = (int)((eyeY + rayY * wallDistance) * 4096);
                int hitZ = (int)((z + rayZ * wallDistance) * 4096);
                Call.Create($"WALLHIT;{hitX};{hitYInt};{hitZ}", party.allConnectedClients, bot);
            }
        }

        // Size of the players for the shots (like the game: 0.35 x 0.9 x 0.35)
        private const float PLAYER_HALF_WIDTH = 0.35f;
        private const float PLAYER_HALF_HEIGHT = 0.9f;

        #endregion

        #region Position

        /// <summary>
        /// Take the position of the client if the server moved the bot
        /// (spawn, new round)
        /// </summary>
        private void SyncPosition()
        {
            Vector3Int position = bot.position;
            if (!hasPosition || position.x != lastPosition.x || position.y != lastPosition.y || position.z != lastPosition.z)
            {
                x = position.x / 4096f;
                y = position.y / 4096f;
                z = position.z / 4096f;
                lastPosition = new Vector3Int(position.x, position.y, position.z);
                hasPosition = true;
                ResetPath();
            }
        }

        /// <summary>
        /// Give the new position to the client (sent by PositionTicker)
        /// </summary>
        private void ApplyPosition()
        {
            Vector3Int position = new((int)(x * 4096), (int)(y * 4096), (int)(z * 4096));
            bot.position = position;
            lastPosition = new Vector3Int(position.x, position.y, position.z);
            bot.cameraAngle = 128; // Bots look straight ahead
            bot.communicator.SendPlayerPosition(false);
        }

        #endregion

        #region Path

        /// <summary>
        /// Choose where to go (like GetRandomPoint() of the game): a random
        /// waypoint near a random bomb site
        /// </summary>
        private void ChooseNewPath(BotNavData.MapNav nav, Party party)
        {
            int destination;
            if (nav.sites != null && nav.sites.Length != 0)
            {
                // Site of the plan of the round, or a random one
                int siteIndex = GetRoamingSite(party, nav);
                if (siteIndex < 0 || siteIndex >= nav.sites.Length)
                    siteIndex = random.Next(nav.sites.Length);
                BotNavData.Site site = nav.sites[siteIndex];
                destination = site.nearWaypoints[random.Next(site.nearWaypoints.Length)];
            }
            else
            {
                destination = random.Next(nav.waypoints.Length);
            }

            GoTo(nav, destination);
        }

        /// <summary>
        /// Start a path to a waypoint
        /// </summary>
        public void GoTo(BotNavData.MapNav nav, int destination)
        {
            int start = BotNavData.GetNearestWaypoint(nav, x, y, z);
            path = BotNavData.FindPath(nav, start, destination);
            pathIndex = 0;
            if (path == null || path.Count == 0)
            {
                path = null;
                waitTime = 1;
            }
        }

        /// <summary>
        /// Walk to the next waypoint of the path
        /// </summary>
        private void Walk(BotNavData.MapNav nav, float deltaTime)
        {
            float step = SPEED_PER_SECOND * deltaTime;

            while (step > 0 && path != null)
            {
                BotNavData.Waypoint waypoint = nav.waypoints[path[pathIndex]];

                // Same height as in the game (waypoint height - 0.1)
                float dx = waypoint.x - x;
                float dy = waypoint.y - 0.1f - y;
                float dz = waypoint.z - z;
                float distance = MathF.Sqrt(dx * dx + dy * dy + dz * dz);

                // Look in the direction of the path (from the previous waypoint)
                float lookX = dx, lookZ = dz;
                if (pathIndex > 0)
                {
                    BotNavData.Waypoint previous = nav.waypoints[path[pathIndex - 1]];
                    lookX = waypoint.x - previous.x;
                    lookZ = waypoint.z - previous.z;
                }
                if (MathF.Abs(lookX) + MathF.Abs(lookZ) > 0.001f)
                    bot.angle = (int)(MathF.Atan2(lookX, lookZ) * 512f / (MathF.PI * 2f) + 256f) & 511;

                if (distance <= step)
                {
                    // Waypoint reached
                    x = waypoint.x;
                    y = waypoint.y - 0.1f;
                    z = waypoint.z;
                    step -= distance;
                    lastWaypoint = path[pathIndex];
                    pathIndex++;

                    if (pathIndex >= path.Count)
                    {
                        // End of the path, a short stop before the next one
                        path = null;
                        waitTime = task == BotTask.NONE ? (float)random.NextDouble() * 1.5f : 0;
                        if (task != BotTask.NONE && approachPoint == null)
                            OnDestinationReached(bot.party);
                    }
                }
                else
                {
                    x += dx / distance * step;
                    y += dy / distance * step;
                    z += dz / distance * step;
                    step = 0;
                }
            }

            ApplyPosition();
        }

        #endregion
    }
}
