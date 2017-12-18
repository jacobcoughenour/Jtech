using System;
using System.Collections;
using System.Collections.Generic;

namespace Oxide.Ext.JCore {

	public static class Data {

		public static void Load() {
			// TODO
		}

		public static void Save() {
			// TODO
		}

		private static void LoadData<T>(ref T data) => data = Core.Interface.Oxide.DataFileSystem.ReadObject<T>("JTech");
		private static void SaveData<T>(T data) => Core.Interface.Oxide.DataFileSystem.WriteObject("JTech", data);
	}
}