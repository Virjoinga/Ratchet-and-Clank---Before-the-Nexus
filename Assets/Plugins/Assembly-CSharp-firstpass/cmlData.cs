using System.Collections.Generic;

public class cmlData
{
	public string data_type;

	public Dictionary<string, string> defined;

	public List<string> data;

	public string this[string field_name]
	{
		get
		{
			return GetField(field_name);
		}
	}

	public virtual int ID
	{
		get
		{
			return int.Parse(defined["id"]);
		}
		set
		{
			Set("id", value.ToString());
		}
	}

	public cmlData(int id = 0)
	{
		data = new List<string>();
		defined = new Dictionary<string, string>();
		ID = id;
	}

	public virtual void Set(string name, string data)
	{
		if (defined.ContainsKey(name))
		{
			defined[name] = data;
		}
		else
		{
			defined.Add(name, data);
		}
	}

	public virtual string GetField(string named)
	{
		string value = string.Empty;
		if (defined.TryGetValue(named, out value))
		{
			return value;
		}
		return string.Empty;
	}

	public virtual void AddToData(string value)
	{
		data.Add(value);
	}

	public virtual void ProcessCombinedFields(string combined)
	{
		if (combined == string.Empty)
		{
			return;
		}
		string[] array = combined.Split(';');
		string[] array2 = array;
		foreach (string text in array2)
		{
			if (text.IndexOf('=') == -1)
			{
				AddToData(text);
				continue;
			}
			string[] array3 = text.Split('=');
			Set(array3[0], array3[1]);
		}
	}
}
