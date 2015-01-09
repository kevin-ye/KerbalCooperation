using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace KCoop
{
	public interface ISaveAble 
	{
		void Load(ConfigNode node);
		void Save(ConfigNode node);
	}

}

