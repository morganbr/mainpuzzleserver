using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace PuzzleTimerTrigger
{
    public class PlayerInfo
    {
        public int InternalPlayerId { get; set; } // Unique ID for the player within the team
        public int PuzzleUserId { get; set; }
        public string PlayerName { get; set; }
        public string PlayerClass { get; set; } // e.g., "captain", "engineer", etc.
        public bool IsImposter { get; set; } // true if this player is the imposter
    }

    public class TeamInfo
    {
        public int TeamId { get; set; }
        public string TeamName { get; set; }
        public bool TeamIsRemote { get; set; }
        public List<PlayerInfo> Players { get; set; }
        public bool HasImpostor { get; set; }
        public int NextNonSusId { get; set; } // Used to assign a unique ID to the next non-sus player
    }


    public class TriggerAdminAnswerSubmission
    {
        /*
        "teamId": 5436,
        "teamName": "Crash Test",
        "teamIsRemote": false,
        "puzzleUserId": 1,
        "playerName": "Megan Quinn",
        "playerClassUniqueName": "warrant"
        */

        public class RawPlayerInfo
        {
            public int teamId { get; set; }
            public string teamName { get; set; }
            public bool teamIsRemote { get; set; }
            public int puzzleUserId { get; set; }
            public string playerName { get; set; }
            public string playerClassUniqueName { get; set; }
        }

        public bool CanBeImpostor(string playerName)
        {
            if (String.IsNullOrWhiteSpace(playerName))
            {
                return false;
            }

            if (!playerName.All(c => Char.IsAscii(c)))
            {
                return false; // Player name must be ASCII characters only
            }

            return true;
        }

        public List<TeamInfo> GetAllTeamInfo()
        {
            List<RawPlayerInfo> allPlayers = HttpClient.GetFromJsonAsync<List<RawPlayerInfo>>("http://puzzlehunt.azurewebsites.net/api/puzzleapi/state/eventteammembers/1062?eventPassword=26d91a28-8fd4-47d4-86c7-9777c59ae0a0").Result;

            Dictionary<int, TeamInfo> teamMap = new Dictionary<int, TeamInfo>();

            foreach (RawPlayerInfo rawPlayer in allPlayers)
            {
                if (!teamMap.ContainsKey(rawPlayer.teamId))
                {
                    teamMap[rawPlayer.teamId] = new TeamInfo
                    {
                        TeamId = rawPlayer.teamId,
                        TeamName = rawPlayer.teamName,
                        TeamIsRemote = rawPlayer.teamIsRemote,
                        Players = new List<PlayerInfo>(),
                        HasImpostor = false,
                        NextNonSusId = 1 // Start with 1 for the first non-sus player
                    };
                }

                bool canBeImpostor = CanBeImpostor(rawPlayer.playerName);

                TeamInfo team = teamMap[rawPlayer.teamId];

                bool isImposter = false;
                int internalPlayerId;
                if (!team.HasImpostor && canBeImpostor)
                {
                    // If this is the first player in the team and they can be an impostor, assign them as the impostor
                    isImposter = true;
                    internalPlayerId = 0;
                }
                else
                {

                    internalPlayerId = team.NextNonSusId;
                    team.NextNonSusId++;
                }

                team.Players.Add(new PlayerInfo
                {
                    PuzzleUserId = rawPlayer.puzzleUserId,
                    PlayerName = rawPlayer.playerName,
                    PlayerClass = rawPlayer.playerClassUniqueName,
                    IsImposter = isImposter, // First player in the list is the impostor
                    InternalPlayerId = internalPlayerId // Assign a unique ID based on the order of players
                });
                team.HasImpostor |= team.Players.Last().IsImposter;
            }

            List<TeamInfo> teams = teamMap.Values.ToList();

            foreach (TeamInfo team in teams)
            {
                if (!team.HasImpostor)
                {
                    //// If no impostor is assigned, assign the first player as the impostor
                    //if (team.Players.Count > 0)
                    //{
                    //    team.Players[0].IsImposter = true;
                    //    team.HasImpostor = true;
                    //}
                }
            }

            return new List<TeamInfo>(teamMap.Values);
        }

        public string GenerateDictionaryCodeForTeamList(List<TeamInfo> teams)
        {
            string code = "new Dictionary<int, TeamInfo>\n{\n";
            foreach (TeamInfo team in teams)
            {
                code += $"    {{ {team.TeamId}, new TeamInfo {{ TeamId = {team.TeamId}, TeamName = \"{team.TeamName}\", TeamIsRemote = {team.TeamIsRemote.ToString().ToLower()}, Players = new List<PlayerInfo>\n    {{\n";
                foreach (PlayerInfo player in team.Players)
                {
                    code += $"        new PlayerInfo {{ InternalPlayerId = {player.InternalPlayerId}, PuzzleUserId = {player.PuzzleUserId}, PlayerName = \"{player.PlayerName}\", PlayerClass = \"{player.PlayerClass}\", IsImposter = {player.IsImposter.ToString().ToLower()} }},\n";
                }
                code += "    }\n    }\n},\n";
            }
            code += "};\n";
            return code;
        }

        /// <summary>
        /// Stores the mapping of PuzzleId:SubmissionInfo where the PlayerId is 1-12 and TeamId is 0 (basically a dataset that can be used to generate the final player dataset later)
        /// </summary>
        private Dictionary<int, SubmissionInfo> LocalPuzzleToSubmissionBaseMapping = new Dictionary<int, SubmissionInfo>
        {
            { 7445, new SubmissionInfo{ AnswerText = "BOREAS", PlayerId=0, PuzzleId=7445, TeamId = 0} },
            { 7449, new SubmissionInfo{ AnswerText = "ELVIK", PlayerId=0, PuzzleId=7449, TeamId = 0} },
            { 7446, new SubmissionInfo{ AnswerText = "EREBUS", PlayerId=0, PuzzleId=7446, TeamId = 0} },
            { 7447, new SubmissionInfo{ AnswerText = "MIDWAY", PlayerId=1, PuzzleId=7447, TeamId = 0} },
            { 7371, new SubmissionInfo{ AnswerText = "MONTAUK", PlayerId=1, PuzzleId=7371, TeamId = 0} },
            { 7407, new SubmissionInfo{ AnswerText = "SEPHORA", PlayerId=1, PuzzleId=7407, TeamId = 0} },
            { 7405, new SubmissionInfo{ AnswerText = "NARCISSUS", PlayerId=2, PuzzleId=7405, TeamId = 0} },
            { 7450, new SubmissionInfo{ AnswerText = "PARADOX", PlayerId=2, PuzzleId=7450, TeamId = 0} },
            { 7426, new SubmissionInfo{ AnswerText = "VIRGINIA", PlayerId=2, PuzzleId=7426, TeamId = 0} },
            { 7427, new SubmissionInfo{ AnswerText = "EUROPA", PlayerId=3, PuzzleId=7427, TeamId = 0} },
            { 7434, new SubmissionInfo{ AnswerText = "MANTICORE", PlayerId=3, PuzzleId=7434, TeamId = 0} },
            { 7372, new SubmissionInfo{ AnswerText = "NOVEMBER", PlayerId=3, PuzzleId=7372, TeamId = 0} },
            { 7404, new SubmissionInfo{ AnswerText = "OUTREACH", PlayerId=4, PuzzleId=7404, TeamId = 0} },
            { 7436, new SubmissionInfo{ AnswerText = "POWELL", PlayerId=4, PuzzleId=7436, TeamId = 0} },
            { 7438, new SubmissionInfo{ AnswerText = "THESEUS", PlayerId=4, PuzzleId=7438, TeamId = 0} },
            { 7444, new SubmissionInfo{ AnswerText = "LEVIATHAN", PlayerId=5, PuzzleId=7444, TeamId = 0} },
            { 7443, new SubmissionInfo{ AnswerText = "MONTEVIDEO", PlayerId=5, PuzzleId=7443, TeamId = 0} },
            { 7463, new SubmissionInfo{ AnswerText = "RESOLUTE", PlayerId=5, PuzzleId=7463, TeamId = 0} },
            { 7373, new SubmissionInfo{ AnswerText = "ALLEGIANCE", PlayerId=6, PuzzleId=7373, TeamId = 0} },
            { 7409, new SubmissionInfo{ AnswerText = "DELILAH", PlayerId=6, PuzzleId=7409, TeamId = 0} },
            { 7428, new SubmissionInfo{ AnswerText = "VICTORY", PlayerId=6, PuzzleId=7428, TeamId = 0} },
            { 7397, new SubmissionInfo{ AnswerText = "ARCHIMEDES", PlayerId=7, PuzzleId=7397, TeamId = 0} },
            { 7432, new SubmissionInfo{ AnswerText = "JUNKET", PlayerId=7, PuzzleId=7432, TeamId = 0} },
            { 7462, new SubmissionInfo{ AnswerText = "SAMSON", PlayerId=7, PuzzleId=7462, TeamId = 0} },
            { 7433, new SubmissionInfo{ AnswerText = "AMERICAN", PlayerId=8, PuzzleId=7433, TeamId = 0} },
            { 7437, new SubmissionInfo{ AnswerText = "AVARICE", PlayerId=8, PuzzleId=7437, TeamId = 0} },
            { 7441, new SubmissionInfo{ AnswerText = "MELVILLE", PlayerId=8, PuzzleId=7441, TeamId = 0} },
            { 7429, new SubmissionInfo{ AnswerText = "ESMERELDA", PlayerId=9, PuzzleId=7429, TeamId = 0} },
            { 7440, new SubmissionInfo{ AnswerText = "GATEWAY", PlayerId=9, PuzzleId=7440, TeamId = 0} },
            { 7460, new SubmissionInfo{ AnswerText = "TYPHOON", PlayerId=9, PuzzleId=7460, TeamId = 0} },
            { 7406, new SubmissionInfo{ AnswerText = "DAHLIA", PlayerId=10, PuzzleId=7406, TeamId = 0} },
            { 7430, new SubmissionInfo{ AnswerText = "MIRANDA", PlayerId=10, PuzzleId=7430, TeamId = 0} },
            { 7431, new SubmissionInfo{ AnswerText = "SHERIDAN", PlayerId=10, PuzzleId=7431, TeamId = 0} },
            { 7461, new SubmissionInfo{ AnswerText = "EMERSON", PlayerId=11, PuzzleId=7461, TeamId = 0} },
            { 7435, new SubmissionInfo{ AnswerText = "GARDENIA", PlayerId=11, PuzzleId=7435, TeamId = 0} },
            { 7459, new SubmissionInfo{ AnswerText = "SNARK", PlayerId=11, PuzzleId=7459, TeamId = 0} },
        };

        /// <summary>
        /// Stores the mapping of PuzzleId:SubmissionInfo where the PlayerId is 1-12 and TeamId is 0 (basically a dataset that can be used to generate the final player dataset later)
        /// </summary>
        private Dictionary<int, SubmissionInfo> RemotePuzzleToSubmissionBaseMapping = new Dictionary<int, SubmissionInfo>
        {
            { 7498, new SubmissionInfo{ AnswerText = "BOREAS", PlayerId=0, PuzzleId=7498, TeamId = 0} },
            { 7449, new SubmissionInfo{ AnswerText = "ELVIK", PlayerId=0, PuzzleId=7449, TeamId = 0} },
            { 7446, new SubmissionInfo{ AnswerText = "EREBUS", PlayerId=0, PuzzleId=7446, TeamId = 0} },
            { 7447, new SubmissionInfo{ AnswerText = "MIDWAY", PlayerId=1, PuzzleId=7447, TeamId = 0} },
            { 7393, new SubmissionInfo{ AnswerText = "MONTAUK", PlayerId=1, PuzzleId=7393, TeamId = 0} },
            { 7407, new SubmissionInfo{ AnswerText = "SEPHORA", PlayerId=1, PuzzleId=7407, TeamId = 0} },
            { 7405, new SubmissionInfo{ AnswerText = "NARCISSUS", PlayerId=2, PuzzleId=7405, TeamId = 0} },
            { 7450, new SubmissionInfo{ AnswerText = "PARADOX", PlayerId=2, PuzzleId=7450, TeamId = 0} },
            { 7426, new SubmissionInfo{ AnswerText = "VIRGINIA", PlayerId=2, PuzzleId=7426, TeamId = 0} },
            { 7427, new SubmissionInfo{ AnswerText = "EUROPA", PlayerId=3, PuzzleId=7427, TeamId = 0} },
            { 7434, new SubmissionInfo{ AnswerText = "MANTICORE", PlayerId=3, PuzzleId=7434, TeamId = 0} },
            { 7394, new SubmissionInfo{ AnswerText = "NOVEMBER", PlayerId=3, PuzzleId=7394, TeamId = 0} },
            { 7404, new SubmissionInfo{ AnswerText = "OUTREACH", PlayerId=4, PuzzleId=7404, TeamId = 0} },
            { 7436, new SubmissionInfo{ AnswerText = "POWELL", PlayerId=4, PuzzleId=7436, TeamId = 0} },
            { 7438, new SubmissionInfo{ AnswerText = "THESEUS", PlayerId=4, PuzzleId=7438, TeamId = 0} },
            { 7444, new SubmissionInfo{ AnswerText = "LEVIATHAN", PlayerId=5, PuzzleId=7444, TeamId = 0} },
            { 7443, new SubmissionInfo{ AnswerText = "MONTEVIDEO", PlayerId=5, PuzzleId=7443, TeamId = 0} },
            { 7463, new SubmissionInfo{ AnswerText = "RESOLUTE", PlayerId=5, PuzzleId=7463, TeamId = 0} },
            { 7395, new SubmissionInfo{ AnswerText = "ALLEGIANCE", PlayerId=6, PuzzleId=7395, TeamId = 0} },
            { 7409, new SubmissionInfo{ AnswerText = "DELILAH", PlayerId=6, PuzzleId=7409, TeamId = 0} },
            { 7428, new SubmissionInfo{ AnswerText = "VICTORY", PlayerId=6, PuzzleId=7428, TeamId = 0} },
            { 7397, new SubmissionInfo{ AnswerText = "ARCHIMEDES", PlayerId=7, PuzzleId=7397, TeamId = 0} },
            { 7432, new SubmissionInfo{ AnswerText = "JUNKET", PlayerId=7, PuzzleId=7432, TeamId = 0} },
            { 7462, new SubmissionInfo{ AnswerText = "SAMSON", PlayerId=7, PuzzleId=7462, TeamId = 0} },
            { 7433, new SubmissionInfo{ AnswerText = "AMERICAN", PlayerId=8, PuzzleId=7433, TeamId = 0} },
            { 7437, new SubmissionInfo{ AnswerText = "AVARICE", PlayerId=8, PuzzleId=7437, TeamId = 0} },
            { 7441, new SubmissionInfo{ AnswerText = "MELVILLE", PlayerId=8, PuzzleId=7441, TeamId = 0} },
            { 7429, new SubmissionInfo{ AnswerText = "ESMERELDA", PlayerId=9, PuzzleId=7429, TeamId = 0} },
            { 7440, new SubmissionInfo{ AnswerText = "GATEWAY", PlayerId=9, PuzzleId=7440, TeamId = 0} },
            { 7460, new SubmissionInfo{ AnswerText = "TYPHOON", PlayerId=9, PuzzleId=7460, TeamId = 0} },
            { 7406, new SubmissionInfo{ AnswerText = "DAHLIA", PlayerId=10, PuzzleId=7406, TeamId = 0} },
            { 7430, new SubmissionInfo{ AnswerText = "MIRANDA", PlayerId=10, PuzzleId=7430, TeamId = 0} },
            { 7431, new SubmissionInfo{ AnswerText = "SHERIDAN", PlayerId=10, PuzzleId=7431, TeamId = 0} },
            { 7461, new SubmissionInfo{ AnswerText = "EMERSON", PlayerId=11, PuzzleId=7461, TeamId = 0} },
            { 7435, new SubmissionInfo{ AnswerText = "GARDENIA", PlayerId=11, PuzzleId=7435, TeamId = 0} },
            { 7459, new SubmissionInfo{ AnswerText = "SNARK", PlayerId=11, PuzzleId=7459, TeamId = 0} },
        };

        private Dictionary<int, SubmissionInfo> _submissions;

        static HttpClient HttpClient { get; } = new HttpClient();

        public TriggerAdminAnswerSubmission()
        {
            _submissions = new Dictionary<int, SubmissionInfo>();
        }

        [FunctionName("TriggerAdminAnswerSubmission")]
        public void Run([TimerTrigger("0 */1 * * * *")] TimerInfo myTimer, ILogger log)
        {
            var allTeams = GetAllTeamInfo();
            string code = GenerateDictionaryCodeForTeamList(allTeams);
            File.WriteAllText(@"C:\Users\atrad\source\repos\mainpuzzleserver\PuzzleTimerTrigger\FullTeamInfo.cs", code);
            return;

            //https://puzzlehunt.azurewebsites.net
            const string eventId = "pd24";
            var response = HttpClient.GetAsync($"http://localhost:44319/api/puzzleapi/state/puzzleunlockstate/{eventId}?minutes=30&timerWindow=20").Result.Content;
            var unlocked = response.ReadFromJsonAsync<List<UnlockDetail>>().Result;

            if (unlocked != null)
            {
                foreach (UnlockDetail unlockedPuzzle in unlocked)
                {
                    //AdminAnswerSubmission submission = new()
                    //{
                    //    AllowFreeformSharing = true,
                    //    EventPassword = "1d5cb3a1-7a73-4c34-b680-6e90c072972c",
                    //    SubmissionText = "time check two",
                    //    SubmitterDisplayName = "DisplayingName",
                    //    Timestamp = DateTime.Now,
                    //};
                    //HttpClient.PostAsJsonAsync("http://localhost:44319/api/puzzleapi/submitanswer/2/19/1", submission).Wait();

                    // Find the matching entry and send the answer submission (using current time & hardcoded values in this version)
                    if(_submissions.ContainsKey(unlockedPuzzle.PuzzleId))
                    {
                        AdminAnswerSubmission submission = new()
                        {
                            AllowFreeformSharing = false,
                            EventPassword = "",
                            SubmissionText = _submissions[unlockedPuzzle.PuzzleId].AnswerText,
                        };

                        HttpClient.PostAsJsonAsync($"https://puzzlehunt.azurewebsites.net/api/puzzleapi/submitanswer/ph24beta/{unlockedPuzzle.PuzzleId}/{_submissions[unlockedPuzzle.PuzzleId]}", submission).Wait();
                    }

                }
            }
            // Call this so it stops being too dark to see
            SetUpPlayerAssignments(new TeamConfiguration(), "");

            //var unlocked = await client.GetFromJsonAsync<List<UnlockDetail>>("http://localhost:44319/api/puzzleapi/state/puzzleunlockstate/2?minutes=10000");
        }

        private void SetUpPlayerAssignments(TeamConfiguration team, string eventPassword)
        {
            int defaultPlayerId = 1;
            Dictionary<int, SubmissionInfo> _submissions;

            if (team.IsRemote)
            {
                _submissions = RemotePuzzleToSubmissionBaseMapping;
            }
            else
            {
                _submissions = LocalPuzzleToSubmissionBaseMapping;
            }

                foreach (PlayerClassesEnum pc in Enum.GetValues(typeof(PlayerClassesEnum)))
                {
                    if (team.Players.ContainsKey(pc))
                    {
                        // Set up the player's submissions
                        if (team.SusPlayerId == team.Players[pc])
                        {
                            // Make sure we get the imposter we want (don't forget that they have to have a name that can be an answer)
                            continue;
                        }

                        // Get the submissions for the role and assign them to the player
                        AdminAnswerSubmission submission = new()
                        {
                            AllowFreeformSharing = false,
                            EventPassword = eventPassword,
                            // SubmissionText = _submissions[]
                        };

                    }
                    else
                    {
                        // Pick a player id for the actual submitter and then add the submitter display name based on the enum above
                    }

                    defaultPlayerId++;
                }
        }
    }

    public class SubmissionInfo
    {
        public int TeamId { get; set; }
        public int PuzzleId { get; set; }
        public int PlayerId { get; set; }
        public string AnswerText { get; set; }

        // This is only used for making original assignments
        public PlayerClassesEnum PcAssignedToSubmission { get; set; }
    }

    public class UnlockDetail
    {
        public int TeamId { get; set; }
        public int PuzzleId { get; set; }
        public DateTime UnlockTime { get; set; }
    }

    public class AdminAnswerSubmission
    {
        public bool AllowFreeformSharing { get; set; }
        public string EventPassword { get; set; }
        public string SubmissionText { get; set; }
        public string SubmitterDisplayName { get; set; }
        public DateTime Timestamp { get; set; }
    }

    public class TeamConfiguration
    {
        public int TeamId { get; set; }
        public Dictionary<PlayerClassesEnum, int> Players { get; set; }
        public int SusPlayerId { get; set; }
        public bool IsRemote { get; set; }
    }

    public enum PlayerClassesEnum 
    {
        capt,
        exo,
        warrant,
        navigator,
        science,
        engineer,
        mechanic,
        medic,
        security,
        comms,
        eva,
        cargo
    }
}
