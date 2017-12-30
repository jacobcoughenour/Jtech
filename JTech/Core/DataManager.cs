using System;
using System.Collections;
using System.Collections.Generic;

namespace Oxide.Plugins.JTechCore {

	public static class DataManager {
		
		public class StoredData {
			public Dictionary<int, JDeployableManager.DeployableSaveData> d = new Dictionary<int, JDeployableManager.DeployableSaveData>();
		}

		public static StoredData data;

		public static void Load() {

			data = new StoredData();
			LoadData(ref data);
		}

		public static void Save() {
			
			SaveData(data);
		}

		private static void LoadData<T>(ref T d) => d = Core.Interface.Oxide.DataFileSystem.ReadObject<T>("JTech");
		private static void SaveData<T>(T d) => Core.Interface.Oxide.DataFileSystem.WriteObject("JTech", d);
	}
}