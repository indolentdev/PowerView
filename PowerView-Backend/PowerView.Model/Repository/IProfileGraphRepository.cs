using System.Collections.Generic;

namespace PowerView.Model.Repository
{
    public interface IProfileGraphRepository
    {
        ICollection<string> GetProfileGraphPages(string period);

        ICollection<ProfileGraph> GetProfileGraphs();

        ICollection<ProfileGraph> GetProfileGraphs(string period, string page);

        void AddProfileGraph(ProfileGraph profileGraph);

        bool UpdateProfileGraph(string period, string page, string title, ProfileGraph profileGraph);

        void DeleteProfileGraph(string period, string page, string title);

        void SwapProfileGraphRank(string period, string page, string title1, string title2);
    }
}
