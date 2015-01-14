using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace KCoop
{
	public class KCoopTimer: ISaveAble
	{
		private DateTime lastdate;
		private TimeSpan counting;
		private bool pausing;
		public string name { get; private set;}

		public KCoopTimer (string n, bool s)
		{
			name = n;
			pausing = true;
			counting = System.TimeSpan.Zero;
			if (s) 
			{
				Start();
			}
		}

		public void Start()
		{
			if (pausing) {
				lastdate = System.DateTime.Now;
				pausing = false;
			}
		}

		public void Pause()
		{
			if (!pausing) {
				pausing = true;
				counting += (System.DateTime.Now - lastdate);
			}
		}

		public TimeSpan getTime() 
		{
			TimeSpan ret = System.TimeSpan.Zero;
			if (pausing) {
				ret = counting;
			} else {
				ret = counting + (System.DateTime.Now - lastdate);
			}

			return ret;
		}

		public void Load(ConfigNode node)
		{
			if (node.HasNode (this.name)) {
				string saveValue = node.GetValue (this.name);
				counting = TimeSpan.Parse (saveValue);
			}
		}


		public void Save(ConfigNode node)
		{
			if (node.HasValue (this.name)) {
				node.SetValue (this.name, counting.ToString());
			} else {
				node.AddValue (this.name, counting.ToString());
			}
		}
	}
}

