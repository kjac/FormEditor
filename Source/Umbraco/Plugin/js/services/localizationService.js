angular.module("umbraco.services").factory("formEditorLocalizationService", ["$http", "$q", "userService",
  function ($http, $q, userService) {
    var service = {
      resourceFileLoaded: false,
      dictionary: {},
      localize: function (key, defaultValue) {
        var deferred = $q.defer();

        if (service.resourceFileLoaded) {
          var value = service._lookup(key, defaultValue);
          deferred.resolve(value);
        }
        else {
          service.initLocalizedResources().then(function (dictionary) {
            var value = service._lookup(key, defaultValue);
            deferred.resolve(value);
          });
        }

        return deferred.promise;
      },
      _lookup: function (key, defaultValue) {
        var value = service.dictionary[key];
        if (value == null) {
          value = defaultValue;
        }
        return value;
      },
      initLocalizedResources: function () {
        var deferred = $q.defer();
        userService.getCurrentUser().then(function (user) {
          $http.get("/App_Plugins/FormEditor/js/langs/" + user.locale + ".js", { cache: true })
              .then(function (response) {
                service.resourceFileLoaded = true;
                service.dictionary = response.data;

                return deferred.resolve(service.dictionary);
              }, function (err) {
                return deferred.reject("Lang file missing");
              });
        });
        return deferred.promise;
      }
    }

    return service;
  }
]);
