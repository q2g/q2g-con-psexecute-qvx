/* License
Copyright (c) 2017 Konrad Mattheis und Martin Berthold
Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 */

import * as template from "text!QlikConnectorPSExecute.webroot/connectdialog.ng.html";
import "css!QlikConnectorPSExecute.webroot/connectdialog.css";

class ConnectDialog {
    name: string="";
    password: string;
    olduser: string;
    passlength: number;
    host: string="";
    isEdit: boolean;
    provider: string = "QlikConnectorPSExecute.exe";
    connectionInfo: string;
    version: string = "";

    input: any;
    scope: any;

    info = "Connector for Windows PowerShell.";

    private _username: string;
    get username(): string {
        return this._username;
    }
    set username(v: string) {
        if (v !== this._username) {
            this._username = v;
            this.password = '';
        }
    }

    get isOkEnabled(): boolean {
        try {
            return this.name.length > 0;
        } catch(ex) {
            return false;
        }
    }

    private connectionString(host: string): string {
        if (!host)
            host = "localhost";
        return "CUSTOM CONNECT TO " + "\"provider=" + this.provider + ";" + "host=" + host + ";" + "\"";
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
                let connStr: string = result.qConnection.qConnectionString;
                let list = connStr.match("host=([^;]*);");
                if (list.length == 2)
                    this.host = list[1];
                else
                    this.host = "";
            });
        }

        input.serverside.sendJsonRequest("getUsername").then((info) => {
            try {
                this.username = (info.qMessage as string);
                this.olduser = this.username;
                this.password = "**********";
            } catch (e) {
            }
        });

        input.serverside.sendJsonRequest("getVersion").then((info) => {
            try {
                this.version = (info.qMessage as string).replace(".Sha.", " Sha.");
            } catch (e) {
            }
        });
    }

    public onOKClicked(): void {
        if (this.name === "") {
            this.connectionInfo = "Please enter a name for the connection.";
        } else {
            if (this.isEdit) {
                var overrideCredentials = this.username !== this.olduser || this.password !== "**********";
                this.input.serverside.modifyConnection(
                    this.input.instanceId,
                    this.name,
                    this.connectionString(this.host),
                    this.provider,
                    overrideCredentials,
                    this.username,
                    this.password
                ).then((result) => {
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

                    this.input.serverside.createNewConnection(this.name, this.connectionString(this.host), this.username, this.password);
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