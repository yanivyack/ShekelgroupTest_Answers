using ENV.Data;
using Firefly.Box.Data.DataProvider;

namespace ENV.Utilities
{
    class SQLHistory : Entity
    {
        public TextColumn SQL = new TextColumn();
        public TextColumn LastRun = new TextColumn();

        public SQLHistory(IEntityDataProvider db) : base("History", db)
        {
        }
    }
}
