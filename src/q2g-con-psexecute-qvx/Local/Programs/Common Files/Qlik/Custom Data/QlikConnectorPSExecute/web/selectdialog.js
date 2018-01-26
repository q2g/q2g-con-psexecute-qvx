define(["require", "exports", "qvangular"], function (require, exports, qvangular) {
    "use strict";
    return ["serverside", "standardSelectDialogService",
        function (serverside, standardSelectDialogService) {
            var dialogContentProvider = {
                getConnectionInfo: function () {
                    return qvangular.promise({
                        dbusage: false,
                        ownerusage: false,
                        dbseparator: ".",
                        ownerseparator: ".",
                        specialchars: '! "$&\"()*+,-/:;<>`{}~[]',
                        quotesuffix: '"',
                        quoteprefix: '"',
                        dbfirst: true,
                        keywords: []
                    });
                },
                getDatabases: function () {
                    return qvangular.promise({ qName: "" });
                },
                getOwners: function (qDatabaseName) {
                    return qvangular.promise([{ qName: "" }]);
                },
                getTables: function (qDatabaseName, qOwnerName) {
                    return qvangular.promise([]);
                },
                getFields: function (qDatabaseName, qOwnerName, qTableName) {
                    return qvangular.promise([]);
                },
                getPreview: function (qDatabaseName, qOwnerName, qTableName) {
                    return qvangular.promise([]);
                }
            };
            standardSelectDialogService.showStandardDialog(dialogContentProvider, {
                precedingLoadVisible: true,
                fieldsAreSelectable: true,
                allowFieldRename: true
            });
        }];
});
//# sourceMappingURL=selectdialog.js.map