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
            string content = "This, is a sample: document.";

            index.AddText(content, documentId);

            CollectionAssert.Contains(index.GetByWord("sample").ToList(), documentId);
            CollectionAssert.Contains(index.GetByWord("document").ToList(), documentId);
            CollectionAssert.Contains(index.GetByWord("this").ToList(), documentId);
        }
    }
}