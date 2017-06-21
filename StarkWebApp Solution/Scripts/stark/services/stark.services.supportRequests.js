if (typeof stark.services === "undefined") {
	stark.services = {
		supportRequests: {}
	};
}
else {
	stark.services.supportRequests = {};
}

//CREATE AJAX CALL

stark.services.supportRequests.create = function (data, onSuccess, onError) {
	var url = "/api/support-requests";

	var settings = {

		cache: false
		, contentType: "application/x-www-form-urlencoded; charset=UTF-8"
		, data: data
		, dataType: "json"
		, success: onSuccess
		, error: onError
		, type: "POST"
	};

	$.ajax(url, settings);
}

//UPDATE AJAX CALL

stark.services.supportRequests.update = function (id, data, onSuccess, onError) {
	var url = "/api/support-requests/" + id;

	var settings = {

		cache: false
		, contentType: "application/x-www-form-urlencoded; charset=UTF-8"
		, data: data
		, dataType: "json"
		, success: onSuccess
		, error: onError
		, type: "PUT"
	};

	$.ajax(url, settings);
}

//GET ALL AJAX CALL

stark.services.supportRequests.getAll = function (onSuccess, onError) {
	var url = "/api/support-requests";

	var settings = {

		cache: false
		, contentType: "application/x-www-form-urlencoded; charset=UTF-8"
		//, data: data
		, dataType: "json"
		, success: onSuccess
		, error: onError
		, type: "GET"
	};

	$.ajax(url, settings);
}

//GET BY ID AJAX CALL

stark.services.supportRequests.getById = function (id, onSuccess, onError) {
	var url = "/api/support-requests/" + id;

	var settings = {

		cache: false
		, contentType: "application/x-www-form-urlencoded; charset=UTF-8"
		//, data: data
		, dataType: "json"
		, success: onSuccess
		, error: onError
		, type: "GET"
	};

	$.ajax(url, settings);
}

//UPDATE ADMIN AJAX CALL

stark.services.supportRequests.updateResponse = function (id, data, onSuccess, onError) {
    var url = "/api/support-requests/" + id + "/response";

    var settings = {

        cache: false
		, contentType: "application/x-www-form-urlencoded; charset=UTF-8"
		, data: data
		, dataType: "json"
		, success: onSuccess
		, error: onError
		, type: "PUT"
    };

    $.ajax(url, settings);
}