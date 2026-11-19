// SPDX-License-Identifier: MIT
//
// This file is part of the server of Counter Strike Nintendo DS Multiplayer Edition (CS:DS)

using System;
using System.Collections.Generic;
using System.Linq;

namespace Counter_Strike_Server
{
    /// <summary>
    /// Game modes of a party: the warm up (1 minute) is a vote for the next
    /// mode (Competitive, Casual, Deathmatch, GunGame). At the end of the game,
    /// the scores are shown 10 seconds, then the party goes back to the warm up.
    /// </summary>
    public partial class Party
    {
        // Mode indexes (PartyManager.allPartyModesData, same numbers as the game)
        public const int MODE_COMPETITIVE = 0;
        public const int MODE_CASUAL = 1;
        public const int MODE_DEATHMATCH = 3;
        public const int MODE_GUNGAME = 4;

        /// <summary>
        /// Modes that can be voted, in the order of the vote menu of the game
        /// </summary>
        public static readonly int[] VOTABLE_MODES = { MODE_COMPETITIVE, MODE_CASUAL, MODE_DEATHMATCH, MODE_GUNGAME };

        /// <summary>
        /// Mode of the warm up
        /// </summary>
        public const int WARMUP_MODE = MODE_COMPETITIVE;

        // GunGame: last gun (G3SG1), kills for the next gun
        public const int GUNGAME_FIRST_GUN = 1; // Glock-18
        public const int GUNGAME_LAST_GUN = 24; // G3SG1
        public const int GUNGAME_KILLS_PER_GUN = 2;

        private static readonly Random modeRandom = new();

        public int ModeIndex => PartyManager.allPartyModesData.IndexOf(partyMode);

        #region Vote

        /// <summary>
        /// A player votes for a mode (index of VOTABLE_MODES)
        /// </summary>
        public void VoteMode(Client client, int voteIndex)
        {
            if (partyStarted || voteIndex < 0 || voteIndex >= VOTABLE_MODES.Length)
                return;
            client.votedMode = VOTABLE_MODES[voteIndex];
            communicator.SendModeVotes();
        }

        /// <summary>
        /// Number of votes for each mode (VOTABLE_MODES order)
        /// </summary>
        public int[] GetModeVotes()
        {
            int[] votes = new int[VOTABLE_MODES.Length];
            foreach (Client client in allConnectedClients)
            {
                int index = Array.IndexOf(VOTABLE_MODES, client.votedMode);
                if (index >= 0 && !client.isBot)
                    votes[index]++;
            }
            return votes;
        }

        /// <summary>
        /// Mode with the most votes (a random one of the best in case of
        /// equality, Competitive without votes)
        /// </summary>
        public int GetVotedMode()
        {
            int[] votes = GetModeVotes();
            int best = votes.Max();
            if (best == 0)
                return MODE_CASUAL; //Nobody voted: Casual

            List<int> bestModes = new();
            for (int i = 0; i < votes.Length; i++)
                if (votes[i] == best)
                    bestModes.Add(VOTABLE_MODES[i]);
            return bestModes[modeRandom.Next(bestModes.Count)];
        }

        #endregion

        #region Start and end

        /// <summary>
        /// End of the warm up: start the game with the voted mode
        /// </summary>
        public void StartWithVotedMode()
        {
            int mode = GetVotedMode();
            partyMode = PartyManager.allPartyModesData[mode];
            communicator.SendMode();
            UserInterfaceManager.PrintMessage($"Party starts in mode {mode}");

            if (partyMode.respawnMode)
                StartRespawnMode();
            else
                Start();
        }

        /// <summary>
        /// Start Deathmatch or GunGame: no rounds, players respawn
        /// </summary>
        private void StartRespawnMode()
        {
            partyStarted = true;
            roundState = RoundState.PLAYING;
            partyTimer = partyMode.roundTime;
            bombSet = false;
            bombDropped = false;
            allGrenades.Clear();
            ClearDroppedGuns();

            AutoTeam();

            foreach (Client client in allConnectedClients)
            {
                client.money = partyMode.startMoney;
                client.killCount = 0;
                client.deathCount = 0;
                client.needRespawn = false;
                client.gunGameKills = 0;
                client.gunGameGun = GUNGAME_FIRST_GUN;
                client.inventory.haveBomb = false;
                client.communicator.SendKillCountAndDeathCount();
            }

            ResetPlayers();
            foreach (Client client in allConnectedClients)
                ApplyModeWeapons(client);

            SetPlayerSpawns();
            communicator.SendMoney();
            communicator.SendPartyRound();
            communicator.SendPartyTimer();
        }

        /// <summary>
        /// Weapons of the mode after a (re)spawn (GunGame gun)
        /// </summary>
        /// <param name="needReload">GunGame new level: empty magazine, the player has to reload before shooting</param>
        public void ApplyModeWeapons(Client client, bool needReload = false)
        {
            if (!partyStarted || !partyMode.gunGame || client.team == TeamEnum.SPECTATOR)
                return;

            Inventory inventory = client.inventory;
            inventory.allSlots[1] = client.gunGameGun;
            inventory.allSlots[2] = -1;
            for (int i = Inventory.INVENTORY_GRENADES_START_POSITION; i < inventory.allSlots.Length; i++)
                inventory.allSlots[i] = -1;
            inventory.haveBomb = false;
            inventory.ResetGunAmmo(1);
            if (needReload)
            {
                // The game starts the reload when it receives an empty magazine
                inventory.AllAmmoMagazine[0].TotalAmmoCount += inventory.AllAmmoMagazine[0].AmmoCount;
                inventory.AllAmmoMagazine[0].AmmoCount = 0;
            }
            inventory.currentSlot = 1;

            client.communicator.SendClientInventoryToClients();
            // The player also needs his own current gun (not sent to him by default)
            Call.Create($"CURGUN;{client.id};1", allConnectedClients);
            Call.Create($"AMMO;{client.id};{inventory.AllAmmoMagazine[0].AmmoCount};{inventory.AllAmmoMagazine[0].TotalAmmoCount};{inventory.AllAmmoMagazine[1].AmmoCount};{inventory.AllAmmoMagazine[1].TotalAmmoCount}", allConnectedClients);
        }

