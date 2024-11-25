using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Fram.Save
{
	public interface ISaveable
	{
		string GUID{get;}
		void RegisterSaveable()
		{
			SaveManager.Instance.RegisterSaveable(this);
		}
		GameSaveData gameSaveData();
		void RestoreData(GameSaveData saveData);
	}
}
