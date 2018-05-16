var pageloaded = false;
var angularApp = angular.module('angularApp', ['ngRoute', 'ui.bootstrap'])
.controller('homeCntrl', function ($scope, $http, $q, $location){
	$scope.dataReady = false;
	$scope.inputs = {
		link: ""
	};
	if (!pageloaded) {
		pageloaded = true;
		var p1 = $http.get('data/json/linkindex.json');
		var p2 = $http.get('data/json/links.json');
		var p3 = $http.get('data/json/pages.json');

		$q.all([p1, p2, p3]).then(function(data)
		{
			$scope.linkIndex = data[0].data;
			$scope.links = data[1].data;
			$scope.pages = data[2].data;
			$scope.results = [];
			$scope.dataReady = true;
			alert("System is ready.")
		});		
	}
	
	$scope.doCheck = function() {
		var il = $scope.inputs.link.toLowerCase();
		
		var rts = [];
		var rst2 = [];

		angular.forEach($scope.links, function(li, idx) {
		  if (li.endsWith(il)) {
			  var lidx = $scope.linkIndex[idx];
			  angular.forEach(lidx, function(linkp, idx2) {
				  if (rts.indexOf(linkp) <0) {
					  rts.push(linkp);
					  rst2.push($scope.pages[linkp]);
				  }
			  });
		  }
		});
	
		$scope.results = rst2;
		$scope.showResults = true;
	}
})
.config(['$routeProvider','$locationProvider',function($routeProvider,$locationProvider)
{
	$locationProvider.html5Mode(false);
	//$locationProvider.html5Mode(true);
	$routeProvider
	.when('/',{
		templateUrl:'view/home.html',
		controller:'homeCntrl'
	})

}]);

