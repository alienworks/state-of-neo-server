namespace StateOfNeo.ViewModels
{
    public class NodeDetailsViewModel : NodeViewModel
    {
        public long? FirstRuntime { get; set; }
        public long? LatestRuntime { get; set; }
        public long SecondsOnline { get; set; }
        public long? LastAudit { get; set; }
    }
}
