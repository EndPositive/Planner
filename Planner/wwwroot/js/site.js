// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.
function ajax(controller, action, type, values = {}, success = function () { window.location.href = "/" + controller; }) {
    $.ajax({
        url: "/" + controller + "/" + action,
        type: type,
        data: values,
        success: success,
        error: function (res) {
            if (res.responseText == "overlap") {
                alert("Overlapping times during these hours.");
            } else if (res.responseText == "bad_times") {
                alert("Incorrect times requested.");
            }
        }
    });
}

function createAvailability() {
    var values = {
        Date: $("input[name=Date]").val(),
        StartTime: $("input[name=StartTime").val(),
        EndTime: $("input[name=EndTime]").val(),
    };
    if ($('#enableSeries').is(':checked')) {
        if ($('input[name=pattern]:checked').val() == "daily") {
            if ($('input[name=dailyPattern]:checked').val() == "every") {
                values.Pattern = $("input[name=everyDays]").val();
                values.Range = $("input[name=range]").val();
                ajax("Availabilities", "CreateDaily", "POST", values);
            } else if ($('input[name=dailyPattern]:checked').val() == "weekday") {
                values.Range = $("input[name=range]").val();
                ajax("Availabilities", "createDailyWeekdays", "POST", values);
            }
        } else if ($('input[name=pattern]:checked').val() == "weekly") {
            var days = $("input[name=everyWeeksDays]:checked").map(function () {
                return $(this).val();
            }).get();
            values.Pattern = $("input[name=everyWeeks]").val();
            values.Range = $("input[name=range]").val();
            values.Days = days;

            ajax("Availabilities", "CreateWeekly", "POST", values);
        }
    } else {
        ajax("Availabilities", "Create", "POST", values);
    }

    return false;
}

function editAvailability() {
    var values = {
        StartTime: $("input[name=StartTime").val(),
        EndTime: $("input[name=EndTime]").val(),
    };
    if ($('#enableSeries').is(':checked')) {
        var series = $("input[name=Series]").val();
        ajax("Availabilities", "EditSeries/" + series, "POST", values);
    } else {
        var id = $("input[name=Id]").val();
        values.Date = $("input[name=Date]").val();
        ajax("Availabilities", "Edit/" + id, "POST", values);
    }

    return false;
}

function deleteAvailability() {
    if ($('#enableSeries').is(':checked')) {
        var series = $("input[name=Series]").val();
        ajax("Availabilities", "DeleteSeries/" + series, "POST");
    } else {
        var id = $("input[name=Id]").val();
        ajax("Availabilities", "Delete/" + id, "POST");
    }

    return false;
}

function createShift() {
    var values = {
        Title: $("input[name=Title]").val(),
        Details: $("input[name=Details]").val(),
        Date: $("input[name=Date]").val(),
        StartTime: $("input[name=StartTime").val(),
        EndTime: $("input[name=EndTime]").val(),
    };
    if ($('#enableSeries').is(':checked')) {
        if ($('input[name=pattern]:checked').val() == "daily") {
            if ($('input[name=dailyPattern]:checked').val() == "every") {
                values.Pattern = $("input[name=everyDays]").val();
                values.Range = $("input[name=range]").val();
                ajax("Shifts", "CreateDaily", "POST", values);
            } else if ($('input[name=dailyPattern]:checked').val() == "weekday") {
                values.Range = $("input[name=range]").val();
                ajax("Shifts", "createDailyWeekdays", "POST", values);
            }
        } else if ($('input[name=pattern]:checked').val() == "weekly") {
            var days = $("input[name=everyWeeksDays]:checked").map(function () {
                return $(this).val();
            }).get();
            values.Pattern = $("input[name=everyWeeks]").val();
            values.Range = $("input[name=range]").val();
            values.Days = days;

            ajax("Shifts", "CreateWeekly", "POST", values);
        }
    } else {
        ajax("Shifts", "Create", "POST", values);
    }

    return false;
}

function deleteShift() {
    if ($('#enableSeries').is(':checked')) {
        var series = $("input[name=Series]").val();
        ajax("Shifts", "DeleteSeries/" + series, "POST");
    } else {
        var id = $("input[name=Id]").val();
        ajax("Shifts", "Delete/" + id, "POST");
    }

    return false;
}

function openAssignWindow(el) {
    // Create a new assignWindow el
    shiftId = el.parentNode.getAttribute("attr-id");

    if ($("#assignWindow").length) assignWindow.remove();

    assignWindow = document.createElement("div");
    assignWindow.setAttribute("id", "assignWindow");
    assignWindow.setAttribute("class", "container");
    assignWindow.setAttribute("attr-id", shiftId);

    document.getElementsByTagName("BODY")[0].appendChild(assignWindow);

    // Request availabilities
    ajax("Shifts", "GetAvailabilitiesForShift", "GET", { ShiftId: shiftId }, function (data) {
        var assignWindow = $("#assignWindow");
        var shiftId = assignWindow.attr("attr-id");

        assignWindow.html(assignWindow.html() + "<h4>Available users</h4><a href=''>Back</a><br />");
        assignWindow.html(assignWindow.html() + "<p style='margin-bottom: 0;'>Select an available user: </p>");

        var thead = "<thead><tr><th>User</th><th>Time</th><th>Til date</th><th></th><th></th></thead>";
        var tbody = "<tbody>";

        var res = JSON.parse(data);

        for (var i = 0; i < res.length; i++) {
            var availability = res[i].availability
            tbody += "<tr><td>" + availability.Username + "</td><td>" + availability.StartTime.slice(0, -3) + " - " + availability.EndTime.slice(0, -3) + "</td><td>" + res[i].availableTilDate + "</td><td onclick='assign(" + availability.Id + "," + shiftId + ")'>Assign</td><td onclick='assignRecurringly(" + availability.Id + "," + shiftId + "," + res[i].availableWeeks + ")'>Assign Recurringly</td></tr>"
        }

        tbody += "</tbody>";
        var table = "<table>" + thead + tbody + "</table>";

        assignWindow.html(assignWindow.html() + table);

        // Request user list
        ajax("Users", "Index", "Get", {}, function (data) {
            var assignWindow = $("#assignWindow");
            assignWindow.html(assignWindow.html() + "<p style='margin-bottom: 0;'>Select any user: </p>");
            assignWindow.html(assignWindow.html() + "<select id='selectUser' onchange='selectUser()'></select>");

            var select = $("#selectUser");
            select.html(select.html() + "<option selected disabled>Select user</option>");

            var res = JSON.parse(data);
            for (var i = 0; i < res.length; i++) {
                select.html(select.html() + "<option value='" + res[i].UserName + "'>" + res[i].UserName + "</option>");
            }
        });
    });
}

function assign(availabilityId, shiftId) {
    ajax("Shifts", "Assign", "POST", {
        shiftId: shiftId,
        availabilityId: availabilityId
    });
}

function selectUser() {
    var shiftId = $("#assignWindow").attr("attr-id");
    var user = $("#selectUser").val();

    ajax("Shifts", "Assign", "POST", {
        shiftId: shiftId,
        assign: user
    });
}

function assignRecurringly(availabilityId, shiftId, availableWeeks) {
    ajax("Shifts", "AssignRecurringly", "POST", {
        shiftId: shiftId,
        availabilityId: availabilityId,
        availableWeeks: availableWeeks
    });
}

function unAssign(shiftId) {
    ajax("Shifts", "UnAssign", "POST", {
        shiftId: shiftId
    });
}
