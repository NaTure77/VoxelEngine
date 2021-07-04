using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class ScrollUI : MonoBehaviour
{
    public RectTransform viewPort;
    public RectTransform container;
    List<ScrollData> dataList_origin;
    List<ScrollData> dataList;
    public Scrollbar scrollbar;

    public RectTransform elementPrefab;

	public ScrollRect scrollRect;

	private int lastItemNo = 0;
	private int instantateItemCount = 0;

	public static ScrollUI instance;

	float length = 0;
	// public WorkSelector workSelector;
	private void Awake()
    {
		//UIManager.scroll = this;
		instance = this;
		Init();
    }
	public Action OnDisable;
	/*private void Start()
	{
		EnableUI("Testing", MakeList(),false);
	}*/
	public static List<ScrollData> MakeDataList(string[] filters, UnityAction<string> callback)
	{
		DirectoryInfo di = new DirectoryInfo(Application.persistentDataPath);
		if (di.Exists == false) { di.Create(); }
		string[] list = filters.SelectMany(f => Directory.GetFiles(Application.persistentDataPath, f)).ToArray();
		List<ScrollData> result = new List<ScrollData>();
		Array.Sort(list);
		for (int i = 0; i < list.Length; i++)
		{
			string filePath = list[i];
			string fileName = Path.GetFileName(filePath);
			ScrollData data = new ScrollData
			{
				path = filePath,
				name = fileName,// sb.ToString();
				action = () =>
				//callback(FileManager<T>.LoadFile_ZF(filePath), Path.GetFileNameWithoutExtension(filePath), extension),
				callback(filePath),
				creationTime = new FileInfo(filePath).CreationTime
			};
			result.Add(data);
		}
		return result;
	}
	void Init()
	{
		//scrollRect 초기화
		scrollRect.horizontal = false;
		scrollRect.vertical = true;
		scrollRect.content = container;
		scrollRect.scrollSensitivity = 50;
		length = viewPort.rect.height / 5f;

		instantateItemCount = 7;
		beforeAnchoredPos = anchoredPosition;
		lastItemNo = instantateItemCount - 1;
		for (int i = 0; i < instantateItemCount; i++)
		{
			ScrollElement element = MakeNewElement(i);
			AddElement(element, i);
			element.SetActive(false);
		}
	}

	void RefreshList()
	{
		List<ScrollData> list = new List<ScrollData>();
		for(int i = 0; i < dataList_origin.Count; i++)
		{
			if(File.Exists(dataList_origin[i].path))
			{
				list.Add(dataList_origin[i]);
			}
		}
		UpdateList(list);
	}
	public void SetMaxPosition()
	{
		//halfWidth - halfBitGap - bitGap* (DataManager.bitNum)
		//maxPosition = -(bitGap * (DataManager.bitNum)) + halfWidth + halfBitGap;
		float newHeight = Mathf.Max(1f, dataList.Count * length);
		container.sizeDelta = new Vector2(container.sizeDelta.x, newHeight);
		maxPosition = Math.Max((length * (dataList.Count)) - minPosition - viewPort.rect.height,0);
	}

	ScrollElement MakeNewElement(int i)
	{
		RectTransform item = Instantiate(elementPrefab) as RectTransform;
		item.SetParent(container, false);

		Vector2 s = item.sizeDelta;
		s.x = 0;
		s.y = length;
		item.sizeDelta = s;
		ScrollElement element = item.gameObject.AddComponent<ScrollElement>();
		element.Init(item.GetComponentInChildren<Text>());

		item.GetComponent<Button>().onClick.AddListener(element.Invoke);
		item.transform.GetChild(1).GetComponent<Button>().onClick.AddListener(()=>
		{
			element.Delete();
			RefreshList();
		});
		element.ID = i;
		return element;
	}
	public void AddElement(ScrollElement element, int i)
	{
		element.ID = i;
		element.gameObject.SetActive(true);
		((RectTransform)element.transform).anchoredPosition = new Vector2(0, length * -i);

		if (First != null)
		{
			Last.Next = element;
			First.Prev = element;
			element.Prev = Last;
			element.Next = First;
			Last = element;
		}
		else
		{
			First = Last = element;
		}
	}

	public void EnableUI(List<ScrollData> lists)
    {
		UpdateList(lists);
		Jump(0);
		OnValueChanged();
    }
	List<ScrollData> CopyList(List<ScrollData> origin)
	{
		return origin.ConvertAll(o => new ScrollData
		{
			path = o.path,
			name = o.name,
			action = o.action,
			creationTime = o.creationTime
		});
	}

    public void DisableUI()
    {
		OnDisable();
    }

	public void Jump(int id)
	{
		scrollRect.velocity = Vector2.zero;
		container.anchoredPosition = new Vector2(container.anchoredPosition.x, -(minPosition - length * (id)));
	}
	private float anchoredPosition
	{
		get
		{
			return container.anchoredPosition.y;
		}
	}
	float beforeAnchoredPos = 0;
	float diffPreFramePosition = 0;
	float minPosition = 0;
	float maxPosition = 0;

	ScrollElement First;
	ScrollElement Last;

	float delta = 0;
	public void OnValueChanged()
	{
		//가장 위쪽
		if (anchoredPosition > maxPosition) // 잘 안되면 아이템 리스트 첫번째 좌표값이 minpos보다 작거나 같을때로 변경
		{
			container.anchoredPosition = new Vector2(container.anchoredPosition.x,maxPosition);
			scrollRect.velocity = Vector2.zero;

			
			//return;
		}
		//가장 아래쪽
		else if (anchoredPosition < minPosition)// 악보 가장 오른쪽
		{
			container.anchoredPosition = new Vector2(container.anchoredPosition.x, minPosition);
			scrollRect.velocity = Vector2.zero;
			//return;
		}
		delta = beforeAnchoredPos - anchoredPosition;
		beforeAnchoredPos = anchoredPosition;
		//요소들이 아래로 감. 아래로 스크롤. 끝에 있는놈이 처음으로 와야 함.
		if (delta > 0)
		{
			//diff가 anchoredPosition이 될 때까지 length를 계속 뺌.
			while (anchoredPosition < diffPreFramePosition)//-bitGap* 2 /* &&lastItemNo < DataManager.bitNum*/)
			{
				diffPreFramePosition -= length;

				
				Last.ID = lastItemNo - instantateItemCount;
				if (Last.ID >= 0 && Last.ID < dataList.Count)
				{
					Last.Set(dataList[Last.ID]);
				}
				/*if(Last.ID >= 0)
				{
					Last.Set(Last.ID);
				}*/
				else Last.SetActive(false);
				((RectTransform)Last.transform).anchoredPosition = -Vector2.up * (length * Last.ID);
				Last = Last.Prev;
				lastItemNo--;

			}
		}
		else if (delta < 0)
		{
			//요소들이 위로 감. 위로 스크롤. 처음에 있는 놈들이 끝으로 가야 됨.
			while (anchoredPosition > diffPreFramePosition + length/*&& firstItemNo > 0*/)
			{
				diffPreFramePosition += length;

				lastItemNo++;
				First = Last.Next;
				First.ID = lastItemNo;

				if (First.ID < dataList.Count)
				{
					First.Set(dataList[First.ID]);
				}
				/*if (First.ID < dataList.Count)
				{
					First.Set(First.ID);
				}*/
				else First.SetActive(false);
				((RectTransform)First.transform).anchoredPosition = -Vector2.up * (length * First.ID);
				Last = Last.Next;
			}
		}
	}
	public void UpdateList(List<ScrollData> lists)
	{
		dataList_origin = lists;
		dataList = CopyList(dataList_origin);
		SetMaxPosition();
		UpdateState();
	}
	public void UpdateState()
	{
		ScrollElement element = Last.Next;//First;
		for (int i = 0; i < instantateItemCount; i++)
		{
			//Debug.Log(box.ID);
			if (element.ID >= 0 && element.ID < dataList.Count)
			{
				element.Set(dataList[element.ID]);
			}
			else
			{
				element.SetActive(false);
			}
			element = element.Next;
		}

	}
	public void SortByName()
	{
		dataList_origin = dataList_origin.OrderBy(d => d.name).ToList();
		dataList = dataList.OrderBy(d => d.name).ToList();
		UpdateState();
		Jump(0);
	}
	public void SortByTime()
	{
		dataList_origin = dataList_origin.OrderByDescending(d => d.creationTime).ToList();
		dataList = dataList.OrderByDescending(d => d.creationTime).ToList();
		UpdateState();
		Jump(0);
	}
	public void Search(string s)
	{
		dataList = CopyList(dataList_origin);
		var a = from d in dataList
				where d.name.IndexOf(s, StringComparison.OrdinalIgnoreCase) >= 0
				select d;


		dataList = a.ToList();

		SetMaxPosition();
		UpdateState();
		Jump(0);
	}
}
public static class StringExtensions
{
	public static bool Contains(this string source, string toCheck, StringComparison comp)
	{
		return source?.IndexOf(toCheck, comp) >= 0;
	}
}
public class ScrollElement : MonoBehaviour
{
    ScrollData data;
    Text text;
	string filePath;
	public ScrollElement Next;
	public ScrollElement Prev;

	public int ID;

	public void Init(Text t)
	{
		text = t;
	}

	public void SetActive(bool b)
	{
		if(gameObject.activeSelf != b)
			gameObject.SetActive(b);
	}
    public void Set(ScrollData d)
    {
		SetActive(true);
		filePath = d.path;
		data = d;
        text.text = d.name;
	}

    public void Invoke()
    {
        data.action();
    }

	public void Delete()
	{
		
		File.Delete(filePath);
	}
}

public class ScrollData
{
    public UnityAction action;
	public DateTime creationTime;
	public string path;
    public string name;
}
