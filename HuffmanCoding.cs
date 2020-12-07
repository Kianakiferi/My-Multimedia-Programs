using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;

namespace MultimediaApp
{
	class HuffmanCoding
	{
		public class OneLineNode
		{
			public char Key { get; set; }
			public string Value { get; set; }

			public OneLineNode(string temp1, string temp2)
			{
				Key = temp1[0];
				Value = temp2;
			}

			public OneLineNode(char temp1, string temp2)
			{
				Key = temp1;
				Value = temp2;
			}

			public OneLineNode(string value)
			{ 
				Key = value[0];
				Value = value.Substring(2);
			}

			public override string ToString()
			{
				return String.Concat(Key + " " + Value + "\n");
			}

		}

		public class Node : IComparable
		{
			public Node LChild { get; set; }
			public Node RChild { get; set; }
			public int Weight { get; set; }
			public char Key { get; set; }

			public Node() { }
			public Node(int weight, char value)
			{
				this.Weight = weight;
				this.Key = value;
			}

			//比较接口，用于排序
			public int CompareTo(object sender)
			{
				int result = 0;
				Node temp = sender as Node;

				if (temp.Weight > this.Weight)
				{
					result = 1;
				}
				else if (temp.Weight < this.Weight)
				{
					result = -1;
				}

				return result;
			}
		}

		// 获得权值数组
		private Node[] GetWeightArray(string str)
		{
			List<Node> result = new List<Node>();

			char[] charArray = str.ToCharArray();

			while (charArray.Length > 0)
			{
				char[] cntChars = null;
				var temp = charArray.Where(m => m == charArray[0]);
				cntChars = new char[temp.Count()];
				temp.ToArray().CopyTo(cntChars, 0);
				charArray = charArray.Where(m => m != temp.First()).ToArray();

				result.Add(new Node(cntChars.Length, cntChars[0]));
			}

			return result.ToArray();
		}

		/// <summary>
		/// 构建哈弗曼树
		/// </summary>
		/// <param name="sources">权值数组</param>
		/// <returns>返回哈弗曼树根</returns>
		private Node CreateHuffmanTree(Node[] sources)
		{
			Node result = null;
			Node[] nodes = sources;
			bool isNext = true;

			while (isNext)
			{
				if (nodes.Length == 2)
				{
					result = new Node();
					result.LChild = nodes[0];
					result.RChild = nodes[1];
					isNext = false;
				}

				Array.Sort(nodes);
				Node node1 = nodes[nodes.Length - 1];
				Node node2 = nodes[nodes.Length - 2];
				Node temp = new Node();
				temp.Weight = node1.Weight + node2.Weight;
				temp.LChild = node1;
				temp.RChild = node2;

				Node[] tempNodes = new Node[nodes.Length - 1];
				Array.Copy(nodes, 0, tempNodes, 0, nodes.Length - 1);
				tempNodes[tempNodes.Length - 1] = temp;

				nodes = tempNodes;
			}

			return result;
		}

		/// <summary>
		/// 字符串转换为哈弗曼代码.当然可以改成二进制数据
		/// </summary>
		/// <param name="key">编码字典</param>
		/// <param name="str">传入字符串</param>
		/// <returns>编码后的字符串</returns>
		public string StringToHuffmanCode(out Dictionary<char, string> key, string str)
		{
			string result = "";

			var tmps = GetWeightArray(str);

			var tree = CreateHuffmanTree(tmps);
			var dict = CreateHuffmanDict(tree);

			result = ToHuffmanCode(str, dict);

			key = dict;
			return result;
		}


		public Dictionary<char, string> CreateNewDictionary(string dicString)
		{
			Dictionary<char, string> result = new Dictionary<char, string>();
			string pattern = @".{1}\s\d+(\\r||\\n){1}";

			MatchCollection matches = Regex.Matches(dicString, pattern);
			foreach (Match match in matches)
			{
				OneLineNode oneLine = new OneLineNode(match.Value);
				result.Add(oneLine.Key, oneLine.Value);
			}
			
			return result;
		}

		/// <summary>
		/// 创建哈弗曼代码字典
		/// </summary>
		/// <param name="hTree">哈弗曼树</param>
		/// <returns></returns>
		private Dictionary<char, string> CreateHuffmanDict(Node hTree)
		{
			return CreateHuffmanDict("", hTree);
		}

		private Dictionary<char, string> CreateHuffmanDict(string code, Node hTree)
		{
			Dictionary<char, string> result = new Dictionary<char, string>();

			if (hTree.LChild == null && hTree.RChild == null)
			{
				result.Add(hTree.Key, code);
			}
			else
			{
				var dictL = CreateHuffmanDict(code + "0", hTree.LChild);
				var dictR = CreateHuffmanDict(code + "1", hTree.RChild);

				foreach (var item in dictL)
				{
					result.Add(item.Key, item.Value);
				}

				foreach (var item in dictR)
				{
					result.Add(item.Key, item.Value);
				}
			}

			return result;
		}

		/// <summary>
		/// 源字符串转换到哈弗曼代码
		/// </summary>
		/// <param name="source">源字符串</param>
		/// <param name="hfdict">哈弗曼代码字典</param>
		/// <returns>编译后代码</returns>
		private string ToHuffmanCode(string source, Dictionary<char, string> hfdict)
		{
			string result = "";

			for (int i = 0; i < source.Length; i++)
			{
				result += hfdict.First(m => m.Key == source[i]).Value;
			}

			return result;
		}

		/// <summary>
		/// 解析回字符串
		/// </summary>
		/// <param name="code">哈弗曼代码</param>
		/// <param name="dictionary">哈弗曼代码字典</param>
		/// <returns>解析后字符串</returns>
		public string HuffmanCodeToString(Dictionary<char, string> dictionary, string code)
		{
			int errortimes = 0;
			int statusPrv = -1;
			string result = "";

			for (int i = 0; i < code.Length;)
			{
				foreach (var item in dictionary)
				{
					if (code[i] == item.Value[0] && item.Value.Length + i <= code.Length)
					{
						char[] oneChar = new char[item.Value.Length];
						Array.Copy(code.ToCharArray(), i, oneChar, 0, oneChar.Length);
						Debug.WriteLine("Now is " + item.Key + " " + item.Value);
						if (new String(oneChar) == item.Value)
						{
							result += item.Key;
							i += item.Value.Length;
							
							break;
						}
					}
					if (statusPrv == i)
					{
						errortimes++;
					}
					else
					{
						errortimes = 0;
					}
					statusPrv = i;

					if (errortimes > dictionary.Count)
					{
						Debug.WriteLine("Error!");
						return result;
					}
				}
			}

			return result;
		}

	}
}
