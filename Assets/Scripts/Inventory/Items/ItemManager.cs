using System.Collections;
using System.Collections.Generic;
using Fram.Save;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Fram.Inventory
{
	public class ItemManager : MonoBehaviour,ISaveable
	{
		public Item itemPrefab;
		public Item bounceItemPrefab;
		private Transform itemParent;

		private Transform PlayerTransform => FindObjectOfType<Player>().transform;

		public string GUID => GetComponent<DataGUID>().guid;

		// ��¼�����е�item
		private Dictionary<string, List<SceneItem>> sceneItemDict = new Dictionary<string, List<SceneItem>>();
		// ��¼�����е�Furniture
		private Dictionary<string, List<SceneFurniture>> sceneFurnitureDict = new Dictionary<string, List<SceneFurniture>>();
		private Dictionary<string,List<int>> startBoxes = new Dictionary<string, List<int>>();
		
		private void Start() {
			ISaveable saveable = this;
			saveable.RegisterSaveable();
		}
		
		private void OnEnable()
		{
			EventHandler.InstantiateItemInScene += OnInstantiateItemInScene;
			EventHandler.BeforeSceneUnloadEvent += OnBeforeSceneUnloadEvent;
			EventHandler.AfterSceneUnloadEvent += OnAfterSceneUnloadEvent;
			EventHandler.DropItemEvent += OnDropItemEvent;
			EventHandler.BuildFurnitureEvent += OnBuildFurnitureEvent;
			EventHandler.StartNewGameEvent += OnStartNewGameEvent;
		}


/*        private void Start()
		{
			itemParent = GameObject.FindWithTag("ItemParent").transform;
		}*/

		private void OnDisable()
		{
			EventHandler.InstantiateItemInScene -= OnInstantiateItemInScene;
			EventHandler.BeforeSceneUnloadEvent -= OnBeforeSceneUnloadEvent;
			EventHandler.AfterSceneUnloadEvent -= OnAfterSceneUnloadEvent;
			EventHandler.DropItemEvent -= OnDropItemEvent;
			EventHandler.BuildFurnitureEvent -= OnBuildFurnitureEvent;
			EventHandler.StartNewGameEvent -= OnStartNewGameEvent;
		}

		private void OnStartNewGameEvent(int obj)
		{
			sceneItemDict.Clear();
			sceneFurnitureDict.Clear();
		}

		private void OnBeforeSceneUnloadEvent()
		{
			GetAllSceneItems();
			GetAllSceneFurnitures();
		}

		private void OnAfterSceneUnloadEvent()
		{
			itemParent = GameObject.FindWithTag("ItemParent").transform;
			RecreateAllItems();
			RecreateAllFurnitures();
			//RecreateAllBoxes();
		}
		/// <summary>
		/// ����ͼֽ�еĽ�����Ʒ
		/// </summary>
		/// <param name="itemID"></param>
		/// <param name="mousePosOnWorld"></param>
		private void OnBuildFurnitureEvent(int itemID, Vector3 mousePosOnWorld)
		{
			var bluePrintPrefab = InventoryManager.Instance.bluePrintDataList_SO.GetBluePrintDetails(itemID).buildPrefab;
			
			var buildItem = Instantiate(bluePrintPrefab,mousePosOnWorld,quaternion.identity,itemParent);
			
			if(buildItem.GetComponent<Box>())
			{
				// ���������Ʒ�����ӵĻ�����ô��Ҫ�ȶ����ı�Ž��г�ʼ����ֵ
				buildItem.GetComponent<Box>().index = InventoryManager.Instance.boxDataList.Count;
				buildItem.GetComponent<Box>().Init(buildItem.GetComponent<Box>().index);
			}
		}

		/// <summary>
		/// ��ʼ�������е�item
		/// </summary>
		/// <param name="ID">item��ID</param>
		/// <param name="pos">item��position</param>
		private void OnInstantiateItemInScene(int ID, Vector3 position)
		{
			var item = Instantiate(bounceItemPrefab, position, Quaternion.identity, itemParent);
			item.itemID = ID;
			item.GetComponent<ItemBounce>().InitBounceItem(position,Vector3.up);
		}

		/// <summary>
		/// �ӵ���Ʒ���¼����������ֱ��������scene����������Ϊ�һ�Ҫ�ı�bag���������
		/// </summary>
		/// <param name="ID">�ӵ���Ʒ��ID</param>
		/// <param name="position">Ҫ�ӵ���λ��</param>
		private void OnDropItemEvent(int ID, Vector3 mousePos,itemType itemType)
		{
			if(itemType == itemType.Seed) return;
			var item = Instantiate(bounceItemPrefab, PlayerTransform.position, Quaternion.identity, itemParent);
			item.itemID = ID;
			var dir = (mousePos - PlayerTransform.position).normalized;
			item.GetComponent<ItemBounce>().InitBounceItem(mousePos, dir);
		}

		/// <summary>
		/// �������е�item��ӽ����ֵ���
		/// </summary>
		private void GetAllSceneItems()
		{
			List<SceneItem> currentSceneItems = new List<SceneItem>();
			foreach(var item in FindObjectsOfType<Item>())
			{
				SceneItem sceneItem = new SceneItem
				{
					itemID = item.itemID,
					position = new SerializableVector3(item.transform.position)
				};
				currentSceneItems.Add(sceneItem);
			}

			// ��item����Ϣ�Ž���������
			if (sceneItemDict.ContainsKey(SceneManager.GetActiveScene().name))
			{
				sceneItemDict[SceneManager.GetActiveScene().name] = currentSceneItems;
			}
			else
			{
				sceneItemDict.Add(SceneManager.GetActiveScene().name, currentSceneItems);
			}
		}
		
		/// <summary>
		/// ��ȡ�����еĽ��������ʵ���л�����ʱ�������ݵ�
		/// </summary>
		private void GetAllSceneFurnitures()
		{
			List<SceneFurniture> currentSceneItems = new List<SceneFurniture>();
			foreach(var item in FindObjectsOfType<Furniture>())
			{
				SceneFurniture sceneItem = new SceneFurniture
				{
					furnitureID = item.itemID,
					position = new SerializableVector3(item.transform.position)
				};
				if(item.GetComponent<Box>())
				{
					sceneItem.boxIndex = item.GetComponent<Box>().index;
				}
				currentSceneItems.Add(sceneItem);
			}

			// ��item����Ϣ�Ž���������
			if (sceneFurnitureDict.ContainsKey(SceneManager.GetActiveScene().name))
			{
				sceneFurnitureDict[SceneManager.GetActiveScene().name] = currentSceneItems;
			}
			else
			{
				sceneFurnitureDict.Add(SceneManager.GetActiveScene().name, currentSceneItems);
			}
		}
		
		// private void GetAllStartBoxes()
		// {
		// 	List<int> items = new List<int>();
		// 	foreach(var item in FindObjectsOfType<StartBox>())
		// 	{
		// 		//Debug.Log(item.gameObject.name);
		// 		items.Add(item.GetComponent<Box>().index);
		// 	}
			
		// 	if(startBoxes.ContainsKey(SceneManager.GetActiveScene().name))
		// 	{
		// 		startBoxes[SceneManager.GetActiveScene().name] = items;
		// 	}else
		// 	{
		// 		startBoxes.Add(SceneManager.GetActiveScene().name,items);
		// 	}
		// }


		/// <summary>
		/// ˢ���ؽ���ǰ��������Ʒ
		/// </summary>
		private void RecreateAllItems()
		{
			List<SceneItem> currentSceneItems = new List<SceneItem>();
			if (sceneItemDict.TryGetValue(SceneManager.GetActiveScene().name, out currentSceneItems)){
				if (currentSceneItems != null)
				{
					// �峡
					foreach (var item in FindObjectsOfType<Item>())
					{
						Destroy(item.gameObject);
					}

					foreach (var item in currentSceneItems)
					{
						Item newItem = Instantiate(itemPrefab, item.position.ToVector3(), Quaternion.identity,itemParent);
						newItem.Init(item.itemID);
					}
				}
			}
		}
		
		
		/// <summary>
		/// �л�����֮���������ɽ�����
		/// </summary>
		private void RecreateAllFurnitures()
		{
			//Debug.Log("furnitureRecreate");
			List<SceneFurniture> currentSceneFurnitures = new List<SceneFurniture>();
			if(sceneFurnitureDict.TryGetValue(SceneManager.GetActiveScene().name,out currentSceneFurnitures))
			{
				if(currentSceneFurnitures != null)
				{
					foreach(SceneFurniture furniture in currentSceneFurnitures)
					{
						var bluePrintPrefab = InventoryManager.Instance.bluePrintDataList_SO.GetBluePrintDetails(furniture.furnitureID).buildPrefab;
						
						var builItem = Instantiate(bluePrintPrefab,furniture.position.ToVector3(),quaternion.identity,itemParent);
						if(builItem.GetComponent<Box>())
						{
							builItem.GetComponent<Box>().Init(furniture.boxIndex);
						}
					}
				}
			}
		}
		
		// private void RecreateAllBoxes()
		// {
		// 	List<int> box = new List<int>();
		// 	StartBox[] obj = FindObjectsOfType<StartBox>();
		// 	if(startBoxes.TryGetValue(SceneManager.GetActiveScene().name,out box))
		// 	{
		// 		if(box != null)
		// 		{
		// 			for(int i = 0;i < box.Count;i++)
		// 			{
		// 				Debug.Log(box[i]);
		// 				for(int j = 0 ;i<obj.Length;j++)
		// 				{
		// 					if(obj[i].GetComponent<Box>().index == box[i])
		// 					{
		// 						obj[i].GetComponent<Box>().InitStartBox();
		// 						Debug.Log("init");
		// 					}
		// 				}
		// 			}
		// 		}else
		// 		{
		// 			Debug.Log("null");
		// 		}
		// 	}
		// }

		/// <summary>
		/// �������е���Ʒ�ͼҾߵ����ݴ洢����
		/// </summary>
		/// <returns></returns>
		public GameSaveData gameSaveData()
		{
			
			GetAllSceneItems();
			GetAllSceneFurnitures();
			//GetAllStartBoxes();
			GameSaveData saveData = new GameSaveData();
			saveData.sceneItemDict = this.sceneItemDict;
			saveData.sceneFurnitureDic = this.sceneFurnitureDict;
			//saveData.sceneBoxes = this.startBoxes;
			
			return saveData;
		}

		/// <summary>
		/// ��ȡ�Ҵ洢����Ʒ�ͼҾߵ�����
		/// </summary>
		/// <param name="saveData"></param>
		public void RestoreData(GameSaveData saveData)
		{
			this.sceneItemDict = saveData.sceneItemDict;
			this.sceneFurnitureDict = saveData.sceneFurnitureDic;
			//this.startBoxes = saveData.sceneBoxes;
			
			RecreateAllItems();
			RecreateAllFurnitures();
			//RecreateAllBoxes();
		}
	}
}

