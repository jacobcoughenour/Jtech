using System;
using System.Collections;
using System.Collections.Generic;
using Oxide.Core;

namespace Oxide.Plugins.JTechCore {

	public static class DataManager {
		
		public class StoredData {
			public Dictionary<int, JDeployableManager.DeployableSaveData> d = new Dictionary<int, JDeployableManager.DeployableSaveData>();
		}

		public static StoredData data;

		public static void Load() {

			data = new StoredData();
			LoadData(ref data);

			if (data == null) {
				data = new StoredData();
				Interface.Oxide.LogWarning("[JTechCore] save data is null?  Creating new save data...");
				SaveData(data);
			}
		}

		public static void Save() {
			
			SaveData(data);
		}

		private static void LoadData<T>(ref T d) => d = Interface.Oxide.DataFileSystem.ReadObject<T>("JTech");
		private static void SaveData<T>(T d) => Interface.Oxide.DataFileSystem.WriteObject("JTech", d);
	}
}