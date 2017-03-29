define( ['qvangular',
		'text!QlikConnectorPSExecute.webroot/connectdialog.ng.html',
		'css!QlikConnectorPSExecute.webroot/connectdialog.css'
], function ( qvangular, template) {
	return {
		template: template,
		controller: ['$scope', 'input', function ( $scope, input ) {

		    /*Initialize*/
		    function init() {
				$scope.isEdit = input.editMode;
				$scope.id = input.instanceId;
				$scope.titleText = $scope.isEdit ? "Change PowerShell connection" : "Add PowerShell connection";
				$scope.saveButtonText = $scope.isEdit ? "Save changes" : "Create";

				$scope.name = "";
				$scope.username = "";
				$scope.password = "";
				$scope.provider = "QlikConnectorPSExecute.exe";
				$scope.connectionInfo = "";
				$scope.connectionSuccessful = false;
				$scope.connectionString = createCustomConnectionString($scope.provider, "host=localhost;");

				input.serverside.sendJsonRequest( "getInfo" ).then( function ( info ) {
					$scope.info = info.qMessage;
				} );

				if ( $scope.isEdit ) {
				    input.serverside.getConnection($scope.id).then(function (result) {
                        $scope.name = result.qConnection.qName;
					} );
				}
			}

			/* Event handlers */
		    $scope.onOKClicked = function () {

		        if ($scope.name == "") {
		            $scope.connectionInfo = "Please enter a name for the connection.";
		        }
		        else if($scope.username == "")
		        {
		            $scope.connectionInfo = "Please enter the user name.";
		        }
		        else if ($scope.password == "") {
		            $scope.connectionInfo = "Please enter the password.";
		        }
		        else {
		            if ($scope.isEdit) {
		                var overrideCredentials = $scope.username !== "" && $scope.password !== "";
		                input.serverside.modifyConnection($scope.id,
                            $scope.name,
                            $scope.connectionString,
                            $scope.provider,
                            overrideCredentials,
                            $scope.username,
                            $scope.password).then(function (result) {
                                if (result) {
                                    $scope.destroyComponent();
                                }
                            });
		            } else {
		                input.serverside.createNewConnection($scope.name, $scope.connectionString, $scope.username, $scope.password);
		                $scope.destroyComponent();
		            }
		        }
		    
			};

			$scope.onTestConnectionClicked = function () {
			    input.serverside.sendJsonRequest("testConnection", $scope.username, $scope.password).then(function (info) {
					$scope.connectionInfo = info.qMessage;
					$scope.connectionSuccessful = info.qMessage.indexOf( "SUCCESS" ) !== -1;
				} );
			};

			$scope.isOkEnabled = function () {
				return $scope.name.length > 0 && $scope.connectionSuccessful;
			};

			$scope.onEscape = $scope.onCancelClicked = function () {
				$scope.destroyComponent();
			};

			
			/* Create ConnectionString */
			function createCustomConnectionString ( filename, connectionstring ) {
				return "CUSTOM CONNECT TO " + "\"provider=" + filename + ";" + connectionstring + "\"";
			}

			init();
		}]
	};
} );