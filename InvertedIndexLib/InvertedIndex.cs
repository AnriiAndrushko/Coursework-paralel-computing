using System.Collections.Concurrent;
using System.Runtime.Serialization.Formatters.Binary;


using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

[assembly: InternalsVisibleTo("Tests")]

namespace InvertedIndexLib
{
    internal class InvertedIndex
    {
        private ConcurrentDictionary<string, HashSet<string>> _data;
        public InvertedIndex(ConcurrentDictionary<string, HashSet<string>> data)
        {
            _data = data;
        }
        public InvertedIndex()
        {
            _data = new ConcurrentDictionary<string, HashSet<string>>();
        }
        public static InvertedIndex LoadIndex(string path)
        {
            using (var stream = new FileStream(path, FileMode.Open))
            {
                var f = new BinaryFormatter();
                var deserializedDic = (Dictionary<string, HashSet<string>>)f.Deserialize(stream);
                return new InvertedIndex(new ConcurrentDictionary<string, HashSet<string>>(deserializedDic));
            }
        }
        public void SaveIndex(string path)
        {
            using (var stream = new FileStream(path, FileMode.Create))
            {
                var f = new BinaryFormatter();
                f.Serialize(stream, new Dictionary<string, HashSet<string>>(_data, _data.Comparer));
            }
        }

        public void AddText(string text, string textOrigin)
        {
            string pattern = @"[\p{P}-[']]+";
            text = Regex.Replace(text.ToLower(), pattern, " ");
            text.Trim();
            foreach (string word in text.Split())
            {
                if (!_data.ContainsKey(word))
                {
                    _data.TryAdd(word, new HashSet<string>());
                }
                lock (_data[word])
                {
                    _data[word].Add(textOrigin);
                }
            }
        }

        public void AddDoc(string docPath)
        {
            if (!File.Exists(docPath))
            {
                throw new FileNotFoundException();
            }
            using (StreamReader sr = new StreamReader(docPath))
            {
                while (sr.Peek() >= 0)
                {
                    AddText(sr.ReadLine(), docPath);
                }
            }
        }
        public IEnumerable<string> GetByWord(string word)
        {
            HashSet<string> result;
            if(_data.TryGetValue(word, out result))
            {
                return result;
            }
            return new string[0];
        }
        public IEnumerable<string> GetByQuery(string sentence)
        {
            List<IEnumerable<string>> files = new List<IEnumerable<string>>();
            foreach (string word in sentence.Split())
            {
                files.Add(GetByWord(word));
            }
            if (files.Count==0)
            {
                return new string[0];
            }
            IEnumerable<string> tmp = files[0];
            foreach (var res in files)
            {
                tmp = tmp.Intersect(res);
            }
            return tmp;
        }
    }
}