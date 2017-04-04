import * as template from "text!QlikConnectorPSExecute.webroot/connectdialog.ng.html";
import "css!QlikConnectorPSExecute.webroot/connectdialog.css";

class ConnectDialog {
    name: string="";
    username: string;
    password: string;
    isEdit: boolean;
    provider: string = "QlikConnectorPSExecute.exe";
    connectionInfo: string;

    input: any;
    scope: any;

    info = "Connector for Windows PowerShell.";

    get isOkEnabled(): boolean {
        try {
            return this.name.length > 0;
        } catch(ex) {
            return false;
        }
    }

    get connectionString(): string {
        return "CUSTOM CONNECT TO " + "\"provider=" + this.provider + ";" + "host=localhost;" + "\"";
    }

    get titleText(): string {
        return this.isEdit ? "Change PowerShell connection" : "Add PowerShell connection";
    }

    get saveButtonText(): string {
        return this.isEdit ? "Save changes" : "Create";
    }

    constructor(input: any, scope: any) {
        this.isEdit = input.editMode;
        this.scope = scope;
        this.input = input;
        if (this.isEdit) {
            input.serverside.getConnection(input.instanceId).then((result)=> {
                this.name = result.qConnection.qName;
            });
        }
    }

    public onOKClicked(): void {
        if (this.name === "") {
            this.connectionInfo = "Please enter a name for the connection.";
        } else {
            if (this.isEdit) {
                var overrideCredentials = this.username !== "" && this.password !== "";
                this.input.serverside.modifyConnection(this.input.instanceId,
                    this.name,
                    this.connectionString,
                    this.provider,
                    overrideCredentials,
                    this.username,
                    this.password).then((result) => {
                        if (result) {
                            this.destroyComponent();
                        }
                    });
            } else {
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
    }

    public onEscape(): void {
        this.destroyComponent();
    }

    public onCancelClicked(): void {
        this.destroyComponent();
    }

    private destroyComponent() {
        this.scope.destroyComponent();
    }
}

export = {
    template: template,
    controller: ["$scope", "input", function ($scope, input) {
        $scope.vm = new ConnectDialog(input, $scope);
    }]
};