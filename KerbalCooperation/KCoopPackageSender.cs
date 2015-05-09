using System;
using System.Net.Sockets;
using System.Xml;
using System.Xml.Serialization;

namespace KCoop
{
	[Serializable()]
	public class KCoopPackageData: ICloneable
	{
		public string opType;
		public int argc;
		public string argv;
		public Object Clone()
		{
			return this.MemberwiseClone ();
		}
	}

	public class KCoopPackageSender
	{
		private readonly string server = "192.99.14.46";
		private readonly int port = 15565;
		private KCoopPackageData data;

		public KCoopPackageSender (KCoopPackageData data)
		{
			this.data = (KCoopPackageData)data.Clone();
		}

		public bool send()
		{
			try
			{
				TcpClient client = new TcpClient(server, port);
				XmlSerializer seriallizer = new XmlSerializer(typeof(KCoopPackageData));
				NetworkStream stream = client.GetStream();
				seriallizer.Serialize(stream, data);
				stream.Close();
				client.Close();
				return true;
			} catch (Exception e) {
				Logger.error ("Error while sending data: " + e.ToString());
				return false;
			}
		}
	}
}

