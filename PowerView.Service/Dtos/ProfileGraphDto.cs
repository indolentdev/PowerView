namespace PowerView.Service.Dtos
{
  public class ProfileGraphDto
  {
    public string Period { get; set; }
    public string Page { get; set; }
    public string Title { get; set; }
    public string Interval { get; set; }
    public long Rank { get; set; }
    public ProfileGraphSerieDto[] Series { get; set; }
  }
}
