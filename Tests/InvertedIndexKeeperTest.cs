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
        public void AddText_AddsTextIntoIndex()
        {

        }
        public void AddDoc_AddsDocIntoIndex()
        {

        }
        public void GetByQuery_GetsDocumentByQuery()
        {

        }
    }
}
