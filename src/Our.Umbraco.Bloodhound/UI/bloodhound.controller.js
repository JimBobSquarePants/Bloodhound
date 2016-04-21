angular.module("umbraco").controller("Our.Umbraco.Bloodhound", function ($scope) {
    
    // Initialize the model variables.
    $scope.selected = {};
    $scope.selectedIndex = -1;
    $scope.rewriteUrl = "";
    $scope.isRegex = false;
    $scope.statusCode = 301;


    if ($scope.model.value.length === 0) {

        $scope.model.value = [];
    }

    // Display
    $scope.getTemplate = function (idx) {

        if (idx === $scope.selectedIndex) {
            return "edit";

        } else {
            return "display";
        }
    };

    // Edit
    $scope.edit = function (idx) {

        $scope.selectedIndex = idx;
        $scope.selected = window.angular.copy($scope.model.value[idx]);
    };

    // Reset
    $scope.reset = function () {

        $scope.selected = {};
        $scope.selectedIndex = -1;
    }

    // Update
    $scope.update = function (idx) {

        $scope.model.value[idx] = window.angular.copy($scope.selected);
        $scope.reset();
    };

    // Add
    $scope.add = function () {

        if ($scope.rewriteUrl.length === 0) {
            return;
        }

        // Format the date to match .NET
        var now = new Date(),
            month = ("0" + (now.getUTCMonth() + 1)).slice(-2),
            day = ("0" + (now.getUTCDay())).slice(-2),
            hours = ("0" + (now.getUTCHours())).slice(-2),
            min = ("0" + (now.getUTCMinutes())).slice(-2),
            sec = ("0" + (now.getUTCSeconds())).slice(-2);

        var redirect = {
            "rewriteUrl": $scope.rewriteUrl,
            "isRegex": $scope.isRegex,
            "statusCode": $scope.statusCode,
            "createdDateUtc": now.getUTCFullYear() + "-" + month + "-" + day + " " + hours + ":" + min + ":" + sec
        }

        $scope.model.value.push(redirect);

        // Reset variables. Separate from index reset.
        $scope.rewriteUrl = "";
        $scope.isRegex = false;
        $scope.statusCode = 301;
    };

    // Remove
    $scope.delete = function (idx) {
        $scope.model.value.splice(idx, 1);
    };
});