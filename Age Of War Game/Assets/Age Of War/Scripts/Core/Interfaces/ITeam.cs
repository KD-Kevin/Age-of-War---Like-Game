namespace AgeOfWar.Core
{
    public interface ITeam
    {
        public int Team { get; set; }

        public abstract void SetTeam(int TeamID);
    }
}
