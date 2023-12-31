using System;

namespace Tests
{
    [TestClass]
    public class InvertedIndexKeeperTest
    {
        [TestMethod]
        public void Save_SavesIndexIntoFile()
        {
            string pathToSave = "testIndex";
            if (File.Exists(pathToSave))
                File.Delete(pathToSave);
            InvertedIndexKeeper index1 = new(2);
            index1.AddText("bla bla", "test");
            index1.Save(pathToSave);
            InvertedIndexKeeper index2 = new(2);
            index2.Load(pathToSave);
            Thread.Sleep(100);
            var res1 = index2.GetByWord("bla");
            var res2 = index1.GetByWord("bla");
            Thread.Sleep(100);
            CollectionAssert.AreEqual(res1.ToList(), res2.ToList());
        }

        [TestMethod]
        public void AddText_AddsTextIntoIndex()
        {
            InvertedIndexKeeper index = new (2);
            string document1Id = "doc1";
            string content1 = "This, is a sample: document. ?and& a text;";
            string document2Id = "doc2";
            string content2 = "this is a document";

            index.AddText(content1, document1Id);
            index.AddText(content2, document2Id);

            CollectionAssert.Contains(index.GetByWord("document").ToList(), document1Id);
            CollectionAssert.Contains(index.GetByWord("document").ToList(), document2Id);
            Assert.AreEqual(1, index.GetByWord("sample").Count());
            Assert.AreEqual(2, index.GetByWord("document").Count());
        }

        [TestMethod]
        public void AddDoc_AddsDocIntoIndex()
        {
            string pathToDoc1 = "testDoc1";
            string pathToDoc2 = "testDoc2";

            InvertedIndex index = new InvertedIndex();
            string content1 = "This, is a sample: document. ?and& a text;";
            string content2 = "this is a document";

            File.AppendAllText(pathToDoc1, content1);
            File.AppendAllText(pathToDoc2, content2);

            if (File.Exists(pathToDoc1))
                File.Delete(pathToDoc1);
            if (File.Exists(pathToDoc2))
                File.Delete(pathToDoc2);
            index.AddText(content1, pathToDoc1);
            index.AddText(content2, pathToDoc2);

            CollectionAssert.Contains(index.GetByWord("document").ToList(), pathToDoc1);
            CollectionAssert.Contains(index.GetByWord("document").ToList(), pathToDoc2);
            Assert.AreEqual(1, index.GetByWord("sample").Count());
            Assert.AreEqual(2, index.GetByWord("document").Count());
        }

        [TestMethod]
        public void GetByQuery_GetsDocumentByQuery()
        {
            string pathToDoc1 = "testDoc1";
            string pathToDoc2 = "testDoc2";

            InvertedIndex index = new InvertedIndex();
            string content1 = "This, is a sample: document. ?and& a text;";
            string content2 = "this is a document";

            File.AppendAllText(pathToDoc1, content1);
            File.AppendAllText(pathToDoc2, content2);

            if (File.Exists(pathToDoc1))
                File.Delete(pathToDoc1);
            if (File.Exists(pathToDoc2))
                File.Delete(pathToDoc2);
            index.AddText(content1, pathToDoc1);
            index.AddText(content2, pathToDoc2);

            CollectionAssert.Contains(index.GetByQuery("is a sample").ToList(), pathToDoc1);
            Assert.AreEqual(1, index.GetByQuery("is a sample").Count());
        }
    }
}
