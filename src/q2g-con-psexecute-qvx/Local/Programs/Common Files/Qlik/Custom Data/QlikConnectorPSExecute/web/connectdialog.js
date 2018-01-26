define(["require", "exports", "text!QlikConnectorPSExecute.webroot/connectdialog.ng.html", "css!QlikConnectorPSExecute.webroot/connectdialog.css"], function (require, exports, template) {
    "use strict";
    var ConnectDialog = (function () {
        function ConnectDialog(input, scope) {
            var _this = this;
            this.name = "";
            this.provider = "QlikConnectorPSExecute.exe";
            this.info = "Connector for Windows PowerShell.";
            this.isEdit = input.editMode;
            this.scope = scope;
            this.input = input;
            if (this.isEdit) {
                input.serverside.getConnection(input.instanceId).then(function (result) {
                    _this.name = result.qConnection.qName;
                });
            }
        }
        Object.defineProperty(ConnectDialog.prototype, "isOkEnabled", {
            get: function () {
                try {
                    return this.name.length > 0;
                }
                catch (ex) {
                    return false;
                }
            },
            enumerable: true,
            configurable: true
        });
        Object.defineProperty(ConnectDialog.prototype, "connectionString", {
            get: function () {
                return "CUSTOM CONNECT TO " + "\"provider=" + this.provider + ";" + "host=localhost;" + "\"";
            },
            enumerable: true,
            configurable: true
        });
        Object.defineProperty(ConnectDialog.prototype, "titleText", {
            get: function () {
                return this.isEdit ? "Change PowerShell connection" : "Add PowerShell connection";
            },
            enumerable: true,
            configurable: true
        });
        Object.defineProperty(ConnectDialog.prototype, "saveButtonText", {
            get: function () {
                return this.isEdit ? "Save changes" : "Create";
            },
            enumerable: true,
            configurable: true
        });
        ConnectDialog.prototype.onOKClicked = function () {
            var _this = this;
            if (this.name === "") {
                this.connectionInfo = "Please enter a name for the connection.";
            }
            else {
                if (this.isEdit) {
                    var overrideCredentials = this.username !== "" && this.password !== "";
                    this.input.serverside.modifyConnection(this.input.instanceId, this.name, this.connectionString, this.provider, overrideCredentials, this.username, this.password).then(function (result) {
                        if (result) {
                            _this.destroyComponent();
                        }
                    });
                }
                else {
                    {
                        if (typeof this.username === "undefined")
                            this.username = "";
                        if (typeof this.password === "undefined")
                            this.password = "";
                        this.input.serverside.createNewConnection(this.name, this.connectionString, this.username, this.password);
                        this.destroyComponent();
                    }
                }
            }
        };
        ConnectDialog.prototype.onEscape = function () {
            this.destroyComponent();
        };
        ConnectDialog.prototype.onCancelClicked = function () {
            this.destroyComponent();
        };
        ConnectDialog.prototype.destroyComponent = function () {
            this.scope.destroyComponent();
        };
        return ConnectDialog;
    }());
    return {
        template: template,
        controller: ["$scope", "input", function ($scope, input) {
                $scope.vm = new ConnectDialog(input, $scope);
            }]
    };
});
//# sourceMappingURL=connectdialog.js.map