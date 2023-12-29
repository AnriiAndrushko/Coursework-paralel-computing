namespace Tests
{
    [TestClass]
    public class IndexTest
    {
        [TestMethod]
        public void AddDocument_AddsDocumentToIndex()
        {
            InvertedIndex index = new InvertedIndex();
            string documentId = "doc1";
            string content = "This, is a sample: document. ?and& a text;";

            index.AddText(content, documentId);

            CollectionAssert.Contains(index.GetByWord("sample").ToList(), documentId);
            CollectionAssert.Contains(index.GetByWord("document").ToList(), documentId);
            CollectionAssert.Contains(index.GetByWord("this").ToList(), documentId);
            CollectionAssert.Contains(index.GetByWord("and").ToList(), documentId);
            CollectionAssert.Contains(index.GetByWord("text").ToList(), documentId);
            Assert.AreEqual(1, index.GetByWord("a").Count());
            Assert.AreEqual(1, index.GetByWord("document").Count());
        }

        [TestMethod]
        public void AddDocument_AddsDocumentsToIndex()
        {
            InvertedIndex index = new InvertedIndex();
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
        public void GetByQuery_GetsDocumentByQuery()
        {
            InvertedIndex index = new InvertedIndex();
            string document1Id = "doc1";
            string content1 = "This, is a sample: document. ?and& a text;";
            string document2Id = "doc2";
            string content2 = "this is a document";

            index.AddText(content1, document1Id);
            index.AddText(content2, document2Id);

            CollectionAssert.Contains(index.GetByQuery("is a sample").ToList(), document1Id);
            Assert.AreEqual(1, index.GetByQuery("is a sample").Count());
        }
    }
}