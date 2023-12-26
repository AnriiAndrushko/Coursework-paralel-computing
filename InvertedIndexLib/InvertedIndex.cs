using System.Collections.Concurrent;
using System.Runtime.Serialization.Formatters.Binary;

namespace InvertedIndexLib
{
    public class InvertedIndex
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
                return new InvertedIndex((ConcurrentDictionary<string, HashSet<string>>)f.Deserialize(stream));
            }
        }
        public void SaveIndex(string path)
        {
            using (var stream = new FileStream(path, FileMode.Create))
            {
                var f = new BinaryFormatter();
                f.Serialize(stream, _data);
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
                    foreach (string word in sr.ReadLine().Split())
                    {
                        if (!_data.ContainsKey(word))
                        {
                            _data.TryAdd(word, new HashSet<string>());
                        }
                        _data[word].Add(docPath);
                    }
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