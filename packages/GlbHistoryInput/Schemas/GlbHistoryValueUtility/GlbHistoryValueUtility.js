define("GlbHistoryValueUtility", [], function() {
	return {
		getHistoryValues: async function(entitySchemaName, columnName) {
			return new Promise((resolve, reject) => {
				let esq = Ext.create("Terrasoft.EntitySchemaQuery", {
					rootSchemaName: "GlbHistoryValue",
					rowCount: 10
				});

				this.setOrderColumn(esq);
				esq.addColumn("GlbValue");

				esq.filters.addItem(Terrasoft.createColumnFilterWithParameter(
					Terrasoft.ComparisonType.EQUAL, "CreatedBy", Terrasoft.SysValue.CURRENT_USER_CONTACT.value));
				esq.filters.addItem(Terrasoft.createColumnFilterWithParameter(
					Terrasoft.ComparisonType.EQUAL, "GlbEntitySchemaName", entitySchemaName));
				esq.filters.addItem(Terrasoft.createColumnFilterWithParameter(
					Terrasoft.ComparisonType.EQUAL, "GlbColumnName", columnName));

				esq.getEntityCollection(function(response) {
					if (response.success) {
						resolve(response.collection.getItems()
							.map(x => x.get("GlbValue")));
					} else {
						reject(response);
					}
				}, this);
			});
		},

		setOrderColumn: function(esq) {
			var createOnCol = esq.addColumn("CreatedOn");
			createOnCol.orderPosition = 0;
			createOnCol.orderDirection = Terrasoft.OrderDirection.DESC;
		},

		saveHistoryValue: async function(entitySchemaName, columnName, value) {
			return new Promise((resolve, reject) => {
				let query = Ext.create("Terrasoft.InsertQuery", {
					rootSchemaName: "GlbHistoryValue"
				});
	
				query.setParameterValue("GlbEntitySchemaName", entitySchemaName, Terrasoft.DataValueType.TEXT);
				query.setParameterValue("GlbColumnName", columnName, Terrasoft.DataValueType.TEXT);
				query.setParameterValue("GlbValue", value, Terrasoft.DataValueType.TEXT);
	
				query.execute(function(result) {
					if (result.success) {
						resolve(result);
					} else {
						reject(result);
					}
				}, this);
			})
		}
	};
});