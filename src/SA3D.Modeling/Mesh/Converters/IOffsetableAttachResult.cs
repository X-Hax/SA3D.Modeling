using System.Collections.Generic;
using System.Linq;

namespace SA3D.Modeling.Mesh.Converters
{
	internal interface IOffsetableAttachResult
	{
		public string Label { get; }
		public int VertexCount { get; }
		public bool Weighted { get; }
		public int[] AttachIndices { get; }
		public Attach[] Attaches { get; }

		public void ModifyVertexOffset(int offset);

		/// <summary>
		/// Checks for any vertex overlaps in the models and sets their vertex offset accordingly
		/// </summary>
		public static void PlanVertexOffsets<T>(T[] attaches) where T : IOffsetableAttachResult
		{
			if(attaches.Length == 0)
			{
				return;
			}

			int nodeCount = attaches.Max(x => x.AttachIndices[^1]) + 1;
			List<(int start, int end)>[] ranges = new List<(int start, int end)>[nodeCount];
			for(int i = 0; i < nodeCount; i++)
			{
				ranges[i] = [];
			}

			foreach(IOffsetableAttachResult cr in attaches)
			{
				int startNode = cr.AttachIndices[0];
				int endNode = cr.AttachIndices[^1];

				// Map out the blocked regions
				Dictionary<int, int> blocked = [];
				for(int i = startNode; i <= endNode; i++)
				{
					foreach((int start, int end) r in ranges[i])
					{
						if(blocked.TryGetValue(r.start, out int end))
						{
							if(r.end > end)
							{
								blocked[r.start] = r.end;
							}
						}
						else
						{
							blocked.Add(r.start, r.end);
						}
					}
				}

				// find a free space in the blocked regions
				int prevEnd = 0;
				foreach(KeyValuePair<int, int> v in blocked.OrderBy(x => x.Key))
				{
					if(prevEnd > v.Key)
					{
						if(v.Value > prevEnd)
						{
							prevEnd = v.Value;
						}
					}
					else if(v.Key - prevEnd >= cr.VertexCount)
					{
						break;
					}
					else
					{
						prevEnd = v.Value;
					}
				}

				// Block the region used by the current attach
				for(int i = startNode; i <= endNode; i++)
				{
					ranges[i].Add((prevEnd, prevEnd + cr.VertexCount));
				}

				// adjust offset if needed
				if(prevEnd > 0)
				{
					cr.ModifyVertexOffset(prevEnd);
				}
			}
		}

	}
}
