using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuickVSOpen
{
    public interface ISearchable
    {
        int CandidateCount { get; }
        SearchEntry Candidate(int i);

        void UpdateSearchQuery(string query, bool incremental);

        void Refresh();

        DateTime LastRefresh { get; }

        int LastRefreshDurationMS { get; }
    }
}