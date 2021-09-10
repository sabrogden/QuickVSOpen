using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace QuickVSOpen
{
	public class SearchEntry
	{
		public string fullPath = "";
		public string project = "";
		public string description = "";
		public string key = "";
		public string filename = "";
		public int? lineNumber = null;
		public string methodType = "";
		public long? lastUsed = null;

		public SearchEntry()
		{
		}

		public class CompareOnRelevance : IComparer<SearchEntry>
		{
			private string mQuery;

			public CompareOnRelevance(string query)
			{
				mQuery = query.ToLower();
			}

			public int Compare(SearchEntry lhs, SearchEntry rhs)
			{
				var relevanceUsed = (rhs.lastUsed.HasValue ? rhs.lastUsed.Value : 0).CompareTo((lhs.lastUsed.HasValue ? lhs.lastUsed.Value : 0));
				if (relevanceUsed != 0)
					return relevanceUsed;

				//if (lhs.lastUsed.HasValue && rhs.lastUsed.HasValue)
				//{
				//    return lhs.lastUsed.Value.CompareTo(rhs.lastUsed.Value);
				//}

				int relevance = (lhs.key.StartsWith(mQuery) ? 0 : 1) - (rhs.key.StartsWith(mQuery) ? 0 : 1);
				if (relevance != 0)
					return relevance;

				return string.Compare(lhs.key, rhs.key);
			}
		}
	}	
}