        /// <summary>
        /// Give back the guns of a player after his respawn (Deathmatch)
        /// </summary>
        private void KeepGuns(Client client, int pistol, int mainGun)
        {
            Inventory inventory = client.inventory;
            if (pistol != -1)
            {
                inventory.allSlots[1] = pistol;
                inventory.ResetGunAmmo(1);
            }
            if (mainGun != -1)
            {
                inventory.allSlots[2] = mainGun;
                inventory.ResetGunAmmo(2);
            }
            inventory.currentSlot = mainGun != -1 ? 2 : 1;

            client.communicator.SendClientInventoryToClients();
            Call.Create($"CURGUN;{client.id};{inventory.currentSlot}", allConnectedClients);
            Call.Create($"AMMO;{client.id};{inventory.AllAmmoMagazine[0].AmmoCount};{inventory.AllAmmoMagazine[0].TotalAmmoCount};{inventory.AllAmmoMagazine[1].AmmoCount};{inventory.AllAmmoMagazine[1].TotalAmmoCount}", allConnectedClients);
        }

        /// <summary>
        /// GunGame: a player killed an enemy
        /// </summary>
        public void OnGunGameKill(Client killer)
        {
            if (!partyStarted || !partyMode.gunGame || roundState != RoundState.PLAYING)
                return;

            killer.gunGameKills++;
            if (killer.gunGameKills % GUNGAME_KILLS_PER_GUN != 0)
                return;

            // Last gun done: he wins
            if (killer.gunGameGun >= GUNGAME_LAST_GUN)
            {
                GameOver(killer, TeamEnum.SPECTATOR);
                return;
            }

            killer.gunGameGun++;
            ApplyModeWeapons(killer, true);
        }

        /// <summary>
        /// End of the game: show the winner and the scores, back to the warm up after 10 s
        /// </summary>
        /// <param name="winner">Winner player (Deathmatch, GunGame), null for a team</param>
        /// <param name="winnerTeam">Winner team (Competitive, Casual), SPECTATOR if none</param>
        public void GameOver(Client winner, TeamEnum winnerTeam)
        {
            roundState = RoundState.END;
            partyTimer = PartyManager.gameOverTime;
            communicator.SendPartyRound();
            communicator.SendGameOver(winner, winnerTeam);
            communicator.SendPartyTimer();
        }

        /// <summary>
        /// Deathmatch time is over: the player with the most kills wins
        /// </summary>
        public void GameOverByKills()
        {
            Client winner = allConnectedClients.Where(c => c.team != TeamEnum.SPECTATOR)
                                               .OrderByDescending(c => c.killCount)
                                               .ThenBy(c => c.deathCount)
                                               .FirstOrDefault();
            GameOver(winner, TeamEnum.SPECTATOR);
        }

        /// <summary>
        /// Back to the warm up (vote for the next mode), same players and teams
        /// </summary>
        public void ReturnToWarmup()
        {
            partyStarted = false;
            roundState = RoundState.TRAINING;
            partyMode = PartyManager.allPartyModesData[WARMUP_MODE];
            partyTimer = partyMode.trainingTime;
            terroristsScore = 0;
            counterScore = 0;
            loseCountTerrorists = 0;
            loseCountCounterTerrorists = 0;
            bombSet = false;
            bombDropped = false;
            allGrenades.Clear();
            ClearDroppedGuns();

            foreach (Client client in allConnectedClients)
            {
                client.votedMode = -1;
                client.wantStartNow = false;
                client.killCount = 0;
                client.deathCount = 0;
                client.needRespawn = false;
                client.gunGameKills = 0;
                client.money = partyMode.maxMoney;
                client.inventory.haveBomb = false;
                client.communicator.SendKillCountAndDeathCount();
            }

            ResetPlayers();
            SetPlayerSpawns();

            communicator.SendMode();
            communicator.SendScore();
            communicator.SendMoney();
            communicator.SendPartyRound();
            communicator.SendPartyTimer();
            communicator.SendModeVotes();
        }

        /// <summary>
        /// Respawn the dead players (warm up, Deathmatch, GunGame), called every second
        /// </summary>
        public void RespawnDeadPlayers(DateTime zeroTime)
        {
            foreach (Client client in new List<Client>(allConnectedClients))
            {
                if (!client.needRespawn)
                    continue;

                client.respawnTimer = client.respawnTimer.AddSeconds(-1);
                if (client.respawnTimer <= zeroTime)
                {
                    // Deathmatch: the bought guns stay after the death
                    int pistol = client.inventory.allSlots[1];
                    int mainGun = client.inventory.allSlots[2];

                    client.needRespawn = false;
                    client.SetPlayerSpawn(true);
                    client.ResetPlayer();

                    if (partyStarted && partyMode.respawnMode && !partyMode.gunGame)
                        KeepGuns(client, pistol, mainGun);
                    ApplyModeWeapons(client);
                }
            }
        }

        #endregion
    }
}
