using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace KCoop
{
	public class KCoopTimer: ISaveAble
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

		public void Load(ConfigNode node)
		{
		}


		public void Save(ConfigNode node)
		{
			TimeSpan ctime = enddate - startdate;
		}
	}
}

