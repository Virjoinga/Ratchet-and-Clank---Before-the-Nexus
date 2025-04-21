using System.Collections.Generic;
using UnityEngine;

public class cmlReader
{
	public List<cmlData> Elements;

	public cmlData Node;

	public cmlData this[int index]
	{
		get
		{
			return (Count <= index) ? null : Elements[index];
		}
	}

	public virtual cmlData Last
	{
		get
		{
			return (Elements.Count != 0) ? Elements[Elements.Count - 1] : null;
		}
	}

	public virtual cmlData First
	{
		get
		{
			return (Elements.Count != 0) ? Elements[0] : null;
		}
	}

	public virtual int Count
	{
		get
		{
			return Elements.Count;
		}
	}

	public cmlReader(string filename = "")
	{
		Initialize();
		if (string.Empty != filename)
		{
			LoadFile(filename);
		}
	}

	public virtual void Initialize()
	{
		Node = null;
		Elements = new List<cmlData>();
	}

	public virtual bool LoadFile(string filename)
	{
		TextAsset textAsset = (TextAsset)Resources.Load(filename, typeof(TextAsset));
		if (null != textAsset)
		{
			return ParseFile(textAsset.text);
		}
		return false;
	}

	public virtual bool ParseFile(string data)
	{
		if (data == string.Empty)
		{
			return false;
		}
		string[] array = data.Split('\n');
		string[] array2 = array;
		foreach (string text in array2)
		{
			string text2 = text;
			int num = text2.IndexOf("//");
			if (num > -1)
			{
				text2 = text2.Substring(0, num);
			}
			text2 = text2.Trim();
			if (text2 == string.Empty || text2.IndexOf("</") == 0)
			{
				continue;
			}
			int num2 = text2.IndexOf('[');
			int num3 = text2.IndexOf(']');
			if (num3 > 1 && num2 == 0)
			{
				if (Last != null)
				{
					string name = text2.Substring(1, num3 - 1).Trim();
					text2 = text2.Substring(num3 + 1, text2.Length - (num3 + 1)).Trim();
					if (Last != null)
					{
						Last.Set(name, text2);
					}
				}
				continue;
			}
			num2 = text2.IndexOf('<');
			num3 = text2.IndexOf('>');
			if (num3 > 1 && num2 == 0)
			{
				string data_type = text2.Substring(1, num3 - 1).Trim();
				string add_data = text2.Substring(num3 + 1, text2.Length - (num3 + 1));
				AddNode(data_type, add_data);
			}
			else if (Last != null)
			{
				Last.AddToData(text2);
			}
		}
		Node = ((Count <= 0) ? null : First);
		return true;
	}

	public virtual bool AddNode(string data_type, string add_data = "")
	{
		if (data_type == string.Empty)
		{
			return false;
		}
		Elements.Add(new cmlData(Elements.Count));
		Last.data_type = data_type;
		Last.ProcessCombinedFields(add_data);
		return true;
	}

	public virtual bool PositionAtFirstNode()
	{
		if (Count > 0)
		{
			Node = Elements[0];
			return true;
		}
		return false;
	}

	public virtual bool PositionAtLastNode()
	{
		if (Count > 0)
		{
			Node = Elements[Count - 1];
			return true;
		}
		return false;
	}

	public virtual bool PositionAtNextNode()
	{
		if (Node == null)
		{
			return false;
		}
		int index = Node.ID + 1;
		return PositionAtNode(index);
	}

	public virtual bool PositionAtPreviousNode()
	{
		if (Node == null)
		{
			return false;
		}
		int index = Node.ID - 1;
		return PositionAtNode(index);
	}

	public virtual bool PositionAtNode(int index)
	{
		if (index < 0)
		{
			return false;
		}
		if (Count > index)
		{
			Node = Elements[index];
			return true;
		}
		return false;
	}

	public virtual bool RemoveCurrentNode()
	{
		return RemoveNode(Node.ID);
	}

	public virtual bool RemoveNode(int index)
	{
		if (Count <= index)
		{
			return false;
		}
		if (Node.ID == index)
		{
			if (index < Count - 1)
			{
				Node = Elements[index + 1];
			}
			else if (Node.ID == 0)
			{
				Node = null;
			}
			else
			{
				Node = Elements[index - 1];
			}
		}
		Elements.RemoveAt(index);
		for (int i = index; i < Elements.Count; i++)
		{
			Elements[i].ID = i;
		}
		return true;
	}

	public virtual bool InsertNode(string data_type, string add_data = "")
	{
		if (Node != null)
		{
			return InsertNode(Node.ID, data_type, add_data);
		}
		return AddNode(data_type, add_data);
	}

	public virtual bool InsertNode(int index, string data_type, string add_data = "")
	{
		if (data_type == string.Empty)
		{
			return false;
		}
		if (Count == 0)
		{
			return AddNode(data_type, add_data);
		}
		Elements.Insert(index, new cmlData(index));
		Node = Elements[index];
		Node.data_type = data_type;
		Node.ProcessCombinedFields(add_data);
		for (int i = index + 1; i < Count; i++)
		{
			Elements[i].ID = i;
		}
		return true;
	}

