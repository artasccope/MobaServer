using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MobaServer.Logic.Match
{
    public class MatchRoom
    {
        public int id;
        public int teamMaxUserCount = 1;
        public List<int> teamOne = new List<int>();
        public List<int> teamTwo = new List<int>();

        public MatchRoom() { }

        public MatchRoom(int id, int teamMax) {
            this.id = id;
            this.teamMaxUserCount = teamMax;
        }

        public void SetTeamMaxUserCount(int teamMax) {
            this.teamMaxUserCount = teamMax;
        }

        public bool IsFull {
            get {
                return teamOne.Count == teamTwo.Count && teamOne.Count == teamMaxUserCount;
            }
        }
        public void Clear() {
            teamOne.Clear();
            teamTwo.Clear();
        }

        public bool RemoveUser(int userId) {
            if (this.teamOne.Contains(userId))
            {
                this.teamOne.Remove(userId);
            }
            else if (this.teamTwo.Contains(userId)) {
                this.teamTwo.Remove(userId);
            }

            return this.teamOne.Count + this.teamTwo.Count == 0;
        }
    }
}
