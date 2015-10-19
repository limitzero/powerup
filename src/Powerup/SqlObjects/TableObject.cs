using Powerup.SqlQueries;

namespace Powerup.SqlObjects
{
    public class TableObject : SqlObject
    {
        public TableObject(IQueryBase parentQuery, string dataBase) : 
            base(parentQuery, dataBase)
        {
            this.SqlType = SqlType.Table;
        }
    }
}