namespace JaxSun.Web.Models.ViewModels
{
    public class HomeViewModel
    {
        public string HeroTitle { get; set; } = "Turn Your Great Ideas Into Thriving Software Businesses";
        public string HeroSubtitle { get; set; } = "Partner with experienced Midwest developers who share the risk and reward";
        public string PrimaryCta { get; set; } = "Submit Your Idea for Evaluation";
        public List<ValueProposition> ValuePropositions { get; set; } = new();
        public List<TrustIndicator> TrustIndicators { get; set; } = new();
        public List<string> HeroImages { get; set; } = new();
    }

    public class ValueProposition
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Icon { get; set; } = string.Empty;
    }

    public class TrustIndicator
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }
}