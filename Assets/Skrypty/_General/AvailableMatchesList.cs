using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.Networking.Match;

class AvailableMatchesList
{
    public static event Action<List<MatchInfoSnapshot>> OnAvailableMatchesChanged = delegate { };

    private static List<MatchInfoSnapshot> matches = new List<MatchInfoSnapshot>();

    public static void HandleNewMatchList( List<MatchInfoSnapshot> matchList)
    {
        matches = matchList;
        OnAvailableMatchesChanged(matches);
    }
}

