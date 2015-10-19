using Powerup.SqlObjects;

namespace Powerup.Templates
{
    public class TableDropCreateTemplate : TemplateBase
    {
        private string _template = string.Empty;

        public TableDropCreateTemplate(SqlObject sqlObject) : base(sqlObject)
        {
        }

        public override string TemplatedProcedure()
        {
            return _template;
        }

        public override void AddText(string text)
        {
            _template = text;
        }
    }
}