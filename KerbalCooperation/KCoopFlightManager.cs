using System;
using System.Collections.Generic;
using System.Linq;
using KSP;
using UnityEngine;

namespace KCoop
{
	public class KCoopFlightManager: MonoBehaviour
	{
		private KerbalCooperation kcoop_model = null;

		public void Start()
		{
			Logger.log ("FlightManager started.");
			kcoop_model = KerbalCooperation.Instance;
			if (kcoop_model == null) 
			{
				Logger.error ("Cannot find KerbalCooperation instance!");
				return;
			}
		}

		public void Awake()
		{
			if (kcoop_model == null) 
			{
				kcoop_model = KerbalCooperation.Instance;
				Logger.error ("Cannot find KerbalCooperation instance!");
				return;
			}
		}
	}
}

