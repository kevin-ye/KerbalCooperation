using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace KCoop
{
	public class KCoopTimer
	{
		private DateTime startdate;
		private DateTime enddate;
		public string name { get; private set;}

		public KCoopTimer (string n, bool s)
		{
			name = n;
			if (s) 
			{
				Start();
			}
		}

		public void Start()
		{
			startdate = System.DateTime.Now;
		}

		public void Pause()
		{
			enddate = System.DateTime.Now;
		}

		public void Save(ConfigNode saveNode)
		{
			TimeSpan ctime = enddate - startdate;
		}
	}
}

