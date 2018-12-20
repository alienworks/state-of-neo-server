namespace StateOfNeo.ViewModels
{
    public class BaseBlockViewModel : StampViewModel
    {
        public string Hash { get; set; }

        public int Height { get; set; }

        public int Size { get; set; }
    }
}