	public virtual cmlData GetFirstNodeOfType(string data_type)
	{
		if (Count == 0)
		{
			return null;
		}
		for (int i = 0; i < Count; i++)
		{
			if (Elements[i].data_type == data_type)
			{
				return Elements[i];
			}
		}
		return null;
	}

	public virtual int GetFirstNodeOfTypei(string data_type)
	{
		if (Count == 0)
		{
			return -1;
		}
		for (int i = 0; i < Count; i++)
		{
			if (Elements[i].data_type == data_type)
			{
				return i;
			}
		}
		return -1;
	}

	public virtual cmlData GetLastNodeOfType(string data_type)
	{
		if (Count == 0)
		{
			return null;
		}
		for (int num = Count - 1; num >= 0; num--)
		{
			if (Elements[num].data_type == data_type)
			{
				return Elements[num];
			}
		}
		return null;
	}

	public virtual int GetLastNodeOfTypei(string data_type)
	{
		if (Count == 0)
		{
			return -1;
		}
		for (int num = Count - 1; num >= 0; num--)
		{
			if (Elements[num].data_type == data_type)
			{
				return num;
			}
		}
		return -1;
	}

	public virtual bool PositionAtFirstNodeOfType(string data_type)
	{
		cmlData firstNodeOfType = GetFirstNodeOfType(data_type);
		if (firstNodeOfType != null)
		{
			PositionAtNode(firstNodeOfType.ID);
		}
		return null != firstNodeOfType;
	}

	public virtual List<cmlData> Children(int index = -1)
	{
		if (index == -1)
		{
			if (Node == null)
			{
				return null;
			}
			index = Node.ID;
		}
		if (Count <= index + 1)
		{
			return new List<cmlData>();
		}
		string data_type = Elements[index++].data_type;
		string data_type2 = Elements[index].data_type;
		if (data_type2 == data_type)
		{
			return new List<cmlData>();
		}
		List<cmlData> list = new List<cmlData>();
		list.Add(Elements[index]);
		for (index++; index < Count; index++)
		{
			if (Elements[index].data_type == data_type2)
			{
				list.Add(Elements[index]);
			}
			else if (Elements[index].data_type == data_type)
			{
				break;
			}
		}
		return list;
	}

	public virtual List<cmlData> AllChildNodes(int index = -1)
	{
		if (index == -1)
		{
			if (Node == null)
			{
				return null;
			}
			index = Node.ID;
		}
		if (Count <= index - 1)
		{
			return null;
		}
		string data_type = Elements[index].data_type;
		List<cmlData> list = new List<cmlData>();
		index++;
		while (index < Count && !(Elements[index].data_type == data_type))
		{
			list.Add(Elements[index]);
			index++;
		}
		if (list.Count > 0)
		{
			return list;
		}
		return null;
	}

	public virtual List<int> Childreni(int index = -1)
	{
		if (index == -1)
		{
			if (Node == null)
			{
				return null;
			}
			index = Node.ID;
		}
		if (Count <= index - 1)
		{
			return null;
		}
		string data_type = Elements[index++].data_type;
		string data_type2 = Elements[index].data_type;
		if (data_type2 == data_type)
		{
			return null;
		}
		List<int> list = new List<int>();
		list.Add(index);
		for (index++; index < Count; index++)
		{
			if (Elements[index].data_type == data_type2)
			{
				list.Add(index);
			}
			else if (Elements[index].data_type == data_type)
			{
				break;
			}
		}
		return list;
	}

	public virtual List<int> AllChildNodesi(int index = -1)
	{
		if (index == -1)
		{
			if (Node == null)
			{
				return null;
			}
			index = Node.ID;
		}
		if (Count <= index - 1)
		{
			return null;
		}
		string data_type = Elements[index].data_type;
		List<int> list = new List<int>();
		index++;
		while (index < Count && !(Elements[index].data_type == data_type))
		{
			list.Add(index);
			index++;
		}
		if (list.Count > 0)
		{
			return list;
		}
		return null;
	}

	public virtual List<cmlData> AllDataOfType(string type_name, int starting_index = 0, string stop_at_data_type = "")
	{
		List<cmlData> list = new List<cmlData>();
		if (starting_index >= Count)
		{
			return list;
		}
		for (int i = starting_index; i < Count && (!(stop_at_data_type != string.Empty) || !(Elements[i].data_type == stop_at_data_type)); i++)
		{
			if (Elements[i].data_type == type_name)
			{
				list.Add(Elements[i]);
			}
		}
		return list;
	}

	public virtual List<int> AllDataOfTypei(string type_name, int starting_index = 0, string stop_at_data_type = "")
	{
		List<int> list = new List<int>();
		if (starting_index >= Count)
		{
			return list;
		}
		for (int i = starting_index; i < Count && (!(stop_at_data_type != string.Empty) || !(Elements[i].data_type == stop_at_data_type)); i++)
		{
			if (Elements[i].data_type == type_name)
			{
				list.Add(i);
			}
		}
		return list;
	}
}
