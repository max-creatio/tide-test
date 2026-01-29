namespace GlbHistoryInput.src.cs.EntityEventListeners
{
    using Terrasoft.Core;
    using Terrasoft.Common;
    using Terrasoft.Core.DB;
    using Terrasoft.Core.Entities;
    using Terrasoft.Core.Entities.Events;
    using CoreSysSettings = Terrasoft.Core.Configuration.SysSettings;
    using System.Linq;

    #region Class: public

    [EntityEventListener(SchemaName = "GlbHistoryValue")]
    public class HistoryValueEntityListener : BaseEntityEventListener
    {
        #region Fields: private

        private UserConnection _userConnection;
        private Entity _entity;
        private readonly string _targetColumnName = "GlbHistoryValue";

        #endregion

        #region Methods: private

        private void RemoveDublicateValues() {
            var delete = new Delete(_userConnection)
                .From(_targetColumnName)
                .Where("CreatedById").IsEqual(Column.Parameter(_entity.GetColumnValue("CreatedById")))
                .And("GlbEntitySchemaName").IsEqual(Column.Parameter(_entity.GetColumnValue("GlbEntitySchemaName")))
                .And("GlbColumnName").IsEqual(Column.Parameter(_entity.GetColumnValue("GlbColumnName")))
                .And("GlbValue").IsEqual(Column.Parameter(_entity.GetColumnValue("GlbValue")));
            delete.Execute();
        }

        private void RemoveOldValues() {
            var maxCountValuesInHistory = CoreSysSettings.GetValue(_userConnection, "GlbMaxHistoryValue", 0);
            var historyValues = GetHistoryValues();

            if (maxCountValuesInHistory > historyValues.Count) {
                return;
            }

            var needDelete = historyValues.Count - (maxCountValuesInHistory - 1);

            for (var i = 0; i < needDelete; i++) {
                historyValues[0].Delete();
            }
        }

        private EntityCollection GetHistoryValues() {
            var esq = CreateEsq(_targetColumnName);
            esq.PrimaryQueryColumn.IsAlwaysSelect = true;

            var orderColumn = esq.AddColumn("CreatedOn");   
            orderColumn.OrderDirection = OrderDirection.Ascending;

            esq.Filters.Add(esq.CreateFilterWithParameters(FilterComparisonType.Equal, "GlbEntitySchemaName", _entity.GetColumnValue("GlbEntitySchemaName")));
            esq.Filters.Add(esq.CreateFilterWithParameters(FilterComparisonType.Equal, "CreatedBy", _entity.GetColumnValue("CreatedById")));
            esq.Filters.Add(esq.CreateFilterWithParameters(FilterComparisonType.Equal, "GlbColumnName", _entity.GetColumnValue("GlbColumnName")));

            return esq.GetEntityCollection(_userConnection);
        }

        private EntitySchemaQuery CreateEsq(string schemaName) {
            return new EntitySchemaQuery(_userConnection.EntitySchemaManager, schemaName) {
                UseAdminRights = false,
            };
        }

        #endregion

        #region Methods: public

        public override void OnInserting(object sender, EntityBeforeEventArgs e) {
            base.OnInserting(sender, e);

            _entity = sender as Entity;
            _userConnection = _entity.UserConnection;

            RemoveDublicateValues();
            RemoveOldValues();
        }

        #endregion
    }

    #endregion
}
