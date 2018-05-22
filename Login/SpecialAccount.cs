using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Login
{
	public class SpecialAccount
	{
		public bool CanLoginByUserName { get; private set; }

		private Dictionary<string, string> mGMAccounts = new Dictionary<string, string>();

		public Dictionary<string, string> GMAccounts
		{
			get { return mGMAccounts; }
		}

		private List<string> mWhiteAccounts = new List<string>();

		public List<string> WhiteAccounts
		{
			get { return mWhiteAccounts; }
		}

		private List<string> mBlackAccounts = new List<string>();

		public List<string> BlackAccounts
		{
			get { return mBlackAccounts; }
		}
        private Dictionary<int,string> mClientConfig = new Dictionary<int, string>();

	    public Dictionary<int, string> ClientConfig
	    {
            get { return mClientConfig;}
	    }


		public void LoadConfig()
		{
			dynamic accConfig = JsonConfig.Config.ApplyJsonFromPath("../Config/account.config");

			CanLoginByUserName = (bool)accConfig.CanLoginByUserName;
			foreach (var val in accConfig.GMAccounts)
			{
				if (!GMAccounts.ContainsKey(val.Account))
				{
					GMAccounts.Add(val.Account, val.Password);
				}
			}
			foreach (var val in accConfig.WhitList)
			{
				if (!mWhiteAccounts.Contains(val))
				{
					mWhiteAccounts.Add(val);
				}
			}
			foreach (var val in accConfig.BlackList)
			{
				if (!mBlackAccounts.Contains(val))
				{
					mBlackAccounts.Add(val);
				}
			}
		    foreach (var val in accConfig.ConfigList)
		    {
		        var key = int.Parse(val.Key);
		        if (!mClientConfig.ContainsKey(key)) 
		            mClientConfig.Add(key,val.Value);
		    }
		}
	}
}
