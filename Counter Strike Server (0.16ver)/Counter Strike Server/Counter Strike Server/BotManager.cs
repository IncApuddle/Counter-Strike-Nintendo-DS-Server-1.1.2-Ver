// SPDX-License-Identifier: MIT
//
// This file is part of the server of Counter Strike Nintendo DS Multiplayer Edition (CS:DS)

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;

namespace Counter_Strike_Server
{
    /// <summary>
    /// Server bots.
    /// A bot is a Client without a DS: it is in the list of players of a party
    /// (allConnectedClients) but not in ConnectionManager.allClients (no socket).
    /// For the DS, a bot is a normal player.
    ///
    /// Auto-fill: each party with at least one human is filled with bots up to
    /// Settings.botFillTo players (+ bots added by hand with bot_add). Bots
    /// leave when humans join. Parties without humans are closed.
    ///
    /// All the bot work is done in the bot thread, with the same lock order as
    /// the network thread (allClients, then allParties).
    /// </summary>
    public static class BotManager
    {
        /// <summary>
        /// Time between two bot updates (ms)
        /// </summary>
        public const int TICK_MS = 50;

        /// <summary>
        /// Print bot events in the console (bot_log command)
        /// </summary>
        public static bool debugLog = false;

        /// <summary>
        /// Print a bot message (always shown, even with "disable console")
        /// </summary>
        public static void Print(string text)
        {
            lock (printLock)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"[{DateTime.Now}] {text}");
            }
        }
        private static readonly object printLock = new();

        /// <summary>
        /// Names of the bots (max 14 characters, like the player names)
        /// </summary>
        private static readonly string[] BOT_NAMES =
        {
            "Bot Alex", "Bot Max", "Bot Ivan", "Bot Leo", "Bot Sam", "Bot Nick",
            "Bot Oleg", "Bot Tom", "Bot Igor", "Bot Dan", "Bot Erik", "Bot Paul",
            "Bot Yuri", "Bot Mark", "Bot Kyle", "Bot Ben", "Bot Vlad", "Bot Ron",
        };

        private static readonly Random random = new();

        public static void Start()
        {
            Thread thread = new(Run);
            thread.IsBackground = true;
            thread.Start();
        }

        private static void Run()
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            long nextTick = TICK_MS;
            long nextFill = 1000;
            long lastUpdate = stopwatch.ElapsedMilliseconds;

            while (true)
            {
                long wait = nextTick - stopwatch.ElapsedMilliseconds;
                if (wait > 0)
                    Thread.Sleep((int)wait);
                nextTick += TICK_MS;
                if (nextTick < stopwatch.ElapsedMilliseconds)
                    nextTick = stopwatch.ElapsedMilliseconds + TICK_MS;

                try
                {
                    lock (ConnectionManager.allClients)
                    {
                        lock (PartyManager.allParties)
                        {
                            // Fill parties once per second
                            if (stopwatch.ElapsedMilliseconds >= nextFill)
                            {
                                nextFill = stopwatch.ElapsedMilliseconds + 1000;
                                foreach (Party party in new List<Party>(PartyManager.allParties))
                                    UpdatePartyBots(party);
                            }

                            // Bot AI
                            long now = stopwatch.ElapsedMilliseconds;
                            float deltaTime = Math.Min((now - lastUpdate) / 1000f, 0.2f);
                            lastUpdate = now;
                            DateTime nowTime = DateTime.Now;
                            foreach (Party party in PartyManager.allParties)
                            {
                                // Who is moving (bots shoot better at players who don't move)
                                foreach (Client client in party.allConnectedClients)
                                {
                                    Vector3Int position = client.position;
                                    if (position.x != client.trackedX || position.y != client.trackedY || position.z != client.trackedZ)
                                    {
                                        client.trackedX = position.x;
                                        client.trackedY = position.y;
                                        client.trackedZ = position.z;
                                        client.lastMoveTime = nowTime;
                                    }
                                }

                                UpdatePartyState(party);

                                foreach (Client client in new List<Client>(party.allConnectedClients))
                                {
                                    if (client.isBot && client.brain != null)
                                    {
                                        try
                                        {
                                            client.brain.Update(deltaTime);
                                        }
                                        catch (Exception e)
                                        {
                                            Logger.LogErrorInFile($"Bot {client.name} error: {e.Message} {e.StackTrace}");
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    Logger.LogErrorInFile($"Bot error: {e.Message} {e.StackTrace}");
                }
            }
        }

        #region Round plan

        private static readonly Dictionary<Party, PartyBotState> partyStates = new();

        /// <summary>
        /// Bot data of a party (round plan, defuser...)
        /// </summary>
        public static PartyBotState GetPartyState(Party party)
        {
            if (!partyStates.TryGetValue(party, out PartyBotState state))
            {
                state = new PartyBotState();
                partyStates[party] = state;
            }
            return state;
        }

        /// <summary>
        /// Maps with bombs (de_ maps)
        /// </summary>
        public static bool IsDefuseMap(mapEnum map)
        {
            return MapManager.GetMapType(map) == MapType.DEFUSE;
        }

        private static readonly Random strategyRandom = new();

        /// <summary>
        /// New round plan, defuser and bomb picker
        /// </summary>
        private static void UpdatePartyState(Party party)
        {
            PartyBotState state = GetPartyState(party);

            // New round: plan of the terrorists (only with two bomb sites)
            if (party.roundState == RoundState.WAIT_START && state.lastRoundState != RoundState.WAIT_START)
            {
                BotNavData.MapNav nav = BotNavData.Get(party.mapId);
                bool twoSites = nav != null && nav.siteAPoint >= 0 && nav.siteBPoint >= 0 && nav.sites.Length >= 2;
                if (IsDefuseMap(party.mapId) && twoSites)
                {
                    state.strategy = TerroristsStrategy.RUSH_A + strategyRandom.Next(3);
                    state.splitBomberSite = strategyRandom.Next(2);
                }
                else
                {
                    state.strategy = TerroristsStrategy.NONE;
                }
                state.defuser = null;
                state.bombPicker = null;
                if (debugLog)
                    Print($"[bot] New round, terrorists plan: {state.strategy}" + (state.strategy == TerroristsStrategy.SPLIT ? $" (bomber to {(state.splitBomberSite == 0 ? "A" : "B")})" : ""));
            }
            state.lastRoundState = party.roundState;

            float bombX = party.bombPosition.x / 4096f, bombZ = party.bombPosition.z / 4096f;

            // A counter-terrorist bot defuses the planted bomb (not if a
            // player is already defusing it)
            if (party.bombSet && party.roundState == RoundState.PLAYING && IsHumanDefusing(party))
            {
                state.defuser = null;
            }
            else if (party.bombSet && party.roundState == RoundState.PLAYING)
            {
                if (!IsAvailableBot(party, state.defuser, TeamEnum.COUNTERTERRORISTS))
                    state.defuser = NearestBot(party, TeamEnum.COUNTERTERRORISTS, bombX, bombZ);
            }
            else
            {
                state.defuser = null;
            }

            // A terrorist bot takes the dropped bomb
            if (party.bombDropped && !party.bombSet && party.roundState == RoundState.PLAYING)
            {
                if (!IsAvailableBot(party, state.bombPicker, TeamEnum.TERRORISTS))
                    state.bombPicker = NearestBot(party, TeamEnum.TERRORISTS, bombX, bombZ);
            }
            else
            {
                state.bombPicker = null;
            }

            // Forget closed parties
            if (partyStates.Count > PartyManager.allParties.Count)
            {
                foreach (Party oldParty in new List<Party>(partyStates.Keys))
                    if (!PartyManager.allParties.Contains(oldParty))
                        partyStates.Remove(oldParty);
            }
        }

        /// <summary>
        /// Is a player (not a bot) defusing the bomb? He started (BOMBPLACING)
        /// less than 10 s ago, he is near the bomb and he doesn't move.
        /// </summary>
        private static bool IsHumanDefusing(Party party)
        {
            DateTime now = DateTime.Now;
            foreach (Client client in party.allConnectedClients)
            {
                if (client.isBot || client.isDead || client.team != TeamEnum.COUNTERTERRORISTS)
                    continue;
                if ((now - client.bombActionStartTime).TotalSeconds > 10.5)
                    continue;
                if ((now - client.lastMoveTime).TotalSeconds < 0.3)
                    continue; // Moving: not defusing
                if (PhysicsManager.CheckBombDefuseZone(client.position, party))
                    return true;
            }
            return false;
        }

        private static bool IsAvailableBot(Party party, Client client, TeamEnum team)
        {
            return client != null && client.isBot && !client.isDead && !client.removed && client.party == party && client.team == team;
        }

        private static Client NearestBot(Party party, TeamEnum team, float posX, float posZ)
        {
            Client nearest = null;
            float nearestDistance = float.MaxValue;
            foreach (Client client in party.allConnectedClients)
            {
                if (!IsAvailableBot(party, client, team))
                    continue;
                float dx = client.position.x / 4096f - posX, dz = client.position.z / 4096f - posZ;
                float distance = dx * dx + dz * dz;
                if (distance < nearestDistance)
                {
                    nearestDistance = distance;
                    nearest = client;
                }
            }
            return nearest;
        }

        #endregion

        #region Party filling

        /// <summary>
        /// Number of players (humans + bots) wanted in a party
        /// </summary>
        public static int GetWantedPlayerCount(Party party)
        {
            int wanted = Settings.botFillTo + party.extraBots;
            return Math.Clamp(wanted, 0, Settings.maxPlayerPerParty);
        }

        /// <summary>
        /// Add or remove bots of a party (one per call), balance the teams
        /// </summary>
        public static void UpdatePartyBots(Party party)
        {
            if (!PartyManager.allParties.Contains(party))
                return;

            int humans = party.HumanCount();
            List<Client> bots = party.allConnectedClients.Where(c => c.isBot).ToList();

            // No human: the party is closed (bots don't play alone)
            if (humans == 0)
            {
                if (bots.Count != 0 || party.allConnectedClients.Count == 0)
                    party.CheckEmptyParty();
                return;
            }

            int wantedBots = Math.Max(0, GetWantedPlayerCount(party) - humans);

            if (bots.Count < wantedBots)
            {
                AddBot(party, TeamEnum.SPECTATOR);
            }
            else if (bots.Count > wantedBots)
            {
                Client bot = ChooseBotToRemove(party);
                if (bot != null)
                    bot.RemoveClient(true);
            }
            else
            {
                BalanceTeams(party);
            }
        }

        /// <summary>
        /// Move a bot to the other team if a team has 2 players more
        /// </summary>
        private static void BalanceTeams(Party party)
        {
            party.CheckTeamCount(out int terrorists, out int counterTerrorists);
            if (Math.Abs(terrorists - counterTerrorists) < 2)
                return;

            TeamEnum biggerTeam = terrorists > counterTerrorists ? TeamEnum.TERRORISTS : TeamEnum.COUNTERTERRORISTS;
            TeamEnum smallerTeam = biggerTeam == TeamEnum.TERRORISTS ? TeamEnum.COUNTERTERRORISTS : TeamEnum.TERRORISTS;

            // Prefer a dead bot (no change for the round in progress)
            Client bot = party.allConnectedClients.Where(c => c.isBot && c.team == biggerTeam)
                                                  .OrderByDescending(c => c.isDead)
                                                  .FirstOrDefault();
            if (bot != null)
                bot.SetTeam(smallerTeam);
        }

        /// <summary>
        /// Bot to remove: in the bigger team, a dead one if possible
        /// </summary>
        private static Client ChooseBotToRemove(Party party)
        {
            party.CheckTeamCount(out int terrorists, out int counterTerrorists);
            TeamEnum biggerTeam = terrorists >= counterTerrorists ? TeamEnum.TERRORISTS : TeamEnum.COUNTERTERRORISTS;

            return party.allConnectedClients.Where(c => c.isBot)
                                            .OrderByDescending(c => c.team == biggerTeam)
                                            .ThenByDescending(c => c.isDead)
                                            .ThenByDescending(c => c.team == TeamEnum.SPECTATOR)
                                            .FirstOrDefault();
        }

        /// <summary>
        /// Remove a bot to make room for a human joining a full party
        /// </summary>
        /// <returns>True if a bot was removed</returns>
        public static bool MakeRoomForHuman(Party party)
        {
            Client bot = ChooseBotToRemove(party);
            if (bot == null)
                return false;
            bot.RemoveClient(true);
            return true;
        }

        #endregion

        #region Bot creation

        /// <summary>
        /// Create a bot in a party
        /// </summary>
        /// <param name="party">Party</param>
        /// <param name="team">Wanted team, SPECTATOR for the smaller team</param>
        /// <returns>The bot, or null if the party is full</returns>
        public static Client AddBot(Party party, TeamEnum team)
        {
            if (party.allConnectedClients.Count >= Settings.maxPlayerPerParty)
                return null;

            Client bot = new()
            {
                isBot = true,
                ip = "bot",
                party = party,
                name = GetFreeName(party),
            };
            bot.id = ConnectionManager.NewClientId();
            bot.brain = new BotBrain(bot);

            // Inventory (like a new connection)
            bot.inventory.allSlots[0] = Inventory.KNIFE_ID;
            for (int i = 0; i < 2; i++)
                bot.inventory.AllAmmoMagazine[i] = new Inventory.AmmoMagazine();
            for (int i = 1; i < Inventory.INVENTORY_SIZE; i++)
                bot.inventory.allSlots[i] = -1;

            party.allConnectedClients.Add(bot);

            // Show the bot to the other players
            Call.Create($"ADDRANGE;{bot.id}", party.allConnectedClients, bot);
            Call.Create($"SETNAME;{bot.id};{bot.name}", party.allConnectedClients, bot);

            bot.money = party.partyStarted ? party.partyMode.startMoney : party.partyMode.maxMoney;

            // Team: the smaller one (or the asked one)
            party.CheckTeamCount(out int terrorists, out int counterTerrorists);
            if (team == TeamEnum.SPECTATOR)
            {
                if (terrorists > counterTerrorists)
                    team = TeamEnum.COUNTERTERRORISTS;
                else if (counterTerrorists > terrorists)
                    team = TeamEnum.TERRORISTS;
                else
                    team = random.Next(2) == 0 ? TeamEnum.TERRORISTS : TeamEnum.COUNTERTERRORISTS;
            }
            bot.team = team;
            party.communicator.SendTeam();

            // After the team: the default gun depends on the team (Glock / USP)
            bot.ResetPlayer();

            if (party.roundState == RoundState.TRAINING)
            {
                // Plays now
                bot.SetPlayerSpawn(false);
            }
            else
            {
                // Plays at the next round
                bot.health = 0;
                bot.isDead = true;
                bot.communicator.SendHealth();
            }

            bot.communicator.SendKillCountAndDeathCount();
            bot.communicator.SendClientCurrentGunToClients();

            BotManager.Print($"Bot added: {bot.name} (id {bot.id}, {team})");
            return bot;
        }

        private static string GetFreeName(Party party)
        {
            List<string> freeNames = BOT_NAMES.Where(n => !party.allConnectedClients.Any(c => c.name == n)).ToList();
            if (freeNames.Count == 0)
                return "Bot " + random.Next(100, 999);
            return freeNames[random.Next(freeNames.Count)];
        }

        #endregion

        #region Console commands

        /// <summary>
        /// Party for the console commands: the one with the most humans
        /// </summary>
        private static Party GetConsoleParty()
        {
            return PartyManager.allParties.OrderByDescending(p => p.HumanCount()).FirstOrDefault();
        }

        /// <summary>
        /// bot_add [t/ct]
        /// </summary>
        public static void CommandAdd(string teamText)
        {
            lock (ConnectionManager.allClients)
            {
                lock (PartyManager.allParties)
                {
                    Party party = GetConsoleParty();
                    if (party == null || party.HumanCount() == 0)
                    {
                        BotManager.Print("No party with players.");
                        return;
                    }

                    TeamEnum team = TeamEnum.SPECTATOR;
                    if (teamText == "t")
                        team = TeamEnum.TERRORISTS;
                    else if (teamText == "ct")
                        team = TeamEnum.COUNTERTERRORISTS;

                    if (AddBot(party, team) == null)
                    {
                        BotManager.Print("The party is full.");
                        return;
                    }
                    // Keep this bot even with the auto-fill
                    party.extraBots = party.allConnectedClients.Count - Settings.botFillTo;
                }
            }
        }

        /// <summary>
        /// bot_kick [name/all]
        /// </summary>
        public static void CommandKick(string target)
        {
            lock (ConnectionManager.allClients)
            {
                lock (PartyManager.allParties)
                {
                    Party party = GetConsoleParty();
                    if (party == null)
                        return;

                    List<Client> bots = party.allConnectedClients.Where(c => c.isBot).ToList();
                    if (target == "all")
                    {
                        foreach (Client bot in bots)
                            bot.RemoveClient(true);
                        // No auto-fill for this party anymore
                        party.extraBots = -Settings.botFillTo;
                    }
                    else
                    {
                        Client bot = string.IsNullOrEmpty(target)
                            ? ChooseBotToRemove(party)
                            : bots.FirstOrDefault(b => b.name.ToLower().Contains(target));
                        if (bot == null)
                        {
                            BotManager.Print("Bot not found.");
                            return;
                        }
                        bot.RemoveClient(true);
                        party.extraBots = party.allConnectedClients.Count - Settings.botFillTo;
                    }
                }
            }
        }

        /// <summary>
        /// bot_fill [number of players]
        /// </summary>
        public static void CommandFill(string countText)
        {
            if (!int.TryParse(countText, out int count) || count < 0 || count > Settings.maxPlayerPerParty)
            {
                BotManager.Print($"bot_fill: number from 0 to {Settings.maxPlayerPerParty}. Current: {Settings.botFillTo}");
                return;
            }

            lock (ConnectionManager.allClients)
            {
                lock (PartyManager.allParties)
                {
                    Settings.botFillTo = count;
                    // Bots added by hand are forgotten
                    foreach (Party party in PartyManager.allParties)
                        party.extraBots = 0;
                }
            }
            BotManager.Print($"Parties are filled with bots up to {count} players.");
        }

        /// <summary>
        /// bot_debug: state of all bots
        /// </summary>
        public static void CommandDebug()
        {
            // Humans as seen by the server
            lock (ConnectionManager.allClients)
            {
                lock (PartyManager.allParties)
                {
                    foreach (Party party in PartyManager.allParties)
                        foreach (Client client in party.allConnectedClients)
                            if (!client.isBot)
                                BotManager.Print($"Player {client.name}: pos=({client.position.x / 4096f:0.0};{client.position.y / 4096f:0.0};{client.position.z / 4096f:0.0}) dead={client.isDead} team={client.team} round={party.roundState}");
                }
            }

            lock (ConnectionManager.allClients)
            {
                lock (PartyManager.allParties)
                {
                    foreach (Party party in PartyManager.allParties)
                        foreach (Client client in party.allConnectedClients)
                            if (client.isBot && client.brain != null)
                                BotManager.Print(client.brain.GetDebugText());
                }
            }
        }

        /// <summary>
        /// bot_list
        /// </summary>
        public static void CommandList()
        {
            lock (ConnectionManager.allClients)
            {
                lock (PartyManager.allParties)
                {
                    BotManager.Print($"Auto-fill up to {Settings.botFillTo} players.");
                    for (int i = 0; i < PartyManager.allParties.Count; i++)
                    {
                        Party party = PartyManager.allParties[i];
                        string bots = string.Join(", ", party.allConnectedClients.Where(c => c.isBot).Select(c => $"{c.name} ({c.team})"));
                        BotManager.Print($"Party {i}: {party.HumanCount()} players, bots: {(bots.Length == 0 ? "none" : bots)}");
                    }
                }
            }
        }

        #endregion
    }
}
