namespace GameCenter
{
	public class Achievement
	{
		public string AchievementID { get; private set; }

		public double PercentComplete { get; private set; }

		public bool IsHidden { get; private set; }

		public string Title { get; private set; }

		public string Description { get; private set; }

		public Achievement(string _AchievementID, double _PercentComplete, bool _IsHidden, string _Title, string _Description)
		{
			AchievementID = _AchievementID;
			PercentComplete = _PercentComplete;
			IsHidden = _IsHidden;
			Title = _Title;
			Description = _Description;
		}
	}
}
